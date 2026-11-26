using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CzechServiceCenter
{
    /// <summary>
    /// Online card payment (Visa / Mastercard / Meeza).
    /// The card number is typed on the gateway's own page, never on this site, so the site stays out of PCI card-data scope.
    /// PaymentMode = Test    : built-in simulated payment page (no money moves). Use while developing.
    /// PaymentMode = Paymob  : Paymob Accept hosted iframe + HMAC-verified callback (see Web.config keys).
    /// InstaPay has no merchant API here: the customer transfers to the center's InstaPay address and sends the reference, the admin confirms.
    /// </summary>
    public static class Payments
    {
        static string S(string key) { return (ConfigurationManager.AppSettings[key] ?? "").Trim(); }

        public static string Mode { get { return string.Equals(S("PaymentMode"), "Paymob", StringComparison.OrdinalIgnoreCase) ? "Paymob" : "Test"; } }
        public static bool IsTest { get { return Mode == "Test"; } }
        public static string InstaPayHandle { get { return S("InstaPayHandle"); } }
        public static string InstaPayName { get { return S("InstaPayAccountName"); } }
        public static string InstaPayPhone { get { return S("InstaPayPhone"); } }

        /// <summary>Absolute URL of a page of this site (used for the gateway return links).</summary>
        static string Abs(string appRelative)
        {
            string baseUrl = S("SiteUrl");
            if (baseUrl.Length == 0)
            {
                var u = System.Web.HttpContext.Current.Request.Url;
                baseUrl = u.GetLeftPart(UriPartial.Authority) + System.Web.VirtualPathUtility.ToAbsolute("~/").TrimEnd('/');
            }
            return baseUrl.TrimEnd('/') + "/" + appRelative.TrimStart('~', '/');
        }

        /// <summary>Starts a card payment for the order and returns where to send the customer. Null + error when the gateway call fails.</summary>
        public static string StartCard(DataRow order, out string error)
        {
            error = null;
            string token = Convert.ToString(order["PublicToken"]);
            if (IsTest) return "~/PayTest.aspx?o=" + token;
            try
            {
                int orderId = Convert.ToInt32(order["OrderId"]);
                long cents = (long)Math.Round(Convert.ToDecimal(order["Total"]) * 100m);

                string auth = (string)Post("https://accept.paymob.com/api/auth/tokens", new JObject { { "api_key", S("PaymobApiKey") } })["token"];

                string merchantRef = orderId + "-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();   // must be unique per gateway order, so a retry gets a new one
                var gwOrder = Post("https://accept.paymob.com/api/ecommerce/orders", new JObject {
                    { "auth_token", auth }, { "delivery_needed", "false" }, { "amount_cents", cents.ToString() },
                    { "currency", "EGP" }, { "merchant_order_id", merchantRef }, { "items", new JArray() } });
                string gwOrderId = Convert.ToString((object)gwOrder["id"]);

                string name = Convert.ToString(order["CustomerName"]).Trim();
                int sp = name.IndexOf(' ');
                string first = sp > 0 ? name.Substring(0, sp) : name, last = sp > 0 ? name.Substring(sp + 1) : "NA";
                var billing = new JObject {
                    { "first_name", first }, { "last_name", last },
                    { "email", order["Email"] is DBNull ? "customer@example.com" : Convert.ToString(order["Email"]) },
                    { "phone_number", Convert.ToString(order["Phone"]) },
                    { "city", order["City"] is DBNull ? "NA" : Convert.ToString(order["City"]) },
                    { "street", order["Address"] is DBNull ? "NA" : Convert.ToString(order["Address"]) },
                    { "apartment", "NA" }, { "floor", "NA" }, { "building", "NA" }, { "shipping_method", "NA" },
                    { "postal_code", "NA" }, { "country", "EG" }, { "state", "NA" } };
                var key = Post("https://accept.paymob.com/api/acceptance/payment_keys", new JObject {
                    { "auth_token", auth }, { "amount_cents", cents.ToString() },
                    { "expiration", Orders.CardHoldMinutes * 60 }, { "order_id", gwOrderId },
                    { "billing_data", billing }, { "currency", "EGP" }, { "integration_id", int.Parse(S("PaymobIntegrationId")) } });

                Db.Exec("UPDATE Orders SET GatewayOrderId=@g WHERE OrderId=@o", Db.P("@g", gwOrderId), Db.P("@o", orderId));
                return "https://accept.paymob.com/api/acceptance/iframes/" + S("PaymobIframeId") + "?payment_token=" + Uri.EscapeDataString((string)key["token"]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Paymob start failed: " + ex);
                error = "The card payment service is not available right now. Please try again in a moment or pay by InstaPay.";
                return null;
            }
        }

        static JObject Post(string url, JObject body)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST"; req.ContentType = "application/json"; req.Timeout = 20000;
            byte[] data = Encoding.UTF8.GetBytes(body.ToString(Newtonsoft.Json.Formatting.None));
            using (var s = req.GetRequestStream()) s.Write(data, 0, data.Length);
            try
            {
                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var rd = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    return JObject.Parse(rd.ReadToEnd());
            }
            catch (WebException we)
            {
                string detail = "";
                if (we.Response != null) using (var rd = new StreamReader(we.Response.GetResponseStream())) detail = rd.ReadToEnd();
                throw new Exception("HTTP error calling " + url + ": " + detail, we);
            }
        }

        // ---- callback verification (Paymob: HMAC-SHA512 over the listed fields, in this order) ----
        static readonly string[] HmacFields = {
            "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction", "id", "integration_id",
            "is_3d_secure", "is_auth", "is_capture", "is_refunded", "is_standalone_payment", "is_voided", "order.id",
            "owner", "pending", "source_data.pan", "source_data.sub_type", "source_data.type", "success" };

        static string HmacHex(string data)
        {
            using (var h = new HMACSHA512(Encoding.UTF8.GetBytes(S("PaymobHmac"))))
            {
                var b = h.ComputeHash(Encoding.UTF8.GetBytes(data));
                var sb = new StringBuilder();
                foreach (byte x in b) sb.Append(x.ToString("x2"));
                return sb.ToString();
            }
        }

        static bool SameHex(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= char.ToLowerInvariant(a[i]) ^ char.ToLowerInvariant(b[i]);
            return diff == 0;
        }

        static string Val(JToken t)
        {
            if (t == null || t.Type == JTokenType.Null) return "";
            if (t.Type == JTokenType.Boolean) return (bool)t ? "true" : "false";
            return Convert.ToString(((JValue)t).Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>Webhook (POST, JSON body with "obj", hmac in the query string). Returns the verified transaction object or null.</summary>
        public static JObject VerifyWebhook(string body, string hmac)
        {
            if (S("PaymobHmac").Length == 0) return null;
            JObject root;
            try { root = JObject.Parse(body); } catch { return null; }
            var obj = root["obj"] as JObject;
            if (obj == null) return null;
            var sb = new StringBuilder();
            foreach (string f in HmacFields) sb.Append(Val(obj.SelectToken(f)));
            return SameHex(HmacHex(sb.ToString()), hmac) ? obj : null;
        }

        /// <summary>Browser return (GET, fields in the query string). Returns a JObject shaped like the webhook object, or null when the hmac is wrong.</summary>
        public static JObject VerifyReturn(System.Collections.Specialized.NameValueCollection q)
        {
            if (S("PaymobHmac").Length == 0) return null;
            var sb = new StringBuilder();
            foreach (string f in HmacFields) sb.Append(f == "order.id" ? (q["order"] ?? "") : (q[f] ?? ""));
            if (!SameHex(HmacHex(sb.ToString()), q["hmac"])) return null;
            var o = new JObject();
            o["amount_cents"] = q["amount_cents"]; o["currency"] = q["currency"]; o["id"] = q["id"];
            o["success"] = q["success"] == "true"; o["pending"] = q["pending"] == "true";
            o["is_voided"] = q["is_voided"] == "true"; o["is_refunded"] = q["is_refunded"] == "true";
            o["order"] = new JObject { { "id", q["order"] }, { "merchant_order_id", q["merchant_order_id"] } };
            return o;
        }

        /// <summary>Applies a verified gateway result to our order. Returns the order token (to show the customer), or null when it does not match an order.</summary>
        public static string Apply(JObject tx)
        {
            string merchant = Val(tx.SelectToken("order.merchant_order_id"));
            int dash = merchant.IndexOf('-'); int orderId;
            if (!int.TryParse(dash > 0 ? merchant.Substring(0, dash) : merchant, out orderId)) return null;
            var o = Db.Row("SELECT OrderId, PublicToken, Total, GatewayOrderId FROM Orders WHERE OrderId=@o AND PaymentMethod='Card'", Db.P("@o", orderId));
            if (o == null) return null;
            // the gateway order must be the one we registered for this order
            string gwOrder = Val(tx.SelectToken("order.id"));
            if (!(o["GatewayOrderId"] is DBNull) && gwOrder.Length > 0 && Convert.ToString(o["GatewayOrderId"]) != gwOrder) return null;

            bool success = Val(tx["success"]) == "true" && Val(tx["pending"]) != "true" && Val(tx["is_voided"]) != "true" && Val(tx["is_refunded"]) != "true";
            if (success)
            {
                long cents; long.TryParse(Val(tx["amount_cents"]), out cents);
                long expected = (long)Math.Round(Convert.ToDecimal(o["Total"]) * 100m);
                if (cents != expected || Val(tx["currency"]) != "EGP")
                {
                    Db.Exec("UPDATE Orders SET Notes=LEFT(COALESCE(Notes+' | ','')+@n,500) WHERE OrderId=@o", Db.P("@n", "Gateway amount mismatch: got " + cents + " expected " + expected), Db.P("@o", orderId));
                    return Convert.ToString(o["PublicToken"]);
                }
                Orders.MarkPaid(orderId, Val(tx["id"]));
            }
            else if (Val(tx["pending"]) != "true") Orders.MarkFailed(orderId);
            return Convert.ToString(o["PublicToken"]);
        }
    }
}

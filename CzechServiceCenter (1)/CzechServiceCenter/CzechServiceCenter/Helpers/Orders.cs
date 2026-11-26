using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace CzechServiceCenter
{
    public class CheckoutInfo
    {
        public int? UserId;
        public string Name, Phone, Email, DeliveryMethod, City, Address, PaymentMethod;
        public int? BranchId;
    }

    /// <summary>Guest orders. Stock is taken when the order is created and given back if the order is cancelled or never paid.</summary>
    public static class Orders
    {
        public static decimal DeliveryFee
        {
            get { decimal v; return decimal.TryParse(ConfigurationManager.AppSettings["DeliveryFee"], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out v) ? v : 0m; }
        }
        static int Cfg(string key, int def) { int v; return (int.TryParse(ConfigurationManager.AppSettings[key], out v) && v > 0) ? v : def; }
        public static int CardHoldMinutes { get { return Cfg("CardHoldMinutes", 30); } }
        public static int InstaPayHoldHours { get { return Cfg("InstaPayHoldHours", 24); } }

        static string NewToken()
        {
            var b = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Last 10 digits of a phone number, used to match the phone typed on the tracking page.</summary>
        public static string PhoneKey(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s ?? "") if (c >= '0' && c <= '9') sb.Append(c);
            string d = sb.ToString();
            return d.Length > 10 ? d.Substring(d.Length - 10) : d;
        }

        /// <summary>Creates the order from the cart lines and takes the stock in one transaction. Returns the order id, 0 with an error message when stock ran out.</summary>
        public static int Create(CheckoutInfo info, DataTable lines, out string token, out string error)
        {
            token = NewToken(); error = null;
            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    decimal subtotal = 0;
                    var priced = new System.Collections.Generic.List<object[]>();
                    foreach (DataRow l in lines.Rows)
                    {
                        int partId = Convert.ToInt32(l["PartId"]), qty = Convert.ToInt32(l["Qty"]);
                        using (var upd = new SqlCommand("UPDATE Parts SET Quantity=Quantity-@q WHERE PartId=@p AND IsActive=1 AND Quantity>=@q", cn, tx))
                        {
                            upd.Parameters.AddWithValue("@q", qty); upd.Parameters.AddWithValue("@p", partId);
                            if (upd.ExecuteNonQuery() == 0)
                            {
                                tx.Rollback();
                                error = Lang.T("Sorry, \"{0}\" is no longer available in the quantity you asked for. Please update your cart.", Convert.ToString(l["Name"]));
                                return 0;
                            }
                        }
                        // the row is locked by our update, so the price we read is the price we charge
                        using (var sel = new SqlCommand("SELECT PartNumber, Name, Price FROM Parts WHERE PartId=@p", cn, tx))
                        {
                            sel.Parameters.AddWithValue("@p", partId);
                            using (var rd = sel.ExecuteReader())
                            {
                                rd.Read();
                                decimal price = rd.GetDecimal(2);
                                subtotal += price * qty;
                                priced.Add(new object[] { partId, rd.GetString(0), rd.GetString(1), price, qty });
                            }
                        }
                    }
                    decimal fee = info.DeliveryMethod == "Delivery" ? DeliveryFee : 0m;
                    int orderId;
                    using (var ins = new SqlCommand(
                        "INSERT INTO Orders(PublicToken, UserId, CustomerName, Phone, Email, DeliveryMethod, BranchId, City, Address, Subtotal, DeliveryFee, Total, PaymentMethod) " +
                        "OUTPUT INSERTED.OrderId VALUES(@t,@u,@n,@ph,@em,@dm,@br,@ci,@ad,@st,@fee,@tot,@pm)", cn, tx))
                    {
                        ins.Parameters.AddWithValue("@t", token);
                        ins.Parameters.AddWithValue("@u", (object)info.UserId ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@n", info.Name);
                        ins.Parameters.AddWithValue("@ph", info.Phone);
                        ins.Parameters.AddWithValue("@em", string.IsNullOrEmpty(info.Email) ? (object)DBNull.Value : info.Email);
                        ins.Parameters.AddWithValue("@dm", info.DeliveryMethod);
                        ins.Parameters.AddWithValue("@br", (object)info.BranchId ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@ci", string.IsNullOrEmpty(info.City) ? (object)DBNull.Value : info.City);
                        ins.Parameters.AddWithValue("@ad", string.IsNullOrEmpty(info.Address) ? (object)DBNull.Value : info.Address);
                        ins.Parameters.AddWithValue("@st", subtotal);
                        ins.Parameters.AddWithValue("@fee", fee);
                        ins.Parameters.AddWithValue("@tot", subtotal + fee);
                        ins.Parameters.AddWithValue("@pm", info.PaymentMethod);
                        orderId = (int)ins.ExecuteScalar();
                    }
                    foreach (var p in priced)
                    {
                        using (var it = new SqlCommand("INSERT INTO OrderItems(OrderId, PartId, PartNumber, Name, UnitPrice, Quantity) VALUES(@o,@p,@pn,@n,@u,@q)", cn, tx))
                        {
                            it.Parameters.AddWithValue("@o", orderId); it.Parameters.AddWithValue("@p", p[0]); it.Parameters.AddWithValue("@pn", p[1]);
                            it.Parameters.AddWithValue("@n", p[2]); it.Parameters.AddWithValue("@u", p[3]); it.Parameters.AddWithValue("@q", p[4]);
                            it.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return orderId;
                }
            }
        }

        public static DataRow ByToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != 32) return null;
            return Db.Row("SELECT o.*, " + Lang.Col("bn", "Name") + " AS BranchName FROM Orders o LEFT JOIN Branches bn ON bn.BranchId=o.BranchId WHERE o.PublicToken=@t", Db.P("@t", token));
        }

        public static DataTable Items(int orderId)
        {
            return Db.Table("SELECT PartNumber, Name, UnitPrice, Quantity, UnitPrice*Quantity AS LineTotal FROM OrderItems WHERE OrderId=@o ORDER BY ItemId", Db.P("@o", orderId));
        }

        /// <summary>Payment confirmed (gateway callback, test page or admin). Safe to call twice.</summary>
        public static bool MarkPaid(int orderId, string txnId)
        {
            int n = Db.Exec("UPDATE Orders SET PaymentStatus='Paid', PaidAt=SYSUTCDATETIME(), GatewayTxnId=COALESCE(@t, GatewayTxnId), " +
                "OrderStatus=CASE WHEN OrderStatus='New' THEN 'Processing' ELSE OrderStatus END " +
                "WHERE OrderId=@o AND OrderStatus<>'Cancelled' AND PaymentStatus IN ('Pending','AwaitingReview','Failed')",
                Db.P("@t", string.IsNullOrEmpty(txnId) ? null : txnId), Db.P("@o", orderId));
            if (n > 0) return true;
            // the order was cancelled/expired before the money arrived
            var o = Db.Row("SELECT OrderStatus, PaymentStatus FROM Orders WHERE OrderId=@o", Db.P("@o", orderId));
            if (o == null || Convert.ToString(o["PaymentStatus"]) == "Paid") return false;
            if (Convert.ToString(o["OrderStatus"]) == "Cancelled")
            {
                string err;
                bool stockOk = Reopen(orderId, out err);
                Db.Exec("UPDATE Orders SET PaymentStatus='Paid', PaidAt=SYSUTCDATETIME(), GatewayTxnId=COALESCE(@t, GatewayTxnId), OrderStatus=CASE WHEN OrderStatus='New' THEN 'Processing' ELSE OrderStatus END, " +
                    "Notes=LEFT(COALESCE(Notes+' | ','')+@note, 500) WHERE OrderId=@o",
                    Db.P("@t", string.IsNullOrEmpty(txnId) ? null : txnId), Db.P("@o", orderId),
                    Db.P("@note", stockOk ? "Paid after the hold expired; stock was re-taken." : "LATE PAYMENT after the hold expired and stock was NOT available: fulfil or refund manually."));
                return true;
            }
            return false;
        }

        public static void MarkFailed(int orderId)
        {
            Db.Exec("UPDATE Orders SET PaymentStatus='Failed' WHERE OrderId=@o AND PaymentStatus='Pending'", Db.P("@o", orderId));
        }

        /// <summary>Cancels an order that is not completed and gives the stock back. Returns false when it was already cancelled/completed.</summary>
        public static bool Cancel(int orderId, string note)
        {
            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    int n;
                    using (var u = new SqlCommand("UPDATE Orders SET OrderStatus='Cancelled', PaymentStatus=CASE WHEN PaymentStatus='Paid' THEN 'Paid' ELSE 'Cancelled' END, " +
                        "Notes=CASE WHEN @note IS NULL THEN Notes ELSE LEFT(COALESCE(Notes+' | ','')+@note, 500) END WHERE OrderId=@o AND OrderStatus NOT IN ('Cancelled','Completed')", cn, tx))
                    {
                        u.Parameters.AddWithValue("@o", orderId);
                        u.Parameters.AddWithValue("@note", string.IsNullOrEmpty(note) ? (object)DBNull.Value : note);
                        n = u.ExecuteNonQuery();
                    }
                    if (n == 0) { tx.Rollback(); return false; }
                    using (var s = new SqlCommand("UPDATE p SET p.Quantity=p.Quantity+i.Quantity FROM Parts p JOIN OrderItems i ON i.PartId=p.PartId WHERE i.OrderId=@o", cn, tx))
                    { s.Parameters.AddWithValue("@o", orderId); s.ExecuteNonQuery(); }
                    tx.Commit();
                    return true;
                }
            }
        }

        /// <summary>Re-opens a cancelled order that has not been paid, taking the stock again. Fails when some part is no longer in stock.</summary>
        public static bool Reopen(int orderId, out string error)
        {
            error = null;
            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    var items = new DataTable();
                    using (var da = new SqlDataAdapter(new SqlCommand("SELECT PartId, Quantity FROM OrderItems WHERE OrderId=" + orderId, cn, tx))) da.Fill(items);
                    foreach (DataRow r in items.Rows)
                    {
                        using (var u = new SqlCommand("UPDATE Parts SET Quantity=Quantity-@q WHERE PartId=@p AND Quantity>=@q", cn, tx))
                        {
                            u.Parameters.AddWithValue("@q", r["Quantity"]); u.Parameters.AddWithValue("@p", r["PartId"]);
                            if (u.ExecuteNonQuery() == 0) { tx.Rollback(); error = "Not enough stock to re-open this order."; return false; }
                        }
                    }
                    int n;
                    using (var u = new SqlCommand("UPDATE Orders SET OrderStatus='New', PaymentStatus=CASE WHEN PaymentStatus='Cancelled' THEN 'Pending' ELSE PaymentStatus END WHERE OrderId=@o AND OrderStatus='Cancelled'", cn, tx))
                    { u.Parameters.AddWithValue("@o", orderId); n = u.ExecuteNonQuery(); }
                    if (n == 0) { tx.Rollback(); error = "This order is not cancelled."; return false; }
                    tx.Commit();
                    return true;
                }
            }
        }

        /// <summary>Cancels unpaid orders whose hold time ran out (card: minutes, InstaPay without a reference: hours) and returns their stock.</summary>
        public static void ExpireStale()
        {
            try
            {
                var stale = Db.Table("SELECT OrderId FROM Orders WHERE OrderStatus='New' AND PaymentStatus IN ('Pending','Failed') AND " +
                    "((PaymentMethod='Card' AND CreatedAt < DATEADD(MINUTE, -@m, SYSUTCDATETIME())) OR (PaymentMethod='InstaPay' AND CreatedAt < DATEADD(HOUR, -@h, SYSUTCDATETIME())))",
                    Db.P("@m", CardHoldMinutes), Db.P("@h", InstaPayHoldHours));
                foreach (DataRow r in stale.Rows) Cancel(Convert.ToInt32(r["OrderId"]), "Unpaid: hold expired");
            }
            catch (SqlException) { /* tables not created yet */ }
        }

        public static string StatusCss(string s)
        {
            switch (s)
            {
                case "Paid": case "Completed": case "Ready": return "ok";
                case "Cancelled": case "Failed": return "bad";
                case "Processing": case "AwaitingReview": return "info";
                default: return "warn";
            }
        }
    }
}

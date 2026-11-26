<%@ WebHandler Language="C#" Class="CzechServiceCenter.PaymentCallbackHandler" %>
using System;
using System.IO;
using System.Web;

namespace CzechServiceCenter
{
    /// <summary>
    /// Paymob calls this address two ways, both verified with the HMAC secret before anything is changed:
    ///   POST (server to server, "transaction processed callback")  ->  marks the order paid / failed
    ///   GET  (customer's browser coming back, "transaction response callback")  ->  same, then shows the order page
    /// Set both URLs in the Paymob dashboard to https://YOUR-SITE/PaymentCallback.ashx
    /// </summary>
    public class PaymentCallbackHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext ctx)
        {
            if (ctx.Request.HttpMethod == "POST")
            {
                string body;
                using (var rd = new StreamReader(ctx.Request.InputStream)) body = rd.ReadToEnd();
                var obj = Payments.VerifyWebhook(body, ctx.Request.QueryString["hmac"]);
                if (obj == null) { ctx.Response.StatusCode = 401; ctx.Response.Write("invalid"); return; }
                Payments.Apply(obj);
                ctx.Response.Write("ok");
                return;
            }
            var tx = Payments.VerifyReturn(ctx.Request.QueryString);
            string token = tx == null ? null : Payments.Apply(tx);
            string target = token == null ? "~/Order.aspx" : "~/Order.aspx?o=" + token + "&r=1";
            ctx.Response.Redirect(VirtualPathUtility.ToAbsolute(target), false);
        }
    }
}

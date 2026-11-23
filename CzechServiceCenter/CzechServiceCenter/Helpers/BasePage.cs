using System;
using System.Web;
using System.Web.UI;

namespace CzechServiceCenter
{
    /// <summary>Base class for every page: CSRF check, form helpers, flash messages.</summary>
    public class BasePage : Page
    {
        protected string Message;
        protected string MessageKind = "ok";

        protected bool IsPost { get { return Request.HttpMethod == "POST"; } }
        protected string F(string key) { return (Request.Form[key] ?? "").Trim(); }
        protected string Q(string key) { return (Request.QueryString[key] ?? "").Trim(); }
        protected string Val(string key) { return HttpUtility.HtmlAttributeEncode(F(key)); }
        protected string CsrfField() { return Csrf.Field(Context); }

        protected override void OnInit(EventArgs e)
        {
            // language switch: any page accepts ?lang=ar or ?lang=en, stores the choice in a cookie and reloads without the parameter
            string wanted = (Request.QueryString["lang"] ?? "").Trim();
            if (!IsPost && (wanted == "ar" || wanted == "en"))
            {
                Lang.Set(wanted);
                var qs = HttpUtility.ParseQueryString(Request.Url.Query);
                qs.Remove("lang");
                Response.Redirect(Request.Url.AbsolutePath + (qs.Count > 0 ? "?" + qs.ToString() : ""), true);
            }
            if (Lang.IsAr) Response.Filter = new LangFilter(Response.Filter, Response.ContentEncoding);

            if (IsPost && !Csrf.IsValid(Context))
                throw new HttpException(400, "Invalid or expired form token. Go back, refresh the page and try again.");
            base.OnInit(e);
        }

        protected void Ok(string msg) { Message = Lang.T(msg); MessageKind = "ok"; }
        protected void Err(string msg) { Message = Lang.T(msg); MessageKind = "err"; }

        protected string Flash()
        {
            if (string.IsNullOrEmpty(Message)) return "";
            return "<div class=\"alert alert-" + MessageKind + "\" role=\"alert\">" + HttpUtility.HtmlEncode(Message) + "</div>";
        }

        protected void RequireLogin()
        {
            if (!Auth.IsAuthenticated)
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.RawUrl), true);
        }

        protected static object IntOrNull(string s)
        {
            int v;
            return (int.TryParse(s, out v) && v > 0) ? (object)v : null;
        }

        protected static bool TryDate(string s, out DateTime d)
        {
            return DateTime.TryParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out d);
        }

        /// <summary>Only allow redirects back into this site.</summary>
        protected static bool IsLocal(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");
        }
    }
}

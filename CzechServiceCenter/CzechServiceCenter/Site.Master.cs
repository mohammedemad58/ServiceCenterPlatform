using System;
using System.Configuration;
using System.Web;
using System.Web.UI;

namespace CzechServiceCenter
{
    public partial class SiteMaster : MasterPage
    {
        protected bool IsLoggedIn { get { return Auth.IsAuthenticated; } }
        protected bool IsAdmin { get { return Auth.IsAdmin; } }
        protected string Phone { get { return ConfigurationManager.AppSettings["CenterPhone"]; } }
        protected string Address { get { return ConfigurationManager.AppSettings["CenterAddress"]; } }

        /// <summary>Link of the English / Arabic button: the current page plus ?lang=other.</summary>
        protected string LangUrl
        {
            get
            {
                var qs = HttpUtility.ParseQueryString(Request.Url.Query);
                qs.Remove("lang");
                qs["lang"] = Lang.IsAr ? "en" : "ar";
                return Request.Url.AbsolutePath + "?" + qs.ToString();
            }
        }

        /// <summary>Returns " active" when the current page matches the menu item.</summary>
        protected string Active(string page)
        {
            string p = Request.AppRelativeCurrentExecutionFilePath;
            if (page == "Default.aspx") return (p == "~/" || p == "~/Default.aspx") ? " active" : "";
            return string.Equals(p, "~/" + page, StringComparison.OrdinalIgnoreCase) ? " active" : "";
        }
    }
}

using System;

namespace CzechServiceCenter
{
    public partial class LogoutPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Auth.SignOut();
            Session.Abandon();
            Response.Redirect("~/", true);
        }
    }
}

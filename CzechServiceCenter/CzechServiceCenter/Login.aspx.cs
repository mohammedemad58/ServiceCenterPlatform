using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class LoginPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPost) return;
            var u = Db.Row("SELECT UserId, FullName, Email, PasswordHash, Role FROM Users WHERE Email=@e", Db.P("@e", F("email")));
            if (u == null || !Passwords.Verify(F("password"), Convert.ToString(u["PasswordHash"])))
            {
                Err("Email or password is incorrect.");
                return;
            }
            string role = Convert.ToString(u["Role"]);
            Auth.SignIn(Convert.ToInt32(u["UserId"]), Convert.ToString(u["Email"]), Convert.ToString(u["FullName"]), role, F("remember") == "1");
            string back = Q("ReturnUrl");
            if (IsLocal(back)) Response.Redirect(back, true);
            Response.Redirect(role == "Admin" ? "~/Admin/Default.aspx" : "~/Account/Profile.aspx", true);
        }
    }
}

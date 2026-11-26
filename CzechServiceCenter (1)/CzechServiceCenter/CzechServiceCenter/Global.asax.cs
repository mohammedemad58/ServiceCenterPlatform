using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace CzechServiceCenter
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            try { EnsureAdmin(); } catch (Exception) { /* database not created yet: run Database/schema.sql */ }
        }

        /// <summary>Puts the role stored in the forms ticket on the principal so Web.config role rules work.</summary>
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var id = Context.User == null ? null : Context.User.Identity as FormsIdentity;
            if (id == null || id.Ticket == null) return;
            var parts = id.Ticket.UserData.Split(new[] { '|' }, 3);
            if (parts.Length >= 2) Context.User = new GenericPrincipal(id, new[] { parts[1] });
        }

        static void EnsureAdmin()
        {
            if (Db.Count("SELECT COUNT(*) FROM Users WHERE Role='Admin'") > 0) return;
            string email = ConfigurationManager.AppSettings["AdminEmail"];
            string pw = ConfigurationManager.AppSettings["AdminPassword"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw)) return;
            Db.Exec("INSERT INTO Users(FullName, Email, Phone, PasswordHash, Role) VALUES(N'Administrator', @e, NULL, @h, 'Admin')",
                Db.P("@e", email), Db.P("@h", Passwords.Hash(pw)));
        }
    }
}

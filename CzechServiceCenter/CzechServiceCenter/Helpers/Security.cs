using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;

namespace CzechServiceCenter
{
    /// <summary>PBKDF2 password hashing.</summary>
    public static class Passwords
    {
        const int Iterations = 100000;

        public static string Hash(string password)
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                hash = kdf.GetBytes(32);
            return "v1." + Iterations + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            var parts = stored.Split('.');
            if (parts.Length != 4 || parts[0] != "v1") return false;
            int iter;
            if (!int.TryParse(parts[1], out iter)) return false;
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            byte[] actual;
            using (var kdf = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256))
                actual = kdf.GetBytes(expected.Length);
            int diff = 0;
            for (int i = 0; i < expected.Length; i++) diff |= expected[i] ^ actual[i];
            return diff == 0;
        }
    }

    /// <summary>Forms authentication with the user id and role stored in the ticket.</summary>
    public static class Auth
    {
        public static void SignIn(int userId, string email, string fullName, string role, bool persist)
        {
            var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now,
                DateTime.Now.AddMinutes(persist ? 43200 : 60), persist,
                userId + "|" + role + "|" + fullName, FormsAuthentication.FormsCookiePath);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.HttpOnly = true;
            cookie.Secure = FormsAuthentication.RequireSSL;
            if (persist) cookie.Expires = ticket.Expiration;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
            var c = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            c.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(c);
        }

        static string[] Data()
        {
            var u = HttpContext.Current.User;
            var id = (u == null) ? null : u.Identity as FormsIdentity;
            if (id == null || id.Ticket == null) return null;
            return id.Ticket.UserData.Split(new[] { '|' }, 3);
        }

        public static bool IsAuthenticated { get { return Data() != null; } }
        public static int UserId
        {
            get { var d = Data(); int v; return (d != null && int.TryParse(d[0], out v)) ? v : 0; }
        }
        public static string Role { get { var d = Data(); return d != null && d.Length > 1 ? d[1] : ""; } }
        public static string FullName { get { var d = Data(); return d != null && d.Length > 2 ? d[2] : ""; } }
        public static string Email { get { return HttpContext.Current.User.Identity.Name; } }
        public static bool IsAdmin { get { return Role == "Admin"; } }
    }

    /// <summary>Double-submit-cookie CSRF protection for the plain HTML forms.</summary>
    public static class Csrf
    {
        const string CookieName = "csc_csrf";

        public static string Token(HttpContext c)
        {
            var cached = c.Items["csrf"] as string;
            if (cached != null) return cached;
            var ck = c.Request.Cookies[CookieName];
            string token;
            if (ck != null && ck.Value.Length >= 32) token = ck.Value;
            else
            {
                var b = new byte[16];
                using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
                token = BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
                var nc = new HttpCookie(CookieName, token);
                nc.HttpOnly = true;
                c.Response.Cookies.Add(nc);
            }
            c.Items["csrf"] = token;
            return token;
        }

        public static string Field(HttpContext c)
        {
            return "<input type=\"hidden\" name=\"__csrf\" value=\"" + Token(c) + "\" />";
        }

        public static bool IsValid(HttpContext c)
        {
            var ck = c.Request.Cookies[CookieName];
            var f = c.Request.Form["__csrf"];
            return ck != null && !string.IsNullOrEmpty(f) && ck.Value == f;
        }
    }
}

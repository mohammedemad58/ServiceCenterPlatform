using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace CzechServiceCenter
{
    public partial class RegisterPage : BasePage
    {
        static readonly Regex EmailRx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPost) return;
            string name = F("fullName"), email = F("email").ToLowerInvariant(), phone = F("phone"), pw = Request.Form["password"] ?? "";

            if (name.Length < 3) { Err("Please enter your full name."); return; }
            if (!EmailRx.IsMatch(email)) { Err("Please enter a valid email address."); return; }
            if (phone.Length < 7) { Err("Please enter a valid mobile number."); return; }
            if (pw.Length < 8) { Err("Password must be at least 8 characters."); return; }
            if (pw != (Request.Form["confirm"] ?? "")) { Err("The two passwords do not match."); return; }

            try
            {
                object id = Db.Scalar("INSERT INTO Users(FullName, Email, Phone, PasswordHash) OUTPUT INSERTED.UserId VALUES(@n, @e, @p, @h)",
                    Db.P("@n", name), Db.P("@e", email), Db.P("@p", phone), Db.P("@h", Passwords.Hash(pw)));
                Auth.SignIn(Convert.ToInt32(id), email, name, "Customer", false);
                Response.Redirect("~/Account/Profile.aspx?welcome=1", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) Err("An account with this email already exists. Try logging in.");
                else throw;
            }
        }
    }
}

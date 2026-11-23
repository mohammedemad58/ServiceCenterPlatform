using System;
using System.Configuration;
using System.Data;

namespace CzechServiceCenter
{
    public partial class ContactPage : BasePage
    {
        protected string Phone { get { return ConfigurationManager.AppSettings["CenterPhone"]; } }
        protected string Address { get { return ConfigurationManager.AppSettings["CenterAddress"]; } }
        protected string Hours { get { return Slots.HoursText; } }
        protected DataTable Branches;

        protected void Page_Load(object sender, EventArgs e)
        {
            Branches = Db.Table("SELECT " + Lang.Col("", "Name") + " AS Name FROM Branches ORDER BY BranchId");
            if (!IsPost)
            {
                if (Q("sent") == "1") Ok("Thank you. Your message has been sent and we will get back to you soon.");
                return;
            }
            if (F("fullName").Length < 2 || F("email").Length < 5 || F("body").Length < 5) { Err("Please fill in your name, email and message."); return; }
            Db.Exec("INSERT INTO ContactMessages(FullName, Email, Phone, Body) VALUES(@n, @e, @p, @b)",
                Db.P("@n", F("fullName")), Db.P("@e", F("email")), Db.P("@p", F("phone").Length == 0 ? null : F("phone")), Db.P("@b", F("body")));
            Response.Redirect("~/Contact.aspx?sent=1", true);
        }
    }
}

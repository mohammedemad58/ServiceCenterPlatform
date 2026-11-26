using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class CheckoutPage : BasePage
    {
        protected DataTable Lines;
        protected decimal Subtotal;
        DataRow me;

        protected void Page_Load(object sender, EventArgs e)
        {
            Lines = Cart.Lines();
            if (Lines.Rows.Count == 0) Response.Redirect("~/Cart.aspx", true);
            Subtotal = Cart.Subtotal(Lines);
            if (Auth.IsAuthenticated) me = Db.Row("SELECT FullName, Email, Phone FROM Users WHERE UserId=@u", Db.P("@u", Auth.UserId));
            if (IsPost) Submit();
        }

        protected decimal Fee { get { return Orders.DeliveryFee; } }

        /// <summary>Posted value, or the logged-in customer's saved detail the first time the form is shown.</summary>
        protected string Field(string key, string userColumn)
        {
            if (IsPost) return HttpUtility_Encode(F(key));
            if (me != null && userColumn != null && !(me[userColumn] is DBNull)) return HttpUtility_Encode(Convert.ToString(me[userColumn]));
            return "";
        }

        static string HttpUtility_Encode(string s) { return System.Web.HttpUtility.HtmlAttributeEncode(s); }
        protected string Chosen(string key, string value, string def) { string cur = IsPost ? F(key) : def; return cur == value ? " checked" : ""; }

        void Submit()
        {
            foreach (DataRow r in Lines.Rows)
                if (Convert.ToInt32(r["Qty"]) > Convert.ToInt32(r["Stock"])) { Err("Some items are no longer available in the quantity you chose. Please update your cart."); return; }

            string name = F("name"), phone = F("phone"), email = F("email"), method = F("delivery"), pay = F("pay");
            string city = F("city"), address = F("address");
            int branch = 0;
            if (name.Length < 3) { Err("Please enter your full name."); return; }
            int digits = Orders.PhoneKey(phone).Length;
            if (digits < 8 || phone.Length > 30) { Err("Please enter a valid phone number."); return; }
            if (email.Length > 0 && (email.Length > 150 || email.IndexOf('@') < 1 || email.IndexOf('.', email.IndexOf('@')) < 0 || email.Contains(" "))) { Err("Please enter a valid email address or leave it empty."); return; }
            if (method != "Pickup" && method != "Delivery") { Err("Please choose pickup or delivery."); return; }
            if (pay != "Card" && pay != "InstaPay") { Err("Please choose how you want to pay."); return; }
            if (method == "Pickup")
            {
                if (!int.TryParse(F("branch"), out branch) || Db.Count("SELECT COUNT(*) FROM Branches WHERE BranchId=@b", Db.P("@b", branch)) == 0) { Err("Please choose the branch where you will pick up."); return; }
            }
            else if (city.Length < 2 || address.Length < 6 || city.Length > 80 || address.Length > 300) { Err("Please enter your city and full delivery address."); return; }

            var info = new CheckoutInfo
            {
                UserId = Auth.IsAuthenticated ? (int?)Auth.UserId : null,
                Name = name, Phone = phone, Email = email, DeliveryMethod = method, PaymentMethod = pay,
                BranchId = method == "Pickup" ? (int?)branch : null,
                City = method == "Delivery" ? city : null, Address = method == "Delivery" ? address : null
            };
            string token, err;
            int id = Orders.Create(info, Lines, out token, out err);
            if (id == 0) { Err(err); return; }
            Cart.Clear();

            if (pay == "Card")
            {
                string perr;
                string url = Payments.StartCard(Orders.ByToken(token), out perr);
                if (url != null) Response.Redirect(url, true);
                Response.Redirect("~/Order.aspx?o=" + token + "&payfail=1", true);
            }
            Response.Redirect("~/Order.aspx?o=" + token, true);
        }
    }
}

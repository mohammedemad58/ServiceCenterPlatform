using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class OrderPage : BasePage
    {
        protected DataRow O;
        protected DataTable Items;
        protected bool Lookup;
        protected bool AutoRefresh;

        protected void Page_Load(object sender, EventArgs e)
        {
            Orders.ExpireStale();
            if (IsPost) Handle();
            if (!IsPost && Q("sent") == "1") Ok("Thank you. We received your transfer reference and will confirm your payment shortly.");
            if (!IsPost && Q("payfail") == "1") Err("The card payment was not completed. You can try again below, or pay by InstaPay.");
            Load();
        }

        void Load()
        {
            string t = Q("o");
            if (t.Length == 0) { Lookup = true; return; }
            O = Orders.ByToken(t);
            if (O == null) { Lookup = true; if (string.IsNullOrEmpty(Message)) Err("We could not find this order."); return; }
            Items = Orders.Items(Convert.ToInt32(O["OrderId"]));
            // just back from the card page and the confirmation has not arrived yet: refresh for a short while
            if (Q("r") == "1" && Pay == "Card" && PayStatus == "Pending" &&
                (DateTime.UtcNow - Convert.ToDateTime(O["CreatedAt"])).TotalMinutes < Orders.CardHoldMinutes) AutoRefresh = true;
        }

        protected string Pay { get { return Convert.ToString(O["PaymentMethod"]); } }
        protected string PayStatus { get { return Convert.ToString(O["PaymentStatus"]); } }
        protected string OrdStatus { get { return Convert.ToString(O["OrderStatus"]); } }
        protected int HoldHours { get { return Orders.InstaPayHoldHours; } }
        protected int HoldMinutes { get { return Orders.CardHoldMinutes; } }
        protected string InstaPayHandle { get { return Payments.InstaPayHandle; } }
        protected string InstaPayName { get { return Payments.InstaPayName; } }
        protected string InstaPayPhone { get { return Payments.InstaPayPhone; } }

        void Handle()
        {
            string cmd = F("cmd");
            if (cmd == "find")
            {
                int id;
                var r = int.TryParse(F("number").TrimStart('#'), out id) ? Db.Row("SELECT PublicToken, Phone FROM Orders WHERE OrderId=@o", Db.P("@o", id)) : null;
                if (r == null || Orders.PhoneKey(Convert.ToString(r["Phone"])) != Orders.PhoneKey(F("phone")) || Orders.PhoneKey(F("phone")).Length < 8)
                { Err("We could not find an order with these details."); return; }
                Response.Redirect("~/Order.aspx?o=" + Convert.ToString(r["PublicToken"]), true);
            }
            else if (cmd == "instapay")
            {
                string reference = F("ref");
                if (reference.Length < 4 || reference.Length > 80) { Err("Please enter the InstaPay transaction reference number."); return; }
                int n = Db.Exec("UPDATE Orders SET PaymentStatus='AwaitingReview', InstaPayRef=@r WHERE PublicToken=@t AND PaymentMethod='InstaPay' AND PaymentStatus IN ('Pending','AwaitingReview') AND OrderStatus<>'Cancelled'",
                    Db.P("@r", reference), Db.P("@t", Q("o")));
                if (n == 0) { Err("This order can no longer accept a payment reference. Please contact us."); return; }
                Response.Redirect("~/Order.aspx?o=" + Q("o") + "&sent=1", true);
            }
            else if (cmd == "paycard")
            {
                var o = Orders.ByToken(Q("o"));
                if (o == null || Convert.ToString(o["PaymentMethod"]) != "Card" || (Convert.ToString(o["PaymentStatus"]) != "Pending" && Convert.ToString(o["PaymentStatus"]) != "Failed") || Convert.ToString(o["OrderStatus"]) == "Cancelled")
                { Err("This order cannot be paid any more."); return; }
                string perr;
                string url = Payments.StartCard(o, out perr);
                if (url == null) { Err(perr); return; }
                Db.Exec("UPDATE Orders SET PaymentStatus='Pending' WHERE OrderId=@o AND PaymentStatus='Failed'", Db.P("@o", o["OrderId"]));
                Response.Redirect(url, true);
            }
        }
    }
}

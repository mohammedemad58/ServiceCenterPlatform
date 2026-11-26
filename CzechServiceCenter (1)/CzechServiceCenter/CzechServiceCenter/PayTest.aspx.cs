using System;
using System.Data;

namespace CzechServiceCenter
{
    /// <summary>Simulated card page for PaymentMode=Test. It never asks for card numbers and is disabled when the real gateway is on.</summary>
    public partial class PayTestPage : BasePage
    {
        protected DataRow O;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Payments.IsTest) throw new System.Web.HttpException(404, "Not found");
            O = Orders.ByToken(Q("o"));
            if (O == null || Convert.ToString(O["PaymentMethod"]) != "Card") throw new System.Web.HttpException(404, "Not found");
            if (IsPost)
            {
                int id = Convert.ToInt32(O["OrderId"]);
                if (F("cmd") == "ok") { Orders.MarkPaid(id, "TEST-" + id); Response.Redirect("~/Order.aspx?o=" + Q("o"), true); }
                Orders.MarkFailed(id);
                Response.Redirect("~/Order.aspx?o=" + Q("o") + "&payfail=1", true);
            }
        }
    }
}

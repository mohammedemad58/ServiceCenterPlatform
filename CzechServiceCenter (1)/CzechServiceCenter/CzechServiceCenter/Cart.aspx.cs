using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class CartPage : BasePage
    {
        protected DataTable Lines;
        protected decimal Subtotal;
        protected bool StockProblem;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Handle();
            Lines = Cart.Lines();
            Subtotal = Cart.Subtotal(Lines);
            foreach (DataRow r in Lines.Rows) if (Convert.ToInt32(r["Qty"]) > Convert.ToInt32(r["Stock"])) StockProblem = true;
        }

        void Handle()
        {
            string cmd = F("cmd");
            if (cmd == "update")
            {
                foreach (string key in Request.Form.AllKeys)
                {
                    int id, q;
                    if (key != null && key.StartsWith("qty_") && int.TryParse(key.Substring(4), out id) && int.TryParse(Request.Form[key], out q)) Cart.Set(id, q);
                }
                Ok("Cart updated.");
            }
            else if (cmd.StartsWith("remove:"))
            {
                int id; if (int.TryParse(cmd.Substring(7), out id)) Cart.Remove(id);
                Ok("Item removed.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CzechServiceCenter
{
    public partial class AdminOrders : BasePage
    {
        protected DataTable Rows, ItemRows;

        protected void Page_Load(object sender, EventArgs e)
        {
            Orders.ExpireStale();
            if (IsPost) Handle();
            Load();
        }

        void Load()
        {
            var where = new StringBuilder("WHERE 1=1");
            var ps = new List<SqlParameter>();
            if (Q("pay").Length > 0) { where.Append(" AND o.PaymentStatus=@pay"); ps.Add(Db.P("@pay", Q("pay"))); }
            if (Q("status").Length > 0) { where.Append(" AND o.OrderStatus=@st"); ps.Add(Db.P("@st", Q("status"))); }
            if (Q("q").Length > 0)
            {
                where.Append(" AND (o.CustomerName LIKE @q OR o.Phone LIKE @q OR CAST(o.OrderId AS NVARCHAR(12))=@exact)");
                ps.Add(Db.P("@q", "%" + Q("q").Replace("[", "[[]").Replace("%", "[%]") + "%"));
                ps.Add(Db.P("@exact", Q("q").TrimStart('#')));
            }
            Rows = Db.Table("SELECT TOP 100 o.*, bn.Name AS BranchName FROM Orders o LEFT JOIN Branches bn ON bn.BranchId=o.BranchId " + where + " ORDER BY o.OrderId DESC", ps.ToArray());
            ItemRows = Db.Table("SELECT i.OrderId, i.PartNumber, i.Name, i.Quantity, i.UnitPrice FROM OrderItems i WHERE i.OrderId IN (SELECT TOP 100 o.OrderId FROM Orders o " + where + " ORDER BY o.OrderId DESC) ORDER BY i.ItemId",
                Clone(ps));
        }

        static SqlParameter[] Clone(List<SqlParameter> ps)
        {
            var a = new SqlParameter[ps.Count];
            for (int i = 0; i < a.Length; i++) a[i] = new SqlParameter(ps[i].ParameterName, ps[i].Value);
            return a;
        }

        protected DataRow[] ItemsOf(object orderId)
        {
            return ItemRows.Select("OrderId=" + Convert.ToInt32(orderId));
        }

        void Handle()
        {
            string cmd = F("cmd");
            int i = cmd.IndexOf(':'); int id;
            if (i < 0 || !int.TryParse(cmd.Substring(i + 1), out id)) return;
            string what = cmd.Substring(0, i);
            if (what == "markpaid")
            {
                if (Orders.MarkPaid(id, null)) Ok("Payment confirmed. The order is now in processing."); else Err("This order could not be marked as paid (already paid?).");
            }
            else if (what == "status")
            {
                string s = F("status_" + id);
                if (Array.IndexOf(new[] { "Processing", "Ready", "Completed" }, s) < 0) { Err("Choose a valid status."); return; }
                int n = Db.Exec("UPDATE Orders SET OrderStatus=@s WHERE OrderId=@o AND PaymentStatus='Paid' AND OrderStatus<>'Cancelled'", Db.P("@s", s), Db.P("@o", id));
                if (n == 0) Err("Only paid orders can move to processing, ready or completed."); else Ok("Order status updated.");
            }
            else if (what == "cancel")
            {
                if (Orders.Cancel(id, "Cancelled by admin")) Ok("Order cancelled and the stock was returned. If the customer already paid, refund them."); else Err("This order is already cancelled or completed.");
            }
            else if (what == "reopen")
            {
                string err;
                if (Orders.Reopen(id, out err)) Ok("Order re-opened and the stock was taken again."); else Err(err);
            }
        }
    }
}

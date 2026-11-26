using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    /// <summary>Shopping cart kept in the session (no login needed). Only part ids and quantities are stored: prices always come from the database.</summary>
    public static class Cart
    {
        const string Key = "csc_cart";
        public const int MaxPerLine = 20;

        static Dictionary<int, int> Items(bool create)
        {
            var ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null) return null;
            var d = ctx.Session[Key] as Dictionary<int, int>;
            if (d == null && create) { d = new Dictionary<int, int>(); ctx.Session[Key] = d; }
            return d;
        }

        public static int Count
        {
            get
            {
                var d = Items(false);
                if (d == null) return 0;
                int n = 0;
                foreach (var kv in d) n += kv.Value;
                return n;
            }
        }

        public static void Add(int partId, int qty)
        {
            if (partId <= 0 || qty <= 0) return;
            var d = Items(true);
            int cur; d.TryGetValue(partId, out cur);
            d[partId] = Math.Min(MaxPerLine, cur + qty);
        }

        public static void Set(int partId, int qty)
        {
            var d = Items(true);
            if (qty <= 0) d.Remove(partId); else d[partId] = Math.Min(MaxPerLine, qty);
        }

        public static void Remove(int partId) { var d = Items(false); if (d != null) d.Remove(partId); }
        public static void Clear() { var d = Items(false); if (d != null) d.Clear(); }

        /// <summary>Cart lines with current price and stock. Lines whose part is gone or inactive are dropped.
        /// Columns: PartId, PartNumber, Name, BrandName, Price, Stock, ImageUrl, Qty, LineTotal.</summary>
        public static DataTable Lines()
        {
            var d = Items(false);
            var empty = new DataTable();
            if (d == null || d.Count == 0) return Build(empty, d);
            var ps = new List<System.Data.SqlClient.SqlParameter>();
            var sb = new StringBuilder();
            int i = 0;
            foreach (var id in d.Keys)
            {
                if (i > 0) sb.Append(',');
                sb.Append("@i" + i);
                ps.Add(Db.P("@i" + i, id));
                i++;
            }
            var dt = Db.Table("SELECT p.PartId, p.PartNumber, " + Lang.Col("p", "Name") + " AS Name, " + Lang.Col("b", "Name") + " AS BrandName, p.Price, p.Quantity AS Stock, p.ImageUrl " +
                "FROM Parts p JOIN Brands b ON b.BrandId=p.BrandId WHERE p.IsActive=1 AND p.PartId IN (" + sb + ") ORDER BY b.Name, p.Name", ps.ToArray());
            return Build(dt, d);
        }

        static DataTable Build(DataTable dt, Dictionary<int, int> d)
        {
            if (!dt.Columns.Contains("PartId")) { dt.Columns.Add("PartId", typeof(int)); dt.Columns.Add("PartNumber", typeof(string)); dt.Columns.Add("Name", typeof(string)); dt.Columns.Add("BrandName", typeof(string)); dt.Columns.Add("Price", typeof(decimal)); dt.Columns.Add("Stock", typeof(int)); dt.Columns.Add("ImageUrl", typeof(string)); }
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("LineTotal", typeof(decimal));
            foreach (DataRow r in dt.Rows)
            {
                int q = d[Convert.ToInt32(r["PartId"])];
                r["Qty"] = q;
                r["LineTotal"] = Convert.ToDecimal(r["Price"]) * q;
            }
            // forget parts that no longer exist
            if (d != null)
            {
                var keep = new HashSet<int>();
                foreach (DataRow r in dt.Rows) keep.Add(Convert.ToInt32(r["PartId"]));
                foreach (var k in new List<int>(d.Keys)) if (!keep.Contains(k)) d.Remove(k);
            }
            return dt;
        }

        public static decimal Subtotal(DataTable lines)
        {
            decimal t = 0;
            foreach (DataRow r in lines.Rows) t += Convert.ToDecimal(r["LineTotal"]);
            return t;
        }
    }
}

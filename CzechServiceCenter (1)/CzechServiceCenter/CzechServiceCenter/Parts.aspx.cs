using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    public partial class PartsPage : BasePage
    {
        const int PageSize = 24;
        protected DataTable PartRows;
        protected int Total, PageNo = 1, Pages = 1;

        protected void Page_Load(object sender, EventArgs e)
        {
            Orders.ExpireStale();
            if (IsPost) AddToCart();
            if (!IsPost && Q("added") == "1") Ok("Added to your cart.");
            Search();
        }

        /// <summary>Guests can add parts: no login needed.</summary>
        void AddToCart()
        {
            int partId, qty;
            if (!int.TryParse(F("partId"), out partId) || !int.TryParse(F("qty"), out qty) || qty < 1 || qty > Cart.MaxPerLine)
            { Err("Please enter a valid quantity (1 to 20)."); return; }
            var p = Db.Row("SELECT Quantity FROM Parts WHERE PartId=@p AND IsActive=1", Db.P("@p", partId));
            if (p == null || Convert.ToInt32(p["Quantity"]) < 1) { Err("Sorry, this part is out of stock."); return; }
            Cart.Add(partId, Math.Min(qty, Convert.ToInt32(p["Quantity"])));
            var qs = HttpUtility.ParseQueryString(Request.Url.Query);
            qs["added"] = "1";
            Response.Redirect(Request.Url.AbsolutePath + "?" + qs, true);
        }

        void Search()
        {
            var where = new StringBuilder("WHERE p.IsActive=1");
            var ps = new List<SqlParameter>();
            string q = Q("q");
            if (q.Length > 0) { where.Append(" AND (p.Name LIKE @q OR p.NameAr LIKE @q OR p.PartNumber LIKE @q)"); ps.Add(Db.P("@q", "%" + q.Replace("[", "[[]").Replace("%", "[%]") + "%")); }
            int brand; if (int.TryParse(Q("brand"), out brand)) { where.Append(" AND p.BrandId=@b"); ps.Add(Db.P("@b", brand)); }
            if (Q("cat").Length > 0) { where.Append(" AND p.Category=@c"); ps.Add(Db.P("@c", Q("cat"))); }
            if (Q("stock") == "in") where.Append(" AND p.Quantity>0");
            if (Q("stock") == "low") where.Append(" AND p.Quantity BETWEEN 1 AND 5");

            // part types from the service schedule page: ?types=OIL,AIR
            var codes = new List<string>();
            foreach (string c in Q("types").Split(','))
            {
                string t = c.Trim().ToUpperInvariant();
                if (t.Length >= 2 && t.Length <= 6 && IsLetters(t) && codes.Count < 15) codes.Add(t);
            }
            if (codes.Count > 0)
            {
                var names = new List<string>();
                for (int i = 0; i < codes.Count; i++) { names.Add("@t" + i); ps.Add(Db.P("@t" + i, codes[i])); }
                where.Append(" AND p.TypeCode IN (" + string.Join(",", names) + ")");
            }

            // vehicle finder: the part must have a fitment row for the chosen model (and year / engine when chosen)
            int model, year, cc;
            if (int.TryParse(Q("model"), out model))
            {
                var fit = new StringBuilder("EXISTS (SELECT 1 FROM PartFitment f WHERE f.PartId=p.PartId AND f.ModelId=@m");
                ps.Add(Db.P("@m", model));
                if (int.TryParse(Q("year"), out year)) { fit.Append(" AND @y BETWEEN f.YearFrom AND f.YearTo"); ps.Add(Db.P("@y", year)); }
                if (int.TryParse(Q("cc"), out cc)) { fit.Append(" AND (f.EngineCc IS NULL OR f.EngineCc=@cc)"); ps.Add(Db.P("@cc", cc)); }
                fit.Append(")");
                where.Append(" AND " + fit);
            }

            Total = Db.Count("SELECT COUNT(*) FROM Parts p " + where, Clone(ps));
            Pages = Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
            int pg; if (int.TryParse(Q("page"), out pg)) PageNo = Math.Min(Math.Max(1, pg), Pages);

            var p2 = Clone(ps);
            Array.Resize(ref p2, p2.Length + 2);
            p2[p2.Length - 2] = Db.P("@skip", (PageNo - 1) * PageSize);
            p2[p2.Length - 1] = Db.P("@take", PageSize);
            PartRows = Db.Table("SELECT p.PartId, p.PartNumber, " + Lang.Col("p", "Name") + " AS Name, p.Category, p.Price, p.Quantity, p.ImageUrl, " + Lang.Col("b", "Name") + " AS BrandName " +
                "FROM Parts p JOIN Brands b ON b.BrandId=p.BrandId " + where +
                " ORDER BY b.Name, p.Category, p.Name OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p2);
        }

        static bool IsLetters(string s) { foreach (char c in s) if (c < 'A' || c > 'Z') return false; return true; }

        static SqlParameter[] Clone(List<SqlParameter> ps)
        {
            var a = new SqlParameter[ps.Count];
            for (int i = 0; i < a.Length; i++) a[i] = new SqlParameter(ps[i].ParameterName, ps[i].Value);
            return a;
        }

        protected string PageUrl(int n)
        {
            return "Parts.aspx?q=" + HttpUtility.UrlEncode(Q("q")) + "&brand=" + HttpUtility.UrlEncode(Q("brand")) +
                   "&model=" + HttpUtility.UrlEncode(Q("model")) + "&year=" + HttpUtility.UrlEncode(Q("year")) + "&cc=" + HttpUtility.UrlEncode(Q("cc")) +
                   "&cat=" + HttpUtility.UrlEncode(Q("cat")) + "&stock=" + HttpUtility.UrlEncode(Q("stock")) + "&types=" + HttpUtility.UrlEncode(Q("types")) + "&page=" + n;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    public partial class AdminParts : BasePage
    {
        const int PageSize = 30;
        protected DataTable PartRows;
        protected DataRow Editing;
        protected bool ShowNew;
        protected int Total, PageNo = 1, Pages = 1;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Handle();
            else if (Q("saved") == "1") Ok("Part saved.");
            ShowNew = Q("new") == "1";
            int eid;
            if (int.TryParse(Q("edit"), out eid))
                Editing = Db.Row("SELECT PartId, PartNumber, Name, NameAr, BrandId, Category, Description, DescriptionAr, Price, Quantity, IsActive FROM Parts WHERE PartId=@i", Db.P("@i", eid));
            Search();
        }

        /// <summary>Form value after a failed post, otherwise the value of the part being edited.</summary>
        protected string Field(string formKey, string column)
        {
            if (IsPost) return F(formKey);
            if (Editing == null) return "";
            object v = Editing[column];
            if (v is DBNull) return "";
            if (v is decimal) return ((decimal)v).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            return Convert.ToString(v);
        }

        void Search()
        {
            var where = new StringBuilder("WHERE 1=1");
            var ps = new List<SqlParameter>();
            if (Q("q").Length > 0) { where.Append(" AND (p.Name LIKE @q OR p.NameAr LIKE @q OR p.PartNumber LIKE @q)"); ps.Add(Db.P("@q", "%" + Q("q").Replace("[", "[[]").Replace("%", "[%]") + "%")); }
            int brand; if (int.TryParse(Q("brand"), out brand)) { where.Append(" AND p.BrandId=@b"); ps.Add(Db.P("@b", brand)); }
            if (Q("stock") == "low") where.Append(" AND p.Quantity<=5");

            var countParams = ps.ConvertAll(p => new SqlParameter(p.ParameterName, p.Value)).ToArray();
            Total = Db.Count("SELECT COUNT(*) FROM Parts p " + where, countParams);
            Pages = Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
            int pg; if (int.TryParse(Q("page"), out pg)) PageNo = Math.Min(Math.Max(1, pg), Pages);
            ps.Add(Db.P("@skip", (PageNo - 1) * PageSize));
            ps.Add(Db.P("@take", PageSize));
            PartRows = Db.Table("SELECT p.PartId, p.PartNumber, " + Lang.Col("p", "Name") + " AS Name, p.Category, p.Price, p.Quantity, p.IsActive, " + Lang.Col("b", "Name") + " AS BrandName " +
                "FROM Parts p JOIN Brands b ON b.BrandId=p.BrandId " + where + " ORDER BY b.Name, p.Category, p.Name OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", ps.ToArray());
        }

        protected string PageUrl(int n)
        {
            return "Parts.aspx?q=" + HttpUtility.UrlEncode(Q("q")) + "&brand=" + HttpUtility.UrlEncode(Q("brand")) + "&stock=" + HttpUtility.UrlEncode(Q("stock")) + "&page=" + n;
        }

        void Handle()
        {
            string cmd = F("cmd");
            if (cmd == "save") { Save(); return; }
            int i = cmd.IndexOf(':'); int id;
            if (i < 0 || !int.TryParse(cmd.Substring(i + 1), out id)) return;

            if (cmd.StartsWith("stock:"))
            {
                int q;
                if (!int.TryParse(F("qty_" + id), out q) || q < 0) { Err("Enter a valid quantity."); return; }
                Db.Exec("UPDATE Parts SET Quantity=@q WHERE PartId=@i", Db.P("@q", q), Db.P("@i", id));
                Ok("Stock updated.");
            }
            else if (cmd.StartsWith("delete:"))
            {
                try { Db.Exec("DELETE FROM Parts WHERE PartId=@i", Db.P("@i", id)); Ok("Part deleted."); }
                catch (SqlException ex)
                {
                    if (ex.Number != 547) throw;
                    // part has reservations: keep the history, hide it from the catalog
                    Db.Exec("UPDATE Parts SET IsActive=0 WHERE PartId=@i", Db.P("@i", id));
                    Ok("This part has reservations, so it was hidden from the catalog instead of deleted.");
                }
            }
        }

        void Save()
        {
            int id, brand, qty; decimal price;
            int.TryParse(F("partId"), out id);
            if (!int.TryParse(F("brand"), out brand) || !int.TryParse(F("qty"), out qty) || qty < 0 ||
                !decimal.TryParse(F("price"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out price) || price < 0 ||
                F("number").Length == 0 || F("name").Length == 0 || F("category").Length == 0)
            { Err("Please fill in part number, name, brand, category, a valid price and quantity."); return; }

            string imageUrl = null;
            HttpPostedFile file = Request.Files["image"];
            if (file != null && file.ContentLength > 0)
            {
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (Array.IndexOf(new[] { ".jpg", ".jpeg", ".png", ".webp" }, ext) < 0) { Err("Only JPG, PNG or WebP images are allowed."); return; }
                if (file.ContentLength > 2 * 1024 * 1024) { Err("The image must be smaller than 2 MB."); return; }
                string dir = Server.MapPath("~/Content/parts/");
                Directory.CreateDirectory(dir);
                string fname = Guid.NewGuid().ToString("N") + ext;
                file.SaveAs(Path.Combine(dir, fname));
                imageUrl = "~/Content/parts/" + fname;
            }

            bool active = F("active") == "1";
            try
            {
                if (id == 0)
                    Db.Exec("INSERT INTO Parts(PartNumber, Name, NameAr, BrandId, Category, Description, DescriptionAr, Price, Quantity, ImageUrl, IsActive) VALUES(@n,@nm,@nma,@b,@c,@d,@da,@p,@q,@img,@a)",
                        Db.P("@nma", F("nameAr").Length == 0 ? null : F("nameAr")), Db.P("@da", F("descAr").Length == 0 ? null : F("descAr")),
                        Db.P("@n", F("number")), Db.P("@nm", F("name")), Db.P("@b", brand), Db.P("@c", F("category")), Db.P("@d", F("desc").Length == 0 ? null : F("desc")),
                        Db.P("@p", price), Db.P("@q", qty), Db.P("@img", imageUrl), Db.P("@a", active));
                else
                    Db.Exec("UPDATE Parts SET PartNumber=@n, Name=@nm, NameAr=@nma, BrandId=@b, Category=@c, Description=@d, DescriptionAr=@da, Price=@p, Quantity=@q, IsActive=@a, ImageUrl=ISNULL(@img, ImageUrl) WHERE PartId=@id",
                        Db.P("@nma", F("nameAr").Length == 0 ? null : F("nameAr")), Db.P("@da", F("descAr").Length == 0 ? null : F("descAr")),
                        Db.P("@n", F("number")), Db.P("@nm", F("name")), Db.P("@b", brand), Db.P("@c", F("category")), Db.P("@d", F("desc").Length == 0 ? null : F("desc")),
                        Db.P("@p", price), Db.P("@q", qty), Db.P("@img", imageUrl), Db.P("@a", active), Db.P("@id", id));
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) { Err("A part with this part number already exists."); return; }
                throw;
            }
            Response.Redirect("~/Admin/Parts.aspx?saved=1", true);
        }
    }
}

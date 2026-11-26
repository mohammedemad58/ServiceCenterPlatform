using System;
using System.Data;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    /// <summary>Builds &lt;option&gt; lists for the dropdowns.</summary>
    public static class Lookups
    {
        static string Opts(DataTable dt, string idCol, string nameCol, string selected, string placeholder)
        {
            var sb = new StringBuilder();
            if (placeholder != null) sb.Append("<option value=\"\">" + HttpUtility.HtmlEncode(placeholder) + "</option>");
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r[idCol]);
                sb.Append("<option value=\"" + HttpUtility.HtmlAttributeEncode(id) + "\""
                    + (id == selected ? " selected" : "") + ">"
                    + HttpUtility.HtmlEncode(Convert.ToString(r[nameCol])) + "</option>");
            }
            return sb.ToString();
        }

        public static string BrandOptions(string selected, string placeholder)
        {
            return Opts(Db.Table("SELECT BrandId, " + Lang.Col("", "Name") + " AS Name FROM Brands ORDER BY BrandId"), "BrandId", "Name", selected, placeholder);
        }

        public static string ServiceOptions(string selected, string placeholder)
        {
            return Opts(Db.Table("SELECT ServiceId, " + Lang.Col("", "Name") + " AS Name FROM Services ORDER BY ServiceId"), "ServiceId", "Name", selected, placeholder);
        }

        public static string BranchOptions(string selected, string placeholder)
        {
            return Opts(Db.Table("SELECT BranchId, " + Lang.Col("", "Name") + " AS Name FROM Branches ORDER BY BranchId"), "BranchId", "Name", selected, placeholder);
        }

        public static string CategoryOptions(string selected, string placeholder)
        {
            var dt = Db.Table("SELECT DISTINCT Category, Category AS Label FROM Parts WHERE IsActive=1 ORDER BY Category");
            foreach (DataRow r in dt.Rows) r["Label"] = Lang.T(Convert.ToString(r["Category"]));
            return Opts(dt, "Category", "Label", selected, placeholder);
        }

        public static string SlotOptions(string selected, string placeholder)
        {
            var sb = new StringBuilder("<option value=\"\">" + placeholder + "</option>");
            foreach (string s in Slots.All())
                sb.Append("<option value=\"" + s + "\"" + (s == selected ? " selected" : "") + ">" + s + "</option>");
            return sb.ToString();
        }
    }
}

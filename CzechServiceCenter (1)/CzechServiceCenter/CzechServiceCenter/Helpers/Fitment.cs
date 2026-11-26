using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    /// <summary>Car models / years / engines for the "find parts for my car" filter.</summary>
    public static class Fitment
    {
        public static DataTable Models(int brandId)
        {
            return Db.Table("SELECT ModelId, " + Lang.Col("", "Name") + " AS Name FROM CarModels WHERE BrandId=@b ORDER BY Name", Db.P("@b", brandId));
        }

        public static List<int> Years(int modelId)
        {
            var r = Db.Row("SELECT YearFrom, YearTo FROM CarModels WHERE ModelId=@m", Db.P("@m", modelId));
            var list = new List<int>();
            if (r == null) return list;
            for (int y = Convert.ToInt32(r["YearTo"]); y >= Convert.ToInt32(r["YearFrom"]); y--) list.Add(y);
            return list;
        }

        public static List<int> Engines(int modelId, int? year)
        {
            var dt = year.HasValue
                ? Db.Table("SELECT EngineCc FROM ModelEngines WHERE ModelId=@m AND @y BETWEEN YearFrom AND YearTo ORDER BY EngineCc", Db.P("@m", modelId), Db.P("@y", year.Value))
                : Db.Table("SELECT EngineCc FROM ModelEngines WHERE ModelId=@m ORDER BY EngineCc", Db.P("@m", modelId));
            var list = new List<int>();
            foreach (DataRow r in dt.Rows) list.Add(Convert.ToInt32(r["EngineCc"]));
            return list;
        }

        public static string EngineLabel(int cc) { return Lang.T("{0} cc", cc); }

        static string Opt(string value, string label, string selected)
        {
            return "<option value=\"" + HttpUtility.HtmlAttributeEncode(value) + "\"" + (value == selected ? " selected" : "") + ">" + HttpUtility.HtmlEncode(label) + "</option>";
        }

        public static string ModelOptions(string brand, string selected)
        {
            var sb = new StringBuilder(Opt("", Lang.T("All models"), ""));
            int b;
            if (int.TryParse(brand, out b))
                foreach (DataRow r in Models(b).Rows) sb.Append(Opt(Convert.ToString(r["ModelId"]), Convert.ToString(r["Name"]), selected));
            return sb.ToString();
        }

        public static string YearOptions(string model, string selected)
        {
            var sb = new StringBuilder(Opt("", Lang.T("Any year"), ""));
            int m;
            if (int.TryParse(model, out m)) foreach (int y in Years(m)) sb.Append(Opt(y.ToString(), y.ToString(), selected));
            return sb.ToString();
        }

        public static string EngineOptions(string model, string year, string selected)
        {
            var sb = new StringBuilder(Opt("", Lang.T("Any engine"), ""));
            int m, y;
            if (int.TryParse(model, out m))
                foreach (int cc in Engines(m, int.TryParse(year, out y) ? (int?)y : null)) sb.Append(Opt(cc.ToString(), EngineLabel(cc), selected));
            return sb.ToString();
        }
    }
}

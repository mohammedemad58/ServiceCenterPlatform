using System;
using System.Collections.Generic;
using System.Data;

namespace CzechServiceCenter
{
    /// <summary>Text-only service schedule: what to do and which parts at every mileage, per brand.</summary>
    public partial class ServicesPage : BasePage
    {
        protected DataTable Brands, Plans;
        protected DataRow BrandRow;
        protected int NextKm, PlanKm, CurrentKm;

        protected void Page_Load(object sender, EventArgs e)
        {
            Brands = Db.Table("SELECT BrandId, Slug, " + Lang.Col("", "Name") + " AS Name FROM Brands ORDER BY BrandId");
            string slug = Q("b").ToLowerInvariant();
            foreach (DataRow r in Brands.Rows) if (Convert.ToString(r["Slug"]) == slug) BrandRow = r;
            if (BrandRow == null) BrandRow = Brands.Rows[0];
            Title = Lang.T("Service schedule") + " | " + Lang.T("Czech Service Center");

            Plans = Db.Table("SELECT PlanId, IntervalKm, " + Lang.Col("", "Tasks") + " AS Tasks, PartCodes FROM ServicePlans WHERE BrandId=@b ORDER BY IntervalKm", Db.P("@b", BrandRow["BrandId"]));
            Plans.Columns.Add("PartNames", typeof(string));
            var types = Db.Table("SELECT Code, " + Lang.Col("", "Name") + " AS Name FROM PartTypes");
            var names = new Dictionary<string, string>();
            foreach (DataRow t in types.Rows) names[Convert.ToString(t["Code"])] = Convert.ToString(t["Name"]);
            foreach (DataRow p in Plans.Rows)
            {
                var list = new List<string>();
                if (!(p["PartCodes"] is DBNull))
                    foreach (string c in Convert.ToString(p["PartCodes"]).Split(','))
                    { string n; if (names.TryGetValue(c.Trim(), out n)) list.Add(n); }
                p["PartNames"] = string.Join("\n", list);
            }

            // "my car is at ... km": show which service comes next (the schedule repeats every 100,000 km)
            int km;
            if (int.TryParse(Q("km"), out km) && km >= 0 && km <= 2000000)
            {
                CurrentKm = km;
                NextKm = km == 0 ? 10000 : (int)(Math.Ceiling(km / 10000.0) * 10000);
                PlanKm = NextKm % 100000 == 0 ? 100000 : NextKm % 100000;
            }
        }

        protected static string[] Lines(object o) { return Convert.ToString(o).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries); }
    }
}

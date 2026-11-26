using System;
using System.Data;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    public static class Fmt
    {
        public static string Money(object v)
        {
            if (v == null || v is DBNull) return "-";
            return Lang.Money(Convert.ToDecimal(v));
        }
        public static string Date(object v)
        {
            if (v == null || v is DBNull) return "-";
            return Lang.Date(Convert.ToDateTime(v));
        }
        public static string IsoDate(object v)
        {
            if (v == null || v is DBNull) return "";
            return Convert.ToDateTime(v).ToString("yyyy-MM-dd");
        }
        public static string Km(object v)
        {
            if (v == null || v is DBNull) return "-";
            return Lang.Km(Convert.ToInt32(v));
        }
        public static string Spaced(string pascal)
        {
            var sb = new StringBuilder();
            foreach (char ch in pascal)
            {
                if (char.IsUpper(ch) && sb.Length > 0) sb.Append(' ');
                sb.Append(ch);
            }
            return sb.ToString();
        }
        public static string StatusCss(string s)
        {
            switch (s)
            {
                case "Approved": case "Completed": case "Collected": case "ReadyForPickup": return "ok";
                case "Cancelled": return "bad";
                case "InProgress": case "UnderInspection": return "info";
                default: return "warn";
            }
        }
    }

    public class MaintAlert
    {
        public string Css;
        public string Text;
    }

    public static class Maint
    {
        public static string StockLabel(int qty, out string css)
        {
            int low = 5;
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["LowStockThreshold"], out low);
            if (low <= 0) low = 5;
            if (qty <= 0) { css = "bad"; return "Out of stock"; }
            if (qty <= low) { css = "warn"; return "Low stock"; }
            css = "ok"; return "Available";
        }

        /// <summary>Next-service alert from the vehicle's latest record, by date or mileage.</summary>
        public static MaintAlert For(DataRow r)
        {
            if (r["ServiceDate"] is DBNull)
                return new MaintAlert { Css = "info", Text = Lang.T("No service history yet") };
            DateTime? dueDate = r["NextServiceDate"] is DBNull ? (DateTime?)null : Convert.ToDateTime(r["NextServiceDate"]);
            int? dueKm = r["NextServiceKm"] is DBNull ? (int?)null : Convert.ToInt32(r["NextServiceKm"]);
            int cur = Convert.ToInt32(r["CurrentMileage"]);
            string dateTxt = dueDate.HasValue ? Lang.Date(dueDate.Value) : "";
            string kmTxt = dueKm.HasValue ? Lang.Km(dueKm.Value) : "";
            string when = (dueDate.HasValue && dueKm.HasValue) ? Lang.T("{0} or {1}", dateTxt, kmTxt) : dateTxt + kmTxt;
            bool overdue = (dueDate.HasValue && dueDate.Value.Date < DateTime.Today) || (dueKm.HasValue && cur >= dueKm.Value);
            bool soon = (dueDate.HasValue && dueDate.Value.Date <= DateTime.Today.AddDays(30)) || (dueKm.HasValue && cur >= dueKm.Value - 1000);
            if (overdue) return new MaintAlert { Css = "bad", Text = Lang.T("Service overdue (due {0})", when) };
            if (soon) return new MaintAlert { Css = "warn", Text = Lang.T("Service due soon ({0})", when) };
            return new MaintAlert { Css = "ok", Text = Lang.T("Next service: {0}", when) };
        }
    }
}

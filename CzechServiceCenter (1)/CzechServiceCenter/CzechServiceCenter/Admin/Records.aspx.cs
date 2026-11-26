using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class AdminRecords : BasePage
    {
        protected DataTable Vehicles, Records;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Save();
            else if (Q("saved") == "1") Ok("Service record saved. The customer's next-service alert was updated.");
            Vehicles = Db.Table("SELECT v.VehicleId, v.PlateNumber, " + Lang.Col("br", "Name") + " + ' ' + v.Model AS Car, u.FullName FROM Vehicles v " +
                "JOIN Brands br ON br.BrandId=v.BrandId JOIN Users u ON u.UserId=v.UserId ORDER BY v.PlateNumber");
            Records = Db.Table("SELECT TOP 30 m.ServiceDate, m.Mileage, m.Description, m.PartsChanged, m.Cost, m.Technician, m.NextServiceDate, m.NextServiceKm, " +
                "v.PlateNumber, " + Lang.Col("br", "Name") + " + ' ' + v.Model AS Car, u.FullName FROM MaintenanceRecords m JOIN Vehicles v ON v.VehicleId=m.VehicleId " +
                "JOIN Brands br ON br.BrandId=v.BrandId JOIN Users u ON u.UserId=v.UserId ORDER BY m.ServiceDate DESC, m.RecordId DESC");
        }

        void Save()
        {
            int vehicle, mileage, nextKm; DateTime date, nextDate; decimal cost;
            if (!int.TryParse(F("vehicle"), out vehicle) || !TryDate(F("date"), out date) || !int.TryParse(F("mileage"), out mileage) || mileage < 0 ||
                !decimal.TryParse(F("cost"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out cost) || cost < 0)
            { Err("Please choose a vehicle and fill in the date, mileage and cost."); return; }

            // default next service: 6 months or 10,000 km after this one
            if (!TryDate(F("nextDate"), out nextDate)) nextDate = date.AddMonths(6);
            if (!int.TryParse(F("nextKm"), out nextKm) || nextKm <= 0) nextKm = mileage + 10000;

            Db.Exec("INSERT INTO MaintenanceRecords(VehicleId, ServiceDate, Mileage, Description, PartsChanged, Cost, Technician, NextServiceDate, NextServiceKm) " +
                    "VALUES(@v, @d, @m, @desc, @parts, @c, @t, @nd, @nk); " +
                    "UPDATE Vehicles SET CurrentMileage=CASE WHEN CurrentMileage<@m THEN @m ELSE CurrentMileage END WHERE VehicleId=@v",
                Db.P("@v", vehicle), Db.P("@d", date), Db.P("@m", mileage),
                Db.P("@desc", F("desc").Length == 0 ? null : F("desc")), Db.P("@parts", F("parts").Length == 0 ? null : F("parts")),
                Db.P("@c", cost), Db.P("@t", F("tech").Length == 0 ? null : F("tech")), Db.P("@nd", nextDate), Db.P("@nk", nextKm));
            Response.Redirect("~/Admin/Records.aspx?saved=1", true);
        }
    }
}

using System;
using System.Data;
using System.Text.RegularExpressions;

namespace CzechServiceCenter
{
    public partial class BookPage : BasePage
    {
        protected DataTable Vehicles;
        protected bool Done;

        protected string Sel(string key) { return IsPost ? F(key) : Q(key); }
        protected string Hours
        {
            get { return Slots.HoursText; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Booking is open to every visitor. Logged-in customers can also pick one of their saved cars.
            bool loggedIn = Auth.IsAuthenticated;
            Done = Q("done") == "1";
            Vehicles = loggedIn
                ? Db.Table("SELECT v.VehicleId, v.BrandId, v.Model, v.PlateNumber, b.Name AS BrandName FROM Vehicles v JOIN Brands b ON b.BrandId=v.BrandId WHERE v.UserId=@u ORDER BY v.VehicleId",
                    Db.P("@u", Auth.UserId))
                : new DataTable();
            if (!IsPost) return;

            // honeypot: real visitors never fill this hidden field
            if (F("website").Length > 0) { Response.Redirect("~/Book.aspx?done=1", true); return; }

            int branch, brand, service; DateTime date;
            if (!int.TryParse(F("branch"), out branch) || !int.TryParse(F("brand"), out brand) || !int.TryParse(F("service"), out service) || !TryDate(F("date"), out date) || F("time").Length == 0)
            {
                Err("Please choose a branch, a brand, a service, a date and a time.");
                return;
            }

            string gName = null, gPhone = null, gEmail = null, gCar = null;
            if (!loggedIn)
            {
                gName = F("gname"); gPhone = F("gphone"); gEmail = F("gemail"); gCar = F("gcar");
                if (gName.Length < 2) { Err("Please enter your name."); return; }
                if (!Regex.IsMatch(gPhone, @"^[0-9+\-\s()]{7,20}$")) { Err("Please enter a valid phone number so we can contact you."); return; }
                if (gEmail.Length > 0 && !Regex.IsMatch(gEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { Err("Please enter a valid email address or leave it empty."); return; }
                if (gName.Length > 100) gName = gName.Substring(0, 100);
                if (gCar.Length > 120) gCar = gCar.Substring(0, 120);
                // simple anti-spam: a guest phone number can hold at most 3 open bookings
                if (Db.Count("SELECT COUNT(*) FROM Bookings WHERE UserId IS NULL AND GuestPhone=@p AND Status IN ('Pending','Approved') AND BookingDate>=CAST(GETDATE() AS DATE)",
                        Db.P("@p", gPhone)) >= 3)
                { Err("This phone number already has 3 upcoming bookings. Please call us to change them."); return; }
            }

            string error;
            if (BookingService.TryCreate(loggedIn ? (int?)Auth.UserId : null, IntOrNullNullable(F("vehicle")), branch, brand, service, date, F("time"), F("notes"),
                    gName, gPhone, gEmail, gCar, out error))
            {
                if (loggedIn) Response.Redirect("~/Account/Profile.aspx?booked=1#bookings", true);
                else Response.Redirect("~/Book.aspx?done=1", true);
            }
            Err(error);
        }

        static int? IntOrNullNullable(string s)
        {
            int v;
            return (int.TryParse(s, out v) && v > 0) ? (int?)v : null;
        }
    }
}

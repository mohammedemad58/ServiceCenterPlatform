using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CzechServiceCenter
{
    public partial class AdminBookings : BasePage
    {
        protected DataTable Rows;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Handle();
            LoadRows();
        }

        void LoadRows()
        {
            var where = new StringBuilder("WHERE 1=1");
            var ps = new List<SqlParameter>();
            DateTime d;
            int br; if (int.TryParse(Q("branch"), out br)) { where.Append(" AND b.BranchId=@br"); ps.Add(Db.P("@br", br)); }
            if (Q("status").Length > 0) { where.Append(" AND b.Status=@s"); ps.Add(Db.P("@s", Q("status"))); }
            if (Q("date").Length > 0 && TryDate(Q("date"), out d)) { where.Append(" AND b.BookingDate=@d"); ps.Add(Db.P("@d", d)); }
            else if (Q("date").Length == 0) where.Append(" AND b.BookingDate>=DATEADD(DAY,-7,CAST(GETDATE() AS DATE))");
            Rows = Db.Table(
                "SELECT TOP 200 b.BookingId, b.BookingDate, b.TimeSlot, b.Status, b.WorkStatus, b.Notes, COALESCE(u.FullName, b.GuestName) AS FullName, COALESCE(u.Phone, b.GuestPhone) AS Phone, " +
                "COALESCE(u.Email, b.GuestEmail) AS Email, CASE WHEN b.UserId IS NULL THEN 1 ELSE 0 END AS IsGuest, b.GuestCar, " +
                Lang.Col("br", "Name") + " AS BrandName, " + Lang.Col("s", "Name") + " AS ServiceName, " + Lang.Col("bn", "Name") + " AS BranchName, v.Model, v.PlateNumber " +
                "FROM Bookings b JOIN Branches bn ON bn.BranchId=b.BranchId LEFT JOIN Users u ON u.UserId=b.UserId JOIN Brands br ON br.BrandId=b.BrandId JOIN Services s ON s.ServiceId=b.ServiceId " +
                "LEFT JOIN Vehicles v ON v.VehicleId=b.VehicleId " + where + " ORDER BY b.BookingDate, b.TimeSlot", ps.ToArray());
        }

        void Handle()
        {
            string cmd = F("cmd");
            int i = cmd.IndexOf(':');
            int id;
            if (i < 0 || !int.TryParse(cmd.Substring(i + 1), out id)) return;
            string name = cmd.Substring(0, i);

            switch (name)
            {
                case "approve": Db.Exec("UPDATE Bookings SET Status='Approved' WHERE BookingId=@i AND Status='Pending'", Db.P("@i", id)); Ok(Lang.T("Booking #{0} approved.", id)); break;
                case "complete": Db.Exec("UPDATE Bookings SET Status='Completed', WorkStatus='ReadyForPickup' WHERE BookingId=@i AND Status='Approved'", Db.P("@i", id)); Ok(Lang.T("Booking #{0} completed.", id)); break;
                case "cancel": Db.Exec("UPDATE Bookings SET Status='Cancelled' WHERE BookingId=@i", Db.P("@i", id)); Ok(Lang.T("Booking #{0} cancelled.", id)); break;
                case "work":
                    string ws = F("work_" + id);
                    if (Array.IndexOf(new[] { "Scheduled", "UnderInspection", "InProgress", "ReadyForPickup" }, ws) < 0) { Err("Unknown status."); break; }
                    // working on a car implies the booking was accepted
                    Db.Exec("UPDATE Bookings SET WorkStatus=@w, Status=CASE WHEN Status='Pending' AND @w<>'Scheduled' THEN 'Approved' ELSE Status END WHERE BookingId=@i AND Status<>'Cancelled'",
                        Db.P("@w", ws), Db.P("@i", id));
                    Ok("Workshop status updated. The customer can see it in their account.");
                    break;
                case "resched":
                    DateTime nd;
                    if (!TryDate(F("date_" + id), out nd) || F("time_" + id).Length == 0) { Err("Choose a new date and time."); break; }
                    string error;
                    if (BookingService.TryReschedule(id, nd, F("time_" + id), out error)) Ok(Lang.T("Booking #{0} moved to {1} {2}.", id, Fmt.Date(nd), F("time_" + id)));
                    else Err(error);
                    break;
            }
        }
    }
}

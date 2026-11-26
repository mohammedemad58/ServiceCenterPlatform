using System;
using System.Data;
using System.Data.SqlClient;

namespace CzechServiceCenter
{
    public partial class ProfilePage : BasePage
    {
        protected DataRow Me;
        protected DataTable Vehicles, Bookings, Records, Reservations;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Handle();
            else if (Q("booked") == "1") Ok("Your booking request was received. We will confirm it shortly.");
            else if (Q("welcome") == "1") Ok("Welcome! Add your vehicle below to get started.");
            LoadData();
        }

        void LoadData()
        {
            int u = Auth.UserId;
            Me = Db.Row("SELECT FullName, Email, Phone FROM Users WHERE UserId=@u", Db.P("@u", u));
            Vehicles = Db.Table(
                "SELECT v.VehicleId, v.Model, v.[Year], v.PlateNumber, v.Vin, v.CurrentMileage, " + Lang.Col("b", "Name") + " AS BrandName, " +
                "r.ServiceDate, r.Mileage AS LastKm, r.NextServiceDate, r.NextServiceKm, r.Cost, r.Technician, r.PartsChanged " +
                "FROM Vehicles v JOIN Brands b ON b.BrandId=v.BrandId " +
                "OUTER APPLY (SELECT TOP 1 * FROM MaintenanceRecords m WHERE m.VehicleId=v.VehicleId ORDER BY m.ServiceDate DESC, m.RecordId DESC) r " +
                "WHERE v.UserId=@u ORDER BY v.VehicleId", Db.P("@u", u));
            Bookings = Db.Table(
                "SELECT bk.BookingId, bk.BookingDate, bk.TimeSlot, bk.Status, bk.WorkStatus, " + Lang.Col("br", "Name") + " AS BrandName, " + Lang.Col("s", "Name") + " AS ServiceName, " + Lang.Col("bn", "Name") + " AS BranchName " +
                "FROM Bookings bk JOIN Branches bn ON bn.BranchId=bk.BranchId JOIN Brands br ON br.BrandId=bk.BrandId JOIN Services s ON s.ServiceId=bk.ServiceId " +
                "WHERE bk.UserId=@u ORDER BY bk.BookingDate DESC, bk.TimeSlot DESC", Db.P("@u", u));
            Records = Db.Table(
                "SELECT m.ServiceDate, m.Mileage, m.Description, m.PartsChanged, m.Cost, m.Technician, v.PlateNumber, " + Lang.Col("br", "Name") + " + ' ' + v.Model AS Car " +
                "FROM MaintenanceRecords m JOIN Vehicles v ON v.VehicleId=m.VehicleId JOIN Brands br ON br.BrandId=v.BrandId " +
                "WHERE v.UserId=@u ORDER BY m.ServiceDate DESC, m.RecordId DESC", Db.P("@u", u));
            Reservations = Db.Table(
                "SELECT r.ReservationId, r.Quantity, r.Status, r.CreatedAt, " + Lang.Col("p", "Name") + " AS PartName, p.PartNumber, p.Price " +
                "FROM PartReservations r JOIN Parts p ON p.PartId=r.PartId WHERE r.UserId=@u ORDER BY r.CreatedAt DESC", Db.P("@u", u));
        }

        static bool Split(string cmd, out string name, out int id)
        {
            name = cmd; id = 0;
            int i = cmd.IndexOf(':');
            if (i < 0) return true;
            name = cmd.Substring(0, i);
            return int.TryParse(cmd.Substring(i + 1), out id);
        }

        void Handle()
        {
            string name; int id;
            if (!Split(F("cmd"), out name, out id)) return;
            int u = Auth.UserId;

            switch (name)
            {
                case "profile":
                    if (F("fullName").Length < 3 || F("phone").Length < 7) { Err("Please enter your name and a valid mobile number."); return; }
                    Db.Exec("UPDATE Users SET FullName=@n, Phone=@p WHERE UserId=@u", Db.P("@n", F("fullName")), Db.P("@p", F("phone")), Db.P("@u", u));
                    Auth.SignIn(u, Auth.Email, F("fullName"), Auth.Role, false);
                    Ok("Your details were saved.");
                    break;

                case "add-vehicle":
                    int brand, year, km = 0;
                    if (!int.TryParse(F("brand"), out brand) || F("model").Length == 0 || F("plate").Length == 0 ||
                        !int.TryParse(F("year"), out year) || year < 1980 || year > DateTime.Now.Year + 1)
                    { Err("Please fill in brand, model, a valid year and the plate number."); return; }
                    if (F("mileage").Length > 0 && (!int.TryParse(F("mileage"), out km) || km < 0)) { Err("Mileage must be a positive number."); return; }
                    Db.Exec("INSERT INTO Vehicles(UserId, BrandId, Model, [Year], PlateNumber, Vin, CurrentMileage) VALUES(@u, @b, @m, @y, @p, @v, @k)",
                        Db.P("@u", u), Db.P("@b", brand), Db.P("@m", F("model")), Db.P("@y", year), Db.P("@p", F("plate")),
                        Db.P("@v", F("vin").Length == 0 ? null : F("vin")), Db.P("@k", km));
                    Response.Redirect("~/Account/Profile.aspx#vehicles", true);
                    break;

                case "mileage":
                    int newKm;
                    if (!int.TryParse(F("km_" + id), out newKm) || newKm < 0) { Err("Enter a valid mileage."); return; }
                    Db.Exec("UPDATE Vehicles SET CurrentMileage=@k WHERE VehicleId=@v AND UserId=@u", Db.P("@k", newKm), Db.P("@v", id), Db.P("@u", u));
                    Ok("Mileage updated.");
                    break;

                case "del-vehicle":
                    try
                    {
                        Db.Exec("DELETE FROM Vehicles WHERE VehicleId=@v AND UserId=@u", Db.P("@v", id), Db.P("@u", u));
                        Ok("Vehicle removed.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547) Err("This vehicle has service history and cannot be removed."); else throw;
                    }
                    break;

                case "cancel-booking":
                    int n = Db.Exec("UPDATE Bookings SET Status='Cancelled' WHERE BookingId=@b AND UserId=@u AND Status IN ('Pending','Approved') AND BookingDate>=CAST(GETDATE() AS DATE)",
                        Db.P("@b", id), Db.P("@u", u));
                    if (n > 0) Ok("Booking cancelled."); else Err("This booking can no longer be cancelled.");
                    break;

                case "cancel-res":
                    // give the stock back in the same transaction
                    Db.Exec("SET XACT_ABORT ON; BEGIN TRAN; DECLARE @pid INT, @q INT; " +
                        "UPDATE PartReservations SET Status='Cancelled', @pid=PartId, @q=Quantity WHERE ReservationId=@r AND UserId=@u AND Status='Reserved'; " +
                        "IF @@ROWCOUNT=1 UPDATE Parts SET Quantity=Quantity+@q WHERE PartId=@pid; COMMIT;",
                        Db.P("@r", id), Db.P("@u", u));
                    Ok("Reservation cancelled.");
                    break;
            }
        }
    }
}

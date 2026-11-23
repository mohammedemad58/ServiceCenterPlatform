using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CzechServiceCenter
{
    /// <summary>Opening hours and availability of booking slots.</summary>
    public static class Slots
    {
        static int App(string key, int def)
        {
            int v;
            return int.TryParse(ConfigurationManager.AppSettings[key], out v) ? v : def;
        }

        public static int Bays { get { return App("Bays", 3); } }
        public static int StartHour { get { return App("SlotStartHour", 9); } }
        public static int EndHour { get { return App("SlotEndHour", 17); } }

        public static DayOfWeek ClosedDay
        {
            get
            {
                DayOfWeek d;
                return Enum.TryParse(ConfigurationManager.AppSettings["ClosedDay"], out d) ? d : DayOfWeek.Friday;
            }
        }

        public static List<string> All()
        {
            var l = new List<string>();
            for (int h = StartHour; h < EndHour; h++) l.Add(h.ToString("00") + ":00");
            return l;
        }

        public static bool IsWorkingDay(DateTime d) { return d.DayOfWeek != ClosedDay; }

        /// <summary>"09:00 to 17:00, closed on Fridays" (or the Arabic equivalent).</summary>
        public static string HoursText
        {
            get { return Lang.T("{0} to {1}, closed on {2}s", StartHour.ToString("00") + ":00", EndHour.ToString("00") + ":00", Lang.DayName(ClosedDay)); }
        }

        /// <summary>Free slots on a date at one branch (a slot is free while fewer than Bays bookings use it there).</summary>
        public static List<string> Available(DateTime date, int branchId, int excludeBookingId)
        {
            var res = new List<string>();
            date = date.Date;
            if (date < DateTime.Today || !IsWorkingDay(date)) return res;
            var taken = new Dictionary<string, int>();
            var dt = Db.Table("SELECT TimeSlot, COUNT(*) AS C FROM Bookings WHERE BookingDate=@d AND BranchId=@br AND Status<>'Cancelled' AND BookingId<>@x GROUP BY TimeSlot",
                Db.P("@d", date), Db.P("@br", branchId), Db.P("@x", excludeBookingId));
            foreach (DataRow r in dt.Rows) taken[Convert.ToString(r["TimeSlot"]).Trim()] = Convert.ToInt32(r["C"]);
            foreach (string s in All())
            {
                int hour = int.Parse(s.Substring(0, 2));
                if (date == DateTime.Today && DateTime.Now.Hour + 1 >= hour) continue;
                int c; taken.TryGetValue(s, out c);
                if (c < Bays) res.Add(s);
            }
            return res;
        }
    }

    /// <summary>Booking rules. Capacity is re-checked inside a serializable transaction so two customers cannot take the last bay at once.</summary>
    public static class BookingService
    {
        static string Validate(DateTime date, string slot)
        {
            if (date.Date < DateTime.Today) return Lang.T("Please choose a date from today onwards.");
            if (!Slots.IsWorkingDay(date)) return Lang.T("The service center is closed on {0}s.", Lang.DayName(date.DayOfWeek));
            if (!Slots.All().Contains(slot)) return Lang.T("Please choose a valid time.");
            if (date.Date == DateTime.Today && int.Parse(slot.Substring(0, 2)) <= DateTime.Now.Hour) return Lang.T("That time has already passed.");
            return null;
        }

        static bool HasRoom(SqlConnection cn, SqlTransaction tx, int branchId, DateTime date, string slot, int excludeId)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE BookingDate=@d AND TimeSlot=@t AND BranchId=@br AND Status<>'Cancelled' AND BookingId<>@x", cn, tx))
            {
                cmd.Parameters.AddWithValue("@br", branchId);
                cmd.Parameters.AddWithValue("@d", date.Date);
                cmd.Parameters.AddWithValue("@t", slot);
                cmd.Parameters.AddWithValue("@x", excludeId);
                return (int)cmd.ExecuteScalar() < Slots.Bays;
            }
        }

        /// <summary>userId = null creates a guest booking (name/phone/email/car are stored on the booking itself).</summary>
        public static bool TryCreate(int? userId, int? vehicleId, int branchId, int brandId, int serviceId, DateTime date, string slot, string notes,
            string guestName, string guestPhone, string guestEmail, string guestCar, out string error)
        {
            error = Validate(date, slot);
            if (error != null) return false;
            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (var chk = new SqlCommand("SELECT COUNT(*) FROM Branches WHERE BranchId=@b", cn, tx))
                    {
                        chk.Parameters.AddWithValue("@b", branchId);
                        if ((int)chk.ExecuteScalar() == 0) { tx.Rollback(); error = Lang.T("Please choose a valid branch."); return false; }
                    }
                    if (!HasRoom(cn, tx, branchId, date, slot, 0))
                    {
                        tx.Rollback();
                        error = Lang.T("Sorry, that time was just taken. Please pick another slot.");
                        return false;
                    }
                    using (var cmd = new SqlCommand(
                        "INSERT INTO Bookings(UserId, VehicleId, BranchId, BrandId, ServiceId, BookingDate, TimeSlot, Notes, GuestName, GuestPhone, GuestEmail, GuestCar) " +
                        "VALUES(@u, (SELECT VehicleId FROM Vehicles WHERE VehicleId=@v AND UserId=@u), @br, @b, @s, @d, @t, @n, @gn, @gp, @ge, @gc)", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@br", branchId);
                        cmd.Parameters.AddWithValue("@u", (object)userId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@gn", string.IsNullOrEmpty(guestName) ? (object)DBNull.Value : guestName);
                        cmd.Parameters.AddWithValue("@gp", string.IsNullOrEmpty(guestPhone) ? (object)DBNull.Value : guestPhone);
                        cmd.Parameters.AddWithValue("@ge", string.IsNullOrEmpty(guestEmail) ? (object)DBNull.Value : guestEmail);
                        cmd.Parameters.AddWithValue("@gc", string.IsNullOrEmpty(guestCar) ? (object)DBNull.Value : guestCar);
                        cmd.Parameters.AddWithValue("@v", (object)vehicleId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@b", brandId);
                        cmd.Parameters.AddWithValue("@s", serviceId);
                        cmd.Parameters.AddWithValue("@d", date.Date);
                        cmd.Parameters.AddWithValue("@t", slot);
                        cmd.Parameters.AddWithValue("@n", string.IsNullOrEmpty(notes) ? (object)DBNull.Value : notes);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            return true;
        }

        public static bool TryReschedule(int bookingId, DateTime date, string slot, out string error)
        {
            error = Validate(date, slot);
            if (error != null) return false;
            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    object br;
                    using (var q = new SqlCommand("SELECT BranchId FROM Bookings WHERE BookingId=@id", cn, tx))
                    {
                        q.Parameters.AddWithValue("@id", bookingId);
                        br = q.ExecuteScalar();
                    }
                    if (br == null || br is DBNull) { tx.Rollback(); error = Lang.T("Booking not found."); return false; }
                    if (!HasRoom(cn, tx, Convert.ToInt32(br), date, slot, bookingId))
                    {
                        tx.Rollback();
                        error = Lang.T("That slot is full. Choose another time.");
                        return false;
                    }
                    using (var cmd = new SqlCommand("UPDATE Bookings SET BookingDate=@d, TimeSlot=@t WHERE BookingId=@id AND Status<>'Cancelled'", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@d", date.Date);
                        cmd.Parameters.AddWithValue("@t", slot);
                        cmd.Parameters.AddWithValue("@id", bookingId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            return true;
        }
    }
}

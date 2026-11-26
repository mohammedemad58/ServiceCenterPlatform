using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class AdminDashboard : BasePage
    {
        protected int TodayCount, PendingCount, InShop, Customers, PartsCount, LowStock;
        protected DataTable Upcoming, LowParts, Messages;

        protected void Page_Load(object sender, EventArgs e)
        {
            TodayCount = Db.Count("SELECT COUNT(*) FROM Bookings WHERE BookingDate=CAST(GETDATE() AS DATE) AND Status<>'Cancelled'");
            PendingCount = Db.Count("SELECT COUNT(*) FROM Bookings WHERE Status='Pending' AND BookingDate>=CAST(GETDATE() AS DATE)");
            InShop = Db.Count("SELECT COUNT(*) FROM Bookings WHERE WorkStatus IN ('UnderInspection','InProgress') AND Status<>'Cancelled'");
            Customers = Db.Count("SELECT COUNT(*) FROM Users WHERE Role='Customer'");
            PartsCount = Db.Count("SELECT COUNT(*) FROM Parts WHERE IsActive=1");
            LowStock = Db.Count("SELECT COUNT(*) FROM Parts WHERE IsActive=1 AND Quantity<=5");
            Upcoming = Db.Table("SELECT TOP 10 b.BookingDate, b.TimeSlot, b.Status, COALESCE(u.FullName, b.GuestName) AS FullName, COALESCE(u.Phone, b.GuestPhone) AS Phone, " + Lang.Col("br", "Name") + " AS BrandName, " + Lang.Col("s", "Name") + " AS ServiceName, " + Lang.Col("bn", "Name") + " AS BranchName " +
                "FROM Bookings b JOIN Branches bn ON bn.BranchId=b.BranchId LEFT JOIN Users u ON u.UserId=b.UserId JOIN Brands br ON br.BrandId=b.BrandId JOIN Services s ON s.ServiceId=b.ServiceId " +
                "WHERE b.BookingDate>=CAST(GETDATE() AS DATE) AND b.Status<>'Cancelled' ORDER BY b.BookingDate, b.TimeSlot");
            LowParts = Db.Table("SELECT TOP 8 PartNumber, Name, Quantity FROM Parts WHERE IsActive=1 AND Quantity<=5 ORDER BY Quantity, Name");
            Messages = Db.Table("SELECT TOP 5 FullName, Email, Phone, Body, CreatedAt FROM ContactMessages ORDER BY CreatedAt DESC");
        }
    }
}

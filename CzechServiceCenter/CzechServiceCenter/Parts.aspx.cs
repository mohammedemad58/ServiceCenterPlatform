using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

namespace CzechServiceCenter
{
    public partial class PartsPage : BasePage
    {
        const int PageSize = 24;
        protected DataTable PartRows, MyBookings, MyVehicles;
        protected DataRow Reserving;
        protected int Total, PageNo = 1, Pages = 1;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Reserve();
            if (!IsPost && Q("reserved") == "1") Ok("Part reserved. You can follow it from My Account.");

            int rid;
            if (int.TryParse(Q("reserve"), out rid))
            {
                RequireLogin();
                Reserving = Db.Row("SELECT PartId, PartNumber, " + Lang.Col("", "Name") + " AS Name, Price, Quantity FROM Parts WHERE PartId=@p AND IsActive=1 AND Quantity>0", Db.P("@p", rid));
                if (Reserving != null)
                {
                    MyBookings = Db.Table("SELECT b.BookingId, b.BookingDate, b.TimeSlot, " + Lang.Col("s", "Name") + " AS ServiceName FROM Bookings b JOIN Services s ON s.ServiceId=b.ServiceId WHERE b.UserId=@u AND b.Status IN ('Pending','Approved') AND b.BookingDate>=CAST(GETDATE() AS DATE) ORDER BY b.BookingDate", Db.P("@u", Auth.UserId));
                    MyVehicles = Db.Table("SELECT v.VehicleId, v.PlateNumber, " + Lang.Col("br", "Name") + " + ' ' + v.Model AS Car FROM Vehicles v JOIN Brands br ON br.BrandId=v.BrandId WHERE v.UserId=@u", Db.P("@u", Auth.UserId));
                }
            }
            Search();
        }

        void Search()
        {
            var where = new StringBuilder("WHERE p.IsActive=1");
            var ps = new List<SqlParameter>();
            string q = Q("q");
            if (q.Length > 0) { where.Append(" AND (p.Name LIKE @q OR p.NameAr LIKE @q OR p.PartNumber LIKE @q)"); ps.Add(Db.P("@q", "%" + q.Replace("[", "[[]").Replace("%", "[%]") + "%")); }
            int brand; if (int.TryParse(Q("brand"), out brand)) { where.Append(" AND p.BrandId=@b"); ps.Add(Db.P("@b", brand)); }
            if (Q("cat").Length > 0) { where.Append(" AND p.Category=@c"); ps.Add(Db.P("@c", Q("cat"))); }
            if (Q("stock") == "in") where.Append(" AND p.Quantity>0");
            if (Q("stock") == "low") where.Append(" AND p.Quantity BETWEEN 1 AND 5");

            Total = Db.Count("SELECT COUNT(*) FROM Parts p " + where, Clone(ps));
            Pages = Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
            int pg; if (int.TryParse(Q("page"), out pg)) PageNo = Math.Min(Math.Max(1, pg), Pages);

            var p2 = Clone(ps);
            Array.Resize(ref p2, p2.Length + 2);
            p2[p2.Length - 2] = Db.P("@skip", (PageNo - 1) * PageSize);
            p2[p2.Length - 1] = Db.P("@take", PageSize);
            PartRows = Db.Table("SELECT p.PartId, p.PartNumber, " + Lang.Col("p", "Name") + " AS Name, p.Category, p.Price, p.Quantity, p.ImageUrl, " + Lang.Col("b", "Name") + " AS BrandName " +
                "FROM Parts p JOIN Brands b ON b.BrandId=p.BrandId " + where +
                " ORDER BY b.Name, p.Category, p.Name OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p2);
        }

        static SqlParameter[] Clone(List<SqlParameter> ps)
        {
            var a = new SqlParameter[ps.Count];
            for (int i = 0; i < a.Length; i++) a[i] = new SqlParameter(ps[i].ParameterName, ps[i].Value);
            return a;
        }

        protected string PageUrl(int n)
        {
            return "Parts.aspx?q=" + HttpUtility.UrlEncode(Q("q")) + "&brand=" + HttpUtility.UrlEncode(Q("brand")) +
                   "&cat=" + HttpUtility.UrlEncode(Q("cat")) + "&stock=" + HttpUtility.UrlEncode(Q("stock")) + "&page=" + n;
        }

        protected string PartUrl(int id)
        {
            return "Parts.aspx?reserve=" + id + "&q=" + HttpUtility.UrlEncode(Q("q")) + "&brand=" + HttpUtility.UrlEncode(Q("brand")) +
                   "&cat=" + HttpUtility.UrlEncode(Q("cat")) + "&stock=" + HttpUtility.UrlEncode(Q("stock")) + "&page=" + PageNo;
        }

        void Reserve()
        {
            RequireLogin();
            int partId, qty;
            if (!int.TryParse(F("partId"), out partId) || !int.TryParse(F("qty"), out qty) || qty < 1 || qty > 10)
            { Err("Please enter a valid quantity (1 to 10)."); return; }

            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    // stock is reduced atomically; fails if not enough is left
                    using (var upd = new SqlCommand("UPDATE Parts SET Quantity=Quantity-@q WHERE PartId=@p AND IsActive=1 AND Quantity>=@q", cn, tx))
                    {
                        upd.Parameters.AddWithValue("@q", qty);
                        upd.Parameters.AddWithValue("@p", partId);
                        if (upd.ExecuteNonQuery() == 0)
                        {
                            tx.Rollback();
                            Err("Sorry, the requested quantity is no longer available.");
                            return;
                        }
                    }
                    using (var ins = new SqlCommand(
                        "INSERT INTO PartReservations(UserId, PartId, VehicleId, BookingId, Quantity) VALUES(@u, @p, " +
                        "(SELECT VehicleId FROM Vehicles WHERE VehicleId=@v AND UserId=@u), " +
                        "(SELECT BookingId FROM Bookings WHERE BookingId=@b AND UserId=@u), @q)", cn, tx))
                    {
                        ins.Parameters.AddWithValue("@u", Auth.UserId);
                        ins.Parameters.AddWithValue("@p", partId);
                        ins.Parameters.AddWithValue("@v", IntOrNull(F("vehicleId")) ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@b", IntOrNull(F("bookingId")) ?? DBNull.Value);
                        ins.Parameters.AddWithValue("@q", qty);
                        ins.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            Response.Redirect("~/Parts.aspx?reserved=1", true);
        }
    }
}

using System;
using System.Data;

namespace CzechServiceCenter
{
    public partial class BrandPage : BasePage
    {
        protected DataRow BrandRow;
        protected DataTable Services, Featured;

        protected void Page_Load(object sender, EventArgs e)
        {
            BrandRow = Db.Row("SELECT BrandId, " + Lang.Col("", "Name") + " AS Name, " + Lang.Col("", "Description") + " AS Description FROM Brands WHERE Slug=@s", Db.P("@s", Q("b").ToLowerInvariant()));
            if (BrandRow == null) throw new System.Web.HttpException(404, "Brand not found");
            Title = Lang.T("{0} service", BrandRow["Name"]) + " | " + Lang.T("Czech Service Center");
            Services = Db.Table("SELECT ServiceId, " + Lang.Col("", "Name") + " AS Name, " + Lang.Col("", "Description") + " AS Description, DurationMinutes, PriceFrom FROM Services ORDER BY ServiceId");
            Featured = Db.Table("SELECT TOP 6 PartNumber, " + Lang.Col("", "Name") + " AS Name, Price, Quantity FROM Parts WHERE BrandId=@b AND IsActive=1 AND Quantity>0 AND Category IN ('Filters','Brakes') ORDER BY PartId",
                Db.P("@b", BrandRow["BrandId"]));
        }
    }
}

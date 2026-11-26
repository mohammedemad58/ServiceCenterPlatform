using System;
using System.Data;

namespace CzechServiceCenter
{
    /// <summary>Which car models / years / engines each part fits (drives the "find parts for your car" filter).</summary>
    public partial class AdminFitment : BasePage
    {
        protected DataTable Found, FitRows, ModelList;
        protected DataRow Part;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPost) Handle();
            Load();
        }

        void Load()
        {
            string q = Q("q");
            Found = q.Length == 0 ? new DataTable() : Db.Table(
                "SELECT TOP 20 p.PartId, p.PartNumber, p.Name, b.Name AS BrandName, (SELECT COUNT(*) FROM PartFitment f WHERE f.PartId=p.PartId) AS Fits FROM Parts p JOIN Brands b ON b.BrandId=p.BrandId " +
                "WHERE p.PartNumber LIKE @q OR p.Name LIKE @q ORDER BY p.PartNumber", Db.P("@q", "%" + q.Replace("[", "[[]").Replace("%", "[%]") + "%"));
            int pid;
            if (int.TryParse(Q("part"), out pid))
            {
                Part = Db.Row("SELECT p.PartId, p.PartNumber, p.Name, p.BrandId, b.Name AS BrandName FROM Parts p JOIN Brands b ON b.BrandId=p.BrandId WHERE p.PartId=@p", Db.P("@p", pid));
                if (Part != null)
                {
                    FitRows = Db.Table("SELECT f.FitId, m.Name AS Model, f.YearFrom, f.YearTo, f.EngineCc FROM PartFitment f JOIN CarModels m ON m.ModelId=f.ModelId WHERE f.PartId=@p ORDER BY m.Name, f.EngineCc", Db.P("@p", pid));
                    ModelList = Db.Table("SELECT ModelId, Name, YearFrom, YearTo FROM CarModels WHERE BrandId=@b ORDER BY Name", Db.P("@b", Part["BrandId"]));
                }
            }
        }

        void Handle()
        {
            int pid; int.TryParse(Q("part"), out pid);
            string cmd = F("cmd");
            if (cmd == "add")
            {
                int model, yFrom, yTo, cc = 0;
                bool hasCc = F("cc").Length > 0;
                if (!int.TryParse(F("model"), out model) || !int.TryParse(F("from"), out yFrom) || !int.TryParse(F("to"), out yTo) || yFrom < 1980 || yTo > 2100 || yFrom > yTo || (hasCc && (!int.TryParse(F("cc"), out cc) || cc < 500 || cc > 8000)))
                { Err("Choose a model, a valid year range, and an engine size between 500 and 8000 cc (or leave it empty for all engines)."); return; }
                // the model must belong to the same brand as the part
                if (Db.Count("SELECT COUNT(*) FROM CarModels m JOIN Parts p ON p.BrandId=m.BrandId WHERE m.ModelId=@m AND p.PartId=@p", Db.P("@m", model), Db.P("@p", pid)) == 0)
                { Err("That model belongs to a different brand."); return; }
                Db.Exec("INSERT INTO PartFitment(PartId, ModelId, YearFrom, YearTo, EngineCc) VALUES(@p,@m,@f,@t,@c)",
                    Db.P("@p", pid), Db.P("@m", model), Db.P("@f", yFrom), Db.P("@t", yTo), Db.P("@c", hasCc ? (object)cc : null));
                Ok("Fitment added.");
            }
            else if (cmd == "allmodels")
            {
                int n = Db.Exec("INSERT INTO PartFitment(PartId, ModelId, YearFrom, YearTo, EngineCc) SELECT p.PartId, m.ModelId, m.YearFrom, m.YearTo, NULL FROM Parts p JOIN CarModels m ON m.BrandId=p.BrandId " +
                    "WHERE p.PartId=@p AND NOT EXISTS (SELECT 1 FROM PartFitment f WHERE f.PartId=p.PartId AND f.ModelId=m.ModelId)", Db.P("@p", pid));
                Ok("Added to every model of this brand.");
            }
            else if (cmd.StartsWith("del:"))
            {
                int fid;
                if (int.TryParse(cmd.Substring(4), out fid)) Db.Exec("DELETE FROM PartFitment WHERE FitId=@f AND PartId=@p", Db.P("@f", fid), Db.P("@p", pid));
                Ok("Fitment removed.");
            }
        }
    }
}

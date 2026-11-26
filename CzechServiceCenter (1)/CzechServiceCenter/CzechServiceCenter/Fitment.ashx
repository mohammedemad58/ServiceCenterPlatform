<%@ WebHandler Language="C#" Class="CzechServiceCenter.FitmentHandler" %>
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using Newtonsoft.Json;

namespace CzechServiceCenter
{
    /// <summary>GET Fitment.ashx?type=models&amp;brand=1 | type=years&amp;model=3 | type=engines&amp;model=3[&amp;year=2020]  ->  [{"v":"3","t":"A3"}]</summary>
    public class FitmentHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            var list = new List<Dictionary<string, string>>();
            int brand, model, year;
            string type = ctx.Request.QueryString["type"];
            try
            {
                if (type == "models" && int.TryParse(ctx.Request.QueryString["brand"], out brand))
                {
                    foreach (DataRow r in Fitment.Models(brand).Rows) list.Add(Item(Convert.ToString(r["ModelId"]), Convert.ToString(r["Name"])));
                }
                else if (type == "years" && int.TryParse(ctx.Request.QueryString["model"], out model))
                {
                    foreach (int y in Fitment.Years(model)) list.Add(Item(y.ToString(), y.ToString()));
                }
                else if (type == "engines" && int.TryParse(ctx.Request.QueryString["model"], out model))
                {
                    int? yr = int.TryParse(ctx.Request.QueryString["year"], out year) ? (int?)year : null;
                    foreach (int cc in Fitment.Engines(model, yr)) list.Add(Item(cc.ToString(), Fitment.EngineLabel(cc)));
                }
                else ctx.Response.StatusCode = 400;
            }
            catch (System.Data.SqlClient.SqlException) { ctx.Response.StatusCode = 500; }
            ctx.Response.Write(JsonConvert.SerializeObject(list));
        }

        static Dictionary<string, string> Item(string v, string t)
        {
            return new Dictionary<string, string> { { "v", v }, { "t", t } };
        }
    }
}

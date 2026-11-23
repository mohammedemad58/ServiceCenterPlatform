<%@ WebHandler Language="C#" Class="CzechServiceCenter.SlotsHandler" %>
using System;
using System.Linq;
using System.Web;

namespace CzechServiceCenter
{
    /// <summary>GET /Slots.ashx?date=yyyy-MM-dd&amp;branch=1  ->  {"closed":false,"slots":["09:00","10:00"]}</summary>
    public class SlotsHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            DateTime d;
            if (!DateTime.TryParseExact(ctx.Request.QueryString["date"], "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d))
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Write("{\"closed\":false,\"slots\":[]}");
                return;
            }
            int branch;
            if (!int.TryParse(ctx.Request.QueryString["branch"], out branch) || branch <= 0)
            {
                ctx.Response.Write("{\"closed\":false,\"nobranch\":true,\"slots\":[]}");
                return;
            }
            bool closed = !Slots.IsWorkingDay(d);
            var slots = closed ? new System.Collections.Generic.List<string>() : Slots.Available(d, branch, 0);
            ctx.Response.Write("{\"closed\":" + (closed ? "true" : "false") + ",\"slots\":["
                + string.Join(",", slots.Select(s => "\"" + s + "\"").ToArray()) + "]}");
        }
    }
}

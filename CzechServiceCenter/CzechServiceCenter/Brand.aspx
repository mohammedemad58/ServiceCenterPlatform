<%@Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Brand service | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Brand.aspx.cs" Inherits="CzechServiceCenter.BrandPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1><%: Lang.T("{0} service", BrandRow["Name"]) %></h1><p><%: BrandRow["Description"] %></p>
  <a class="btn btn-red" href="<%= ResolveUrl("~/Book.aspx?brand=" + BrandRow["BrandId"]) %>"><%: Lang.T("Book a {0} service", BrandRow["Name"]) %> <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></a></div></section>
<section class="section"><div class="container">
  <h2 class="rule-title"><span>Services</span></h2>
  <div class="tile-grid">
    <% foreach (System.Data.DataRow s in Services.Rows) { %>
    <div class="card tile">
      <h3><%: s["Name"] %></h3>
      <p class="muted"><%: s["Description"] %></p>
      <p class="tile-meta"><span><%: Lang.T("About {0} min", s["DurationMinutes"]) %></span><strong><%: Lang.T("from {0}", Fmt.Money(s["PriceFrom"])) %></strong></p>
      <a class="btn btn-outline-dark btn-sm" href="<%= ResolveUrl("~/Book.aspx?brand=" + BrandRow["BrandId"] + "&service=" + s["ServiceId"]) %>">Book this service</a>
    </div>
    <% } %>
  </div>
  <h2 class="rule-title mt"><span>Popular genuine parts</span></h2>
  <div class="parts-grid">
    <% foreach (System.Data.DataRow r in Featured.Rows) { string css; string label = Maint.StockLabel(Convert.ToInt32(r["Quantity"]), out css); %>
    <article class="part-card">
      <div class="part-img"><svg class="ico" viewBox="0 0 24 24"><use href="#i-gear"/></svg></div>
      <h3><%: r["Name"] %></h3>
      <p class="muted"><%: r["PartNumber"] %></p>
      <p class="price"><%: Fmt.Money(r["Price"]) %></p>
      <span class="badge badge-<%= css %>"><%: label %></span>
    </article>
    <% } %>
  </div>
  <p class="center mt"><a class="btn btn-outline-dark" href="<%= ResolveUrl("~/Parts.aspx?brand=" + BrandRow["BrandId"]) %>"><%: Lang.T("Browse all {0} parts", BrandRow["Name"]) %></a></p>
</div></section>
</asp:Content>

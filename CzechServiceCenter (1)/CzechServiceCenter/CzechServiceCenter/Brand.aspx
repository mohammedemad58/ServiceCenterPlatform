<%@Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Brand service | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Brand.aspx.cs" Inherits="CzechServiceCenter.BrandPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1><%: Lang.T("{0} service", BrandRow["Name"]) %></h1><p><%: BrandRow["Description"] %></p>
  <a class="btn btn-red" href="<%= ResolveUrl("~/Book.aspx?brand=" + BrandRow["BrandId"]) %>"><%: Lang.T("Book a {0} service", BrandRow["Name"]) %> <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></a></div></section>
<section class="section"><div class="container">
  <h2 class="rule-title"><span>Service schedule</span></h2>
  <p class="muted">What your car needs at every mileage, and which parts to use.</p>
  <div class="chips">
    <% foreach (System.Data.DataRow s in Plans.Rows) { %><a class="chip" href="<%= ResolveUrl("~/Services.aspx?b=" + BrandRow["Slug"]) %>#km<%= s["IntervalKm"] %>"><%: Fmt.Km(s["IntervalKm"]) %></a><% } %>
  </div>
  <p><a class="btn btn-outline-dark" href="<%= ResolveUrl("~/Services.aspx?b=" + BrandRow["Slug"]) %>">See the full schedule</a></p>
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

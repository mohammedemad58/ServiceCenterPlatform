<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Service schedule | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Services.aspx.cs" Inherits="CzechServiceCenter.ServicesPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Service schedule</h1><p>What your car needs at every mileage, and the parts to use. Pick your brand and enter your mileage.</p></div></section>
<section class="section"><div class="container">
  <nav class="brand-tabs" aria-label="Brand">
    <% foreach (System.Data.DataRow b in Brands.Rows) { %><a class="<%= Convert.ToString(b["Slug"]) == Convert.ToString(BrandRow["Slug"]) ? "on" : "" %>" href="<%= ResolveUrl("~/Services.aspx?b=" + b["Slug"]) %>"><%: b["Name"] %></a><% } %>
  </nav>

  <form method="get" class="card km-form">
    <input type="hidden" name="b" value="<%: BrandRow["Slug"] %>" />
    <label>My car has driven (km)<input type="number" name="km" min="0" max="2000000" value="<%: Q("km") %>" placeholder="e.g. 47000" /></label>
    <button class="btn btn-red" type="submit">Show my next service</button>
  </form>
  <% if (NextKm > 0) { %>
  <div class="alert alert-ok"><%: Lang.T("At {0} your next service is the {1} service. It is highlighted below.", Fmt.Km(CurrentKm), Fmt.Km(PlanKm)) %></div>
  <% } %>

  <h2 class="rule-title"><span><%: Lang.T("{0} service by mileage", BrandRow["Name"]) %></span></h2>
  <div class="plan-grid">
    <% foreach (System.Data.DataRow p in Plans.Rows) { bool hit = NextKm > 0 && Convert.ToInt32(p["IntervalKm"]) == PlanKm; %>
    <article class="card plan<%= hit ? " plan-hit" : "" %>" id="km<%= p["IntervalKm"] %>">
      <h3><%: Lang.T("Every {0}", Fmt.Km(p["IntervalKm"])) %></h3>
      <h4>What we do</h4>
      <ul><% foreach (string t in Lines(p["Tasks"])) { %><li><%: t %></li><% } %></ul>
      <% if (Lines(p["PartNames"]).Length > 0) { %>
      <h4>Parts you need</h4>
      <ul><% foreach (string t in Lines(p["PartNames"])) { %><li><%: t %></li><% } %></ul>
      <a class="btn btn-outline-dark btn-sm" href="<%= ResolveUrl("~/Parts.aspx?brand=" + BrandRow["BrandId"] + "&types=" + HttpUtility.UrlEncode(Convert.ToString(p["PartCodes"]))) %>"><%: Lang.T("Shop these parts for {0}", BrandRow["Name"]) %></a>
      <% } %>
    </article>
    <% } %>
  </div>
  <p class="muted center mt-s">After 100,000 km the schedule starts again from the 10,000 km service. Always follow your owner's manual for your exact engine and gearbox.</p>
</div></section>
</asp:Content>

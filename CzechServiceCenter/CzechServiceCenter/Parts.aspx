<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Genuine Parts | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Parts.aspx.cs" Inherits="CzechServiceCenter.PartsPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Genuine Parts</h1><p>Official spare parts for Audi, &Scaron;koda, SEAT, Volkswagen &amp; CUPRA. Search the catalog and reserve what you need.</p></div></section>
<section class="section"><div class="container">
  <%= Flash() %>

  <% if (Reserving != null) { %>
  <form method="post" class="card reserve-panel">
    <%= CsrfField() %>
    <input type="hidden" name="partId" value="<%: Reserving["PartId"] %>" />
    <div>
      <h3><%: Lang.T("Reserve: {0}", Reserving["Name"]) %></h3>
      <p class="muted"><%: Reserving["PartNumber"] %> &middot; <%: Lang.T("{0} each · {1} in stock", Fmt.Money(Reserving["Price"]), Reserving["Quantity"]) %></p>
    </div>
    <div class="reserve-fields">
      <label>Quantity<input type="number" name="qty" value="1" min="1" max="10" required /></label>
      <label>My vehicle
        <select name="vehicleId"><option value="">Not specified</option>
          <% foreach (System.Data.DataRow v in MyVehicles.Rows) { %><option value="<%: v["VehicleId"] %>"><%: v["Car"] %> (<%: v["PlateNumber"] %>)</option><% } %>
        </select></label>
      <label>Add to a service booking
        <select name="bookingId"><option value="">Pick up at the center</option>
          <% foreach (System.Data.DataRow b in MyBookings.Rows) { %><option value="<%: b["BookingId"] %>"><%: Fmt.Date(b["BookingDate"]) %> <%: b["TimeSlot"] %> - <%: b["ServiceName"] %></option><% } %>
        </select></label>
      <button class="btn btn-red" type="submit">Confirm reservation</button>
      <a class="btn btn-outline-dark" href="<%= ResolveUrl("~/Parts.aspx") %>">Cancel</a>
    </div>
  </form>
  <% } %>

  <form method="get" class="filters card">
    <input type="search" name="q" value="<%: Q("q") %>" placeholder="Search by name or part number" aria-label="Search parts" />
    <select name="brand" aria-label="Brand"><%= Lookups.BrandOptions(Q("brand"), "All brands") %></select>
    <select name="cat" aria-label="Category"><%= Lookups.CategoryOptions(Q("cat"), "All types") %></select>
    <select name="stock" aria-label="Availability">
      <option value="">Any availability</option>
      <option value="in"<%= Q("stock") == "in" ? " selected" : "" %>>Available</option>
      <option value="low"<%= Q("stock") == "low" ? " selected" : "" %>>Low stock</option>
    </select>
    <button class="btn btn-red" type="submit">Search</button>
  </form>

  <p class="muted results-line"><%: Total == 1 ? Lang.T("1 part found") : Lang.T("{0} parts found", Total) %></p>

  <% if (PartRows.Rows.Count == 0) { %>
    <div class="card empty">No parts match your search. Try another name, brand or type.</div>
  <% } %>
  <div class="parts-grid">
    <% foreach (System.Data.DataRow r in PartRows.Rows) { string css; string label = Maint.StockLabel(Convert.ToInt32(r["Quantity"]), out css); %>
    <article class="part-card">
      <div class="part-img">
        <% if (!(r["ImageUrl"] is DBNull)) { %><img src="<%: ResolveUrl(Convert.ToString(r["ImageUrl"])) %>" alt="<%: r["Name"] %>" loading="lazy" />
        <% } else { %><svg class="ico" viewBox="0 0 24 24"><use href="#i-gear"/></svg><% } %>
      </div>
      <p class="part-meta"><%: r["BrandName"] %> &middot; <%: Lang.T(Convert.ToString(r["Category"])) %></p>
      <h3><%: r["Name"] %></h3>
      <p class="muted"><%: r["PartNumber"] %></p>
      <p class="price"><%: Fmt.Money(r["Price"]) %></p>
      <div class="part-foot">
        <span class="badge badge-<%= css %>"><%: label %></span>
        <% if (Convert.ToInt32(r["Quantity"]) > 0) { %><a class="btn btn-red btn-sm" href="<%= PartUrl(Convert.ToInt32(r["PartId"])) %>">Reserve</a><% } %>
      </div>
    </article>
    <% } %>
  </div>

  <% if (Pages > 1) { %>
  <nav class="pager" aria-label="Pages">
    <% if (PageNo > 1) { %><a href="<%= PageUrl(PageNo - 1) %>">Previous</a><% } %>
    <span><%: Lang.T("Page {0} of {1}", PageNo, Pages) %></span>
    <% if (PageNo < Pages) { %><a href="<%= PageUrl(PageNo + 1) %>">Next</a><% } %>
  </nav>
  <% } %>
</div></section>
</asp:Content>

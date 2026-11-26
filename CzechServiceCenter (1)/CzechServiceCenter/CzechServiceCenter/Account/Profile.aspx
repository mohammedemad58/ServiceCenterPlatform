<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="My Account | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="CzechServiceCenter.ProfilePage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>My Account</h1><p><%: Lang.T("Welcome, {0}. Manage your vehicles, bookings and service history.", Me["FullName"]) %></p>
  <a class="btn btn-red" href="<%= ResolveUrl("~/Book.aspx") %>">Book a service <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></a></div></section>
<section class="section"><div class="container">
  <%= Flash() %>

  <!-- VEHICLES -->
  <h2 class="rule-title"><span>My Vehicles</span></h2>
  <form method="post" id="vehicles">
    <%= CsrfField() %>
    <% if (Vehicles.Rows.Count == 0) { %><div class="card empty">No vehicles yet. Add your car below so we can keep track of its service history.</div><% } %>
    <div class="tile-grid">
      <% foreach (System.Data.DataRow v in Vehicles.Rows) { MaintAlert al = Maint.For(v); string vid = Convert.ToString(v["VehicleId"]); %>
      <div class="card tile vehicle">
        <h3><%: v["BrandName"] %> <%: v["Model"] %> <small>(<%: v["Year"] %>)</small></h3>
        <p class="plate"><%: v["PlateNumber"] %></p>
        <% if (!(v["Vin"] is DBNull)) { %><p class="muted"><%: Lang.T("VIN {0}", v["Vin"]) %></p><% } %>
        <p class="alert-line"><span class="badge badge-<%= al.Css %>"><%: al.Text %></span></p>
        <% if (!(v["ServiceDate"] is DBNull)) { %>
        <p class="muted"><%: Lang.T("Last service {0} at {1}", Fmt.Date(v["ServiceDate"]), Fmt.Km(v["LastKm"])) %><% if (!(v["Technician"] is DBNull)) { %> <%: Lang.T("by {0}", v["Technician"]) %><% } %><br />
          <% if (!(v["PartsChanged"] is DBNull)) { %><%: Lang.T("Parts changed: {0}", v["PartsChanged"]) %><br /><% } %><%: Lang.T("Cost: {0}", Fmt.Money(v["Cost"])) %></p>
        <% } %>
        <div class="inline-form">
          <label>Current mileage (km)<input type="number" name="km_<%= vid %>" value="<%: v["CurrentMileage"] %>" min="0" max="2000000" /></label>
          <button class="btn btn-outline-dark btn-sm" type="submit" name="cmd" value="mileage:<%= vid %>">Update</button>
          <button class="btn btn-link btn-sm" type="submit" name="cmd" value="del-vehicle:<%= vid %>" data-confirm="Remove this vehicle?">Remove</button>
        </div>
      </div>
      <% } %>
    </div>
  </form>

  <details class="card add-box" <%= Vehicles.Rows.Count == 0 ? "open" : "" %>>
    <summary>Add a vehicle</summary>
    <form method="post" class="form-grid">
      <%= CsrfField() %>
      <input type="hidden" name="cmd" value="add-vehicle" />
      <label>Brand<select name="brand" required><%= Lookups.BrandOptions(F("brand"), "Select Brand") %></select></label>
      <label>Model<input type="text" name="model" value="<%= Val("model") %>" maxlength="60" placeholder="e.g. Octavia" required /></label>
      <label>Year<input type="number" name="year" value="<%= Val("year") %>" min="1980" max="<%= DateTime.Now.Year + 1 %>" required /></label>
      <label>Plate number<input type="text" name="plate" value="<%= Val("plate") %>" maxlength="30" required /></label>
      <label>Chassis / VIN <small>(optional)</small><input type="text" name="vin" value="<%= Val("vin") %>" maxlength="30" /></label>
      <label>Current mileage (km)<input type="number" name="mileage" value="<%= Val("mileage") %>" min="0" max="2000000" /></label>
      <div class="span2"><button class="btn btn-red" type="submit">Save vehicle</button></div>
    </form>
  </details>

  <!-- BOOKINGS -->
  <h2 class="rule-title mt" id="bookings"><span>My Bookings</span></h2>
  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table">
      <thead><tr><th>Date</th><th>Time</th><th>Branch</th><th>Brand</th><th>Service</th><th>Status</th><th>Progress</th><th></th></tr></thead>
      <tbody>
      <% foreach (System.Data.DataRow b in Bookings.Rows) { bool canCancel = (Convert.ToString(b["Status"]) == "Pending" || Convert.ToString(b["Status"]) == "Approved") && Convert.ToDateTime(b["BookingDate"]) >= DateTime.Today; %>
      <tr>
        <td><%: Fmt.Date(b["BookingDate"]) %></td><td><%: b["TimeSlot"] %></td><td><%: b["BranchName"] %></td><td><%: b["BrandName"] %></td><td><%: b["ServiceName"] %></td>
        <td><span class="badge badge-<%= Fmt.StatusCss(Convert.ToString(b["Status"])) %>"><%: b["Status"] %></span></td>
        <td><span class="badge badge-<%= Fmt.StatusCss(Convert.ToString(b["WorkStatus"])) %>"><%: Fmt.Spaced(Convert.ToString(b["WorkStatus"])) %></span></td>
        <td><% if (canCancel) { %><button class="btn btn-link btn-sm" type="submit" name="cmd" value="cancel-booking:<%= b["BookingId"] %>" data-confirm="Cancel this booking?">Cancel</button><% } %></td>
      </tr>
      <% } %>
      <% if (Bookings.Rows.Count == 0) { %><tr><td colspan="8" class="muted">No bookings yet.</td></tr><% } %>
      </tbody></table></div>
  </form>

  <!-- MAINTENANCE HISTORY -->
  <h2 class="rule-title mt"><span>Maintenance History</span></h2>
  <div class="table-wrap"><table class="table">
    <thead><tr><th>Date</th><th>Vehicle</th><th>Mileage</th><th>Work done</th><th>Parts changed</th><th>Cost</th><th>Technician</th></tr></thead>
    <tbody>
    <% foreach (System.Data.DataRow m in Records.Rows) { %>
    <tr><td><%: Fmt.Date(m["ServiceDate"]) %></td><td><%: m["Car"] %><br /><small class="muted"><%: m["PlateNumber"] %></small></td><td><%: Fmt.Km(m["Mileage"]) %></td>
      <td><%: m["Description"] %></td><td><%: m["PartsChanged"] %></td><td><%: Fmt.Money(m["Cost"]) %></td><td><%: m["Technician"] %></td></tr>
    <% } %>
    <% if (Records.Rows.Count == 0) { %><tr><td colspan="7" class="muted">Your service history will appear here after your first visit.</td></tr><% } %>
    </tbody></table></div>

  <!-- PART RESERVATIONS -->
  <h2 class="rule-title mt" id="reservations"><span>My Part Reservations</span></h2>
  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table">
      <thead><tr><th>Part</th><th>Qty</th><th>Total</th><th>Status</th><th>Reserved on</th><th></th></tr></thead>
      <tbody>
      <% foreach (System.Data.DataRow r in Reservations.Rows) { %>
      <tr><td><%: r["PartName"] %><br /><small class="muted"><%: r["PartNumber"] %></small></td><td><%: r["Quantity"] %></td>
        <td><%: Fmt.Money(Convert.ToDecimal(r["Price"]) * Convert.ToInt32(r["Quantity"])) %></td>
        <td><span class="badge badge-<%= Fmt.StatusCss(Convert.ToString(r["Status"])) %>"><%: r["Status"] %></span></td>
        <td><%: Fmt.Date(r["CreatedAt"]) %></td>
        <td><% if (Convert.ToString(r["Status"]) == "Reserved") { %><button class="btn btn-link btn-sm" type="submit" name="cmd" value="cancel-res:<%= r["ReservationId"] %>" data-confirm="Cancel this reservation?">Cancel</button><% } %></td></tr>
      <% } %>
      <% if (Reservations.Rows.Count == 0) { %><tr><td colspan="6" class="muted">No reservations yet. <a href="<%= ResolveUrl("~/Parts.aspx") %>">Browse genuine parts</a>.</td></tr><% } %>
      </tbody></table></div>
  </form>

  <!-- PERSONAL DATA -->
  <h2 class="rule-title mt"><span>Personal Details</span></h2>
  <form method="post" class="card form-grid">
    <%= CsrfField() %>
    <input type="hidden" name="cmd" value="profile" />
    <label>Full name<input type="text" name="fullName" value="<%: Me["FullName"] %>" maxlength="100" required /></label>
    <label>Mobile number<input type="tel" name="phone" value="<%: Me["Phone"] %>" maxlength="30" required /></label>
    <label>Email<input type="email" value="<%: Me["Email"] %>" disabled /></label>
    <div class="span2"><button class="btn btn-outline-dark" type="submit">Save changes</button></div>
  </form>
</div></section>
</asp:Content>

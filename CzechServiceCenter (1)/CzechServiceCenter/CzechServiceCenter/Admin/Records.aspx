<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Maintenance Records | Admin" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Records.aspx.cs" Inherits="CzechServiceCenter.AdminRecords" %>
<asp:Content ContentPlaceHolderID="AdminContent" runat="server">
  <%= Flash() %>
  <details class="card add-box" open>
    <summary>Record a completed service</summary>
    <form method="post" class="form-grid">
      <%= CsrfField() %>
      <label class="span2">Vehicle
        <select name="vehicle" required><option value="">Select a vehicle</option>
          <% foreach (System.Data.DataRow v in Vehicles.Rows) { %><option value="<%: v["VehicleId"] %>"<%= F("vehicle") == Convert.ToString(v["VehicleId"]) ? " selected" : "" %>><%: v["PlateNumber"] %> - <%: v["Car"] %> - <%: v["FullName"] %></option><% } %>
        </select></label>
      <label>Service date<input type="date" name="date" value="<%= F("date").Length > 0 ? F("date") : DateTime.Today.ToString("yyyy-MM-dd") %>" required /></label>
      <label>Mileage at service (km)<input type="number" name="mileage" min="0" value="<%= Val("mileage") %>" required /></label>
      <label class="span2">Work done<textarea name="desc" rows="2" maxlength="500"><%: F("desc") %></textarea></label>
      <label class="span2">Parts changed<textarea name="parts" rows="2" maxlength="500" placeholder="e.g. Oil filter, air filter, front brake pads"><%: F("parts") %></textarea></label>
      <label>Cost (EGP)<input type="number" name="cost" min="0" step="0.01" value="<%= Val("cost") %>" required /></label>
      <label>Technician<input type="text" name="tech" maxlength="100" value="<%= Val("tech") %>" /></label>
      <label>Next service date <small>(blank = in 6 months)</small><input type="date" name="nextDate" value="<%= Val("nextDate") %>" /></label>
      <label>Next service mileage <small>(blank = +10,000 km)</small><input type="number" name="nextKm" min="0" value="<%= Val("nextKm") %>" /></label>
      <div class="span2"><button class="btn btn-red" type="submit">Save record</button></div>
    </form>
  </details>

  <h2 class="rule-title mt"><span>Latest records</span></h2>
  <div class="table-wrap"><table class="table">
    <thead><tr><th>Date</th><th>Vehicle</th><th>Owner</th><th>Mileage</th><th>Work / parts</th><th>Cost</th><th>Technician</th><th>Next service</th></tr></thead>
    <tbody>
    <% foreach (System.Data.DataRow r in Records.Rows) { %>
    <tr><td><%: Fmt.Date(r["ServiceDate"]) %></td><td><%: r["Car"] %><br /><small class="muted"><%: r["PlateNumber"] %></small></td><td><%: r["FullName"] %></td>
      <td><%: Fmt.Km(r["Mileage"]) %></td><td><%: r["Description"] %><br /><small class="muted"><%: r["PartsChanged"] %></small></td>
      <td><%: Fmt.Money(r["Cost"]) %></td><td><%: r["Technician"] %></td><td><%: Fmt.Date(r["NextServiceDate"]) %><br /><small class="muted"><%: Fmt.Km(r["NextServiceKm"]) %></small></td></tr>
    <% } %>
    <% if (Records.Rows.Count == 0) { %><tr><td colspan="8" class="muted">No records yet.</td></tr><% } %>
    </tbody></table></div>
</asp:Content>

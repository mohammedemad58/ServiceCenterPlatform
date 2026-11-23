<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Bookings | Admin" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Bookings.aspx.cs" Inherits="CzechServiceCenter.AdminBookings" %>
<asp:Content ContentPlaceHolderID="AdminContent" runat="server">
  <%= Flash() %>
  <form method="get" class="filters card">
    <select name="branch" aria-label="Branch"><%= Lookups.BranchOptions(Q("branch"), "All branches") %></select>
    <select name="status" aria-label="Status">
      <option value="">All statuses</option>
      <% foreach (string s in new string[] { "Pending", "Approved", "Completed", "Cancelled" }) { %><option value="<%= s %>"<%= Q("status") == s ? " selected" : "" %>><%= s %></option><% } %>
    </select>
    <input type="date" name="date" value="<%: Q("date") %>" aria-label="Date" />
    <button class="btn btn-red" type="submit">Filter</button>
    <a class="btn btn-outline-dark" href="Bookings.aspx">Reset</a>
  </form>

  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table table-admin">
      <thead><tr><th>#</th><th>When</th><th>Branch</th><th>Customer</th><th>Car / service</th><th>Status</th><th>Workshop progress</th><th>Reschedule</th></tr></thead>
      <tbody>
      <% foreach (System.Data.DataRow r in Rows.Rows) { string id = Convert.ToString(r["BookingId"]); string st = Convert.ToString(r["Status"]); string ws = Convert.ToString(r["WorkStatus"]); %>
      <tr>
        <td><%= id %></td>
        <td><strong><%: Fmt.Date(r["BookingDate"]) %></strong><br /><%: r["TimeSlot"] %></td>
        <td><%: r["BranchName"] %></td>
        <td><%: r["FullName"] %><% if (Convert.ToInt32(r["IsGuest"]) == 1) { %> <span class="badge badge-pending">Guest</span><% } %><br /><small class="muted"><%: r["Phone"] %></small>
          <% if (!(r["Email"] is DBNull)) { %><br /><small class="muted"><%: r["Email"] %></small><% } %></td>
        <td><%: r["BrandName"] %> <%: r["Model"] %> <small class="muted"><%: r["PlateNumber"] %></small>
          <% if (!(r["GuestCar"] is DBNull)) { %><small class="muted"><%: r["GuestCar"] %></small><% } %><br /><%: r["ServiceName"] %>
          <% if (!(r["Notes"] is DBNull)) { %><br /><small class="muted"><%: Lang.T("Note: {0}", r["Notes"]) %></small><% } %></td>
        <td><span class="badge badge-<%= Fmt.StatusCss(st) %>"><%: st %></span>
          <div class="btn-row">
            <% if (st == "Pending") { %><button class="btn btn-red btn-xs" name="cmd" value="approve:<%= id %>" type="submit">Approve</button><% } %>
            <% if (st == "Approved") { %><button class="btn btn-outline-dark btn-xs" name="cmd" value="complete:<%= id %>" type="submit">Complete</button><% } %>
            <% if (st != "Cancelled" && st != "Completed") { %><button class="btn btn-link btn-xs" name="cmd" value="cancel:<%= id %>" type="submit" data-confirm="Cancel this booking?">Cancel</button><% } %>
          </div></td>
        <td>
          <% if (st != "Cancelled") { %>
          <select name="work_<%= id %>">
            <% foreach (string w in new string[] { "Scheduled", "UnderInspection", "InProgress", "ReadyForPickup" }) { %><option value="<%= w %>"<%= ws == w ? " selected" : "" %>><%= Fmt.Spaced(w) %></option><% } %>
          </select>
          <button class="btn btn-outline-dark btn-xs" name="cmd" value="work:<%= id %>" type="submit">Set</button>
          <% } %>
        </td>
        <td>
          <% if (st != "Cancelled" && st != "Completed") { %>
          <input type="date" name="date_<%= id %>" value="<%: Fmt.IsoDate(r["BookingDate"]) %>" min="<%= DateTime.Today.ToString("yyyy-MM-dd") %>" />
          <select name="time_<%= id %>"><%= Lookups.SlotOptions(Convert.ToString(r["TimeSlot"]).Trim(), "Time") %></select>
          <button class="btn btn-outline-dark btn-xs" name="cmd" value="resched:<%= id %>" type="submit">Move</button>
          <% } %>
        </td>
      </tr>
      <% } %>
      <% if (Rows.Rows.Count == 0) { %><tr><td colspan="8" class="muted">No bookings found.</td></tr><% } %>
      </tbody></table></div>
  </form>
</asp:Content>

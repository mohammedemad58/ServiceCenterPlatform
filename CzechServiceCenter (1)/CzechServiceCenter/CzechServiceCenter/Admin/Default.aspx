<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Admin Dashboard | Czech Service Center" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CzechServiceCenter.AdminDashboard" %>
<asp:Content ContentPlaceHolderID="AdminContent" runat="server">
  <div class="stat-grid">
    <div class="card stat"><strong><%= TodayCount %></strong><span>Bookings today</span></div>
    <div class="card stat"><strong><%= PendingCount %></strong><span>Waiting for approval</span></div>
    <div class="card stat"><strong><%= InShop %></strong><span>Cars in the workshop</span></div>
    <div class="card stat"><strong><%= Customers %></strong><span>Customers</span></div>
    <div class="card stat"><strong><%= PartsCount %></strong><span>Active parts</span></div>
    <div class="card stat warn"><strong><%= LowStock %></strong><span>Low or out of stock</span></div>
  </div>

  <h2 class="rule-title mt"><span>Upcoming bookings</span></h2>
  <div class="table-wrap"><table class="table">
    <thead><tr><th>Date</th><th>Time</th><th>Customer</th><th>Branch</th><th>Car</th><th>Service</th><th>Status</th></tr></thead>
    <tbody>
    <% foreach (System.Data.DataRow r in Upcoming.Rows) { %>
    <tr><td><%: Fmt.Date(r["BookingDate"]) %></td><td><%: r["TimeSlot"] %></td><td><%: r["FullName"] %><br /><small class="muted"><%: r["Phone"] %></small></td>
      <td><%: r["BranchName"] %></td><td><%: r["BrandName"] %></td><td><%: r["ServiceName"] %></td>
      <td><span class="badge badge-<%= Fmt.StatusCss(Convert.ToString(r["Status"])) %>"><%: r["Status"] %></span></td></tr>
    <% } %>
    <% if (Upcoming.Rows.Count == 0) { %><tr><td colspan="7" class="muted">No upcoming bookings.</td></tr><% } %>
    </tbody></table></div>
  <p><a href="<%= ResolveUrl("~/Admin/Bookings.aspx") %>">Manage all bookings</a></p>

  <div class="two-col mt">
    <div>
      <h2 class="rule-title"><span>Low stock</span></h2>
      <div class="table-wrap"><table class="table">
        <thead><tr><th>Part</th><th>Qty</th></tr></thead><tbody>
        <% foreach (System.Data.DataRow r in LowParts.Rows) { %><tr><td><%: r["Name"] %><br /><small class="muted"><%: r["PartNumber"] %></small></td><td><%: r["Quantity"] %></td></tr><% } %>
        </tbody></table></div>
    </div>
    <div>
      <h2 class="rule-title"><span>Latest messages</span></h2>
      <% foreach (System.Data.DataRow r in Messages.Rows) { %>
      <div class="card msg"><strong><%: r["FullName"] %></strong> <small class="muted"><%: r["Email"] %> <%: r["Phone"] %> &middot; <%: Fmt.Date(r["CreatedAt"]) %></small>
        <p><%: r["Body"] %></p></div>
      <% } %>
      <% if (Messages.Rows.Count == 0) { %><div class="card empty">No messages yet.</div><% } %>
    </div>
  </div>
</asp:Content>

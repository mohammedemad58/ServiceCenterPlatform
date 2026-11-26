<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Orders | Admin" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="CzechServiceCenter.AdminOrders" %>
<asp:Content ContentPlaceHolderID="AdminContent" runat="server">
  <%= Flash() %>
  <form method="get" class="filters card">
    <input type="search" name="q" value="<%: Q("q") %>" placeholder="Order number, name or phone" aria-label="Search orders" />
    <select name="pay" aria-label="Payment">
      <option value="">All payments</option>
      <% foreach (string s in new string[] { "Pending", "AwaitingReview", "Paid", "Failed", "Cancelled" }) { %><option value="<%= s %>"<%= Q("pay") == s ? " selected" : "" %>><%= Fmt.Spaced(s) %></option><% } %>
    </select>
    <select name="status" aria-label="Order status">
      <option value="">All order statuses</option>
      <% foreach (string s in new string[] { "New", "Processing", "Ready", "Completed", "Cancelled" }) { %><option value="<%= s %>"<%= Q("status") == s ? " selected" : "" %>><%= s %></option><% } %>
    </select>
    <button class="btn btn-red" type="submit">Filter</button>
    <a class="btn btn-outline-dark" href="Orders.aspx">Reset</a>
  </form>

  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table table-admin">
      <thead><tr><th>#</th><th>Customer</th><th>Parts</th><th>Receive</th><th>Payment</th><th>Order</th></tr></thead>
      <tbody>
      <% foreach (System.Data.DataRow r in Rows.Rows) { string id = Convert.ToString(r["OrderId"]); string pay = Convert.ToString(r["PaymentStatus"]); string st = Convert.ToString(r["OrderStatus"]); bool needsRefund = st == "Cancelled" && pay == "Paid"; %>
      <tr>
        <td><strong><%= id %></strong><br /><small class="muted"><%: Fmt.Date(r["CreatedAt"]) %></small></td>
        <td><%: r["CustomerName"] %><br /><small class="muted" dir="ltr"><%: r["Phone"] %></small>
          <% if (!(r["Email"] is DBNull)) { %><br /><small class="muted"><%: r["Email"] %></small><% } %>
          <% if (!(r["UserId"] is DBNull)) { %><br /><span class="badge badge-info">Account</span><% } else { %><br /><span class="badge badge-warn">Guest</span><% } %></td>
        <td><details><summary><%: Fmt.Money(r["Total"]) %> (<%= ItemsOf(r["OrderId"]).Length %>)</summary>
          <% foreach (System.Data.DataRow it in ItemsOf(r["OrderId"])) { %><small><%: it["PartNumber"] %> &times; <%: it["Quantity"] %> &middot; <%: it["Name"] %></small><br /><% } %></details></td>
        <td><%: r["DeliveryMethod"] %><br /><small class="muted">
          <% if (Convert.ToString(r["DeliveryMethod"]) == "Pickup") { %><%: r["BranchName"] %><% } else { %><%: r["City"] %>, <%: r["Address"] %><% } %></small></td>
        <td><%: r["PaymentMethod"] %> <span class="badge badge-<%= Orders.StatusCss(pay) %>"><%: Fmt.Spaced(pay) %></span>
          <% if (!(r["InstaPayRef"] is DBNull)) { %><br /><small>Ref: <strong dir="ltr"><%: r["InstaPayRef"] %></strong></small><% } %>
          <% if (!(r["GatewayTxnId"] is DBNull)) { %><br /><small class="muted">Txn <%: r["GatewayTxnId"] %></small><% } %>
          <% if (!(r["Notes"] is DBNull)) { %><br /><small class="stock-warn"><%: r["Notes"] %></small><% } %>
          <% if (needsRefund) { %><br /><span class="badge badge-bad">Paid but cancelled: refund</span><% } %>
          <div class="btn-row">
            <% if (pay == "AwaitingReview" || pay == "Pending") { %><button class="btn btn-red btn-xs" type="submit" name="cmd" value="markpaid:<%= id %>" data-confirm="Confirm that the money for this order arrived?">Mark as paid</button><% } %>
          </div></td>
        <td><span class="badge badge-<%= Orders.StatusCss(st) %>"><%: st %></span>
          <div class="btn-row">
            <% if (pay == "Paid" && st != "Cancelled" && st != "Completed") { %>
              <select name="status_<%= id %>"><% foreach (string s in new string[] { "Processing", "Ready", "Completed" }) { %><option value="<%= s %>"<%= st == s ? " selected" : "" %>><%= s %></option><% } %></select>
              <button class="btn btn-outline-dark btn-xs" type="submit" name="cmd" value="status:<%= id %>">Save</button>
            <% } %>
            <% if (st != "Cancelled" && st != "Completed") { %><button class="btn btn-link btn-xs" type="submit" name="cmd" value="cancel:<%= id %>" data-confirm="Cancel this order and return the stock?">Cancel</button><% } %>
            <% if (st == "Cancelled" && pay != "Paid") { %><button class="btn btn-link btn-xs" type="submit" name="cmd" value="reopen:<%= id %>">Re-open</button><% } %>
          </div></td>
      </tr>
      <% } %>
      <% if (Rows.Rows.Count == 0) { %><tr><td colspan="6" class="empty">No orders yet.</td></tr><% } %>
      </tbody>
    </table></div>
  </form>
</asp:Content>

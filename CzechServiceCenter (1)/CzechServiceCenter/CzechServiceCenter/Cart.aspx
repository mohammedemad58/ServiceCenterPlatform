<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Your Cart | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="CzechServiceCenter.CartPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Your Cart</h1><p>Review your parts, then pay online by Visa or InstaPay. No account needed.</p></div></section>
<section class="section"><div class="container">
  <%= Flash() %>
  <% if (Lines.Rows.Count == 0) { %>
    <div class="card empty">Your cart is empty.</div>
    <p class="center"><a class="btn btn-red" href="<%= ResolveUrl("~/Parts.aspx") %>">Browse genuine parts</a></p>
  <% } else { %>
  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table cart-table">
      <thead><tr><th>Part</th><th>Price</th><th>Quantity</th><th>Total</th><th></th></tr></thead>
      <tbody>
      <% foreach (System.Data.DataRow r in Lines.Rows) { string id = Convert.ToString(r["PartId"]); bool over = Convert.ToInt32(r["Qty"]) > Convert.ToInt32(r["Stock"]); %>
      <tr>
        <td><strong><%: r["Name"] %></strong><br /><small class="muted"><%: r["BrandName"] %> &middot; <%: r["PartNumber"] %></small>
          <% if (over) { %><br /><small class="stock-warn"><%: Lang.T("Only {0} left in stock", r["Stock"]) %></small><% } %></td>
        <td><%: Fmt.Money(r["Price"]) %></td>
        <td><input type="number" name="qty_<%= id %>" value="<%: r["Qty"] %>" min="0" max="<%= Cart.MaxPerLine %>" aria-label="Quantity" class="qty-input" /></td>
        <td><strong><%: Fmt.Money(r["LineTotal"]) %></strong></td>
        <td><button class="btn btn-link btn-xs" type="submit" name="cmd" value="remove:<%= id %>">Remove</button></td>
      </tr>
      <% } %>
      </tbody>
    </table></div>
    <div class="cart-foot">
      <div><button class="btn btn-outline-dark" type="submit" name="cmd" value="update">Update cart</button>
        <a class="btn btn-link" href="<%= ResolveUrl("~/Parts.aspx") %>">Continue shopping</a></div>
      <div class="cart-total"><span>Subtotal</span> <strong><%: Fmt.Money(Subtotal) %></strong></div>
    </div>
  </form>
  <% if (StockProblem) { %><div class="alert alert-err">Some items are no longer available in the quantity you chose. Update the quantities to continue.</div>
  <% } else { %><p class="center"><a class="btn btn-red" href="<%= ResolveUrl("~/Checkout.aspx") %>">Proceed to checkout <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></a></p><% } %>
  <% } %>
</div></section>
</asp:Content>

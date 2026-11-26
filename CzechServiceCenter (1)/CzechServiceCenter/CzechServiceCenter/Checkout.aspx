<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Checkout | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="CzechServiceCenter.CheckoutPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Checkout</h1><p>No account needed. Your parts are reserved for you while you pay.</p></div></section>
<section class="section"><div class="container two-col checkout">
  <form method="post" class="card form-stack" id="checkoutForm">
    <%= Flash() %>
    <%= CsrfField() %>
    <h3>Your details</h3>
    <div class="form-grid">
      <label class="span2">Full name<input type="text" name="name" value="<%= Field("name", "FullName") %>" maxlength="100" required /></label>
      <label>Phone<input type="tel" name="phone" value="<%= Field("phone", "Phone") %>" maxlength="30" required /></label>
      <label>Email <small>(optional, for your receipt)</small><input type="email" name="email" value="<%= Field("email", "Email") %>" maxlength="150" /></label>
    </div>

    <h3>Receive your parts</h3>
    <div class="choice-row">
      <label class="choice"><input type="radio" name="delivery" value="Pickup" <%= Chosen("delivery", "Pickup", "Pickup") %> /> <span>Pick up at a branch</span></label>
      <label class="choice"><input type="radio" name="delivery" value="Delivery" <%= Chosen("delivery", "Delivery", "Pickup") %> /> <span>Delivery</span></label>
    </div>
    <div id="boxPickup">
      <label>Branch<select name="branch"><%= Lookups.BranchOptions(Request.Form["branch"], "Select a branch") %></select></label>
    </div>
    <div id="boxDelivery" class="form-grid">
      <label>City<input type="text" name="city" value="<%= Field("city", null) %>" maxlength="80" /></label>
      <label class="span2">Full address<input type="text" name="address" value="<%= Field("address", null) %>" maxlength="300" /></label>
      <% if (Fee > 0) { %><p class="muted span2"><%: Lang.T("Delivery fee: {0}", Fmt.Money(Fee)) %></p><% } %>
    </div>

    <h3>Payment</h3>
    <div class="choice-row">
      <label class="choice"><input type="radio" name="pay" value="Card" <%= Chosen("pay", "Card", "Card") %> /> <span>Visa / Mastercard (card)</span></label>
      <label class="choice"><input type="radio" name="pay" value="InstaPay" <%= Chosen("pay", "InstaPay", "Card") %> /> <span>InstaPay transfer</span></label>
    </div>
    <p class="muted" id="payHelpCard">You will be taken to the secure payment page of our payment provider. Your card details are never stored on this site.</p>
    <p class="muted" id="payHelpInsta">After you place the order you will see our InstaPay address. Transfer the total, then send us the transaction reference. We confirm it and prepare your order.</p>
    <button class="btn btn-red btn-block" type="submit">Place order and pay</button>
  </form>

  <aside class="card order-summary">
    <h3>Order summary</h3>
    <% foreach (System.Data.DataRow r in Lines.Rows) { %>
    <p class="sum-line"><span><%: r["Name"] %> &times; <%: r["Qty"] %></span><strong><%: Fmt.Money(r["LineTotal"]) %></strong></p>
    <% } %>
    <p class="sum-line"><span>Subtotal</span><strong><%: Fmt.Money(Subtotal) %></strong></p>
    <% if (Fee > 0) { %><p class="sum-line muted"><span>Delivery</span><span><%: Fmt.Money(Fee) %></span></p><% } %>
    <p class="muted"><small>Prices are in Egyptian pounds.</small></p>
    <p><a href="<%= ResolveUrl("~/Cart.aspx") %>">Edit cart</a></p>
  </aside>
</div></section>
</asp:Content>
<asp:Content ContentPlaceHolderID="Scripts" runat="server">
<script>
(function () {
  var f = document.getElementById('checkoutForm'); if (!f) return;
  function val(n) { var e = f.querySelector('input[name=' + n + ']:checked'); return e ? e.value : ''; }
  function show(id, on) { var e = document.getElementById(id); if (e) e.style.display = on ? '' : 'none'; }
  function sync() {
    var d = val('delivery') === 'Delivery', c = val('pay') === 'Card';
    show('boxPickup', !d); show('boxDelivery', d); show('payHelpCard', c); show('payHelpInsta', !c);
  }
  f.addEventListener('change', sync); sync();
})();
</script>
</asp:Content>

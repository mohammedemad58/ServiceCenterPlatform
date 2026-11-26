<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Order | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Order.aspx.cs" Inherits="CzechServiceCenter.OrderPage" %>
<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
<% if (AutoRefresh) { %><meta http-equiv="refresh" content="6" /><% } %>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<% if (Lookup) { %>
<section class="page-head"><div class="container"><h1>Track your order</h1><p>Enter your order number and the phone number you used at checkout.</p></div></section>
<section class="section"><div class="container">
  <%= Flash() %>
  <form method="post" class="card form-stack narrow">
    <%= CsrfField() %>
    <input type="hidden" name="cmd" value="find" />
    <label>Order number<input type="text" name="number" value="<%= Val("number") %>" maxlength="12" required inputmode="numeric" /></label>
    <label>Phone<input type="tel" name="phone" value="<%= Val("phone") %>" maxlength="30" required /></label>
    <button class="btn btn-red btn-block" type="submit">Find my order</button>
  </form>
</div></section>
<% } else { %>
<section class="page-head"><div class="container"><h1><%: Lang.T("Order #{0}", O["OrderId"]) %></h1>
  <p><span class="badge badge-<%= Orders.StatusCss(PayStatus) %>"><%: Lang.T(Fmt.Spaced(PayStatus)) %></span>
     <span class="badge badge-<%= Orders.StatusCss(OrdStatus) %>"><%: Lang.T(Fmt.Spaced(OrdStatus)) %></span></p></div></section>
<section class="section"><div class="container two-col checkout">
  <div>
    <%= Flash() %>

    <% if (OrdStatus == "Cancelled" && PayStatus != "Paid") { %>
    <div class="card"><h3>This order was cancelled</h3><p>The parts were released. If you already paid, please contact us with your order number.</p>
      <a class="btn btn-red" href="<%= ResolveUrl("~/Parts.aspx") %>">Browse parts</a></div>

    <% } else if (PayStatus == "Paid") { %>
    <div class="card"><h3>Payment received. Thank you!</h3>
      <% if (Convert.ToString(O["DeliveryMethod"]) == "Pickup") { %><p><%: Lang.T("We will prepare your parts for pickup at {0}. We will call you on {1} when they are ready.", O["BranchName"], O["Phone"]) %></p>
      <% } else { %><p><%: Lang.T("We will prepare your parts and arrange delivery. We will call you on {0}.", O["Phone"]) %></p><% } %>
    </div>

    <% } else if (Pay == "Card") { %>
    <div class="card">
      <% if (AutoRefresh) { %><h3>Confirming your payment...</h3><p>This page refreshes by itself. It usually takes a few seconds.</p>
      <% } else { %><h3>Pay by card</h3><p><%: Lang.T("Your parts are held for {0} minutes. Complete the payment to confirm your order.", HoldMinutes) %></p>
      <form method="post"><%= CsrfField() %><input type="hidden" name="cmd" value="paycard" /><button class="btn btn-red" type="submit">Pay now by card</button></form><% } %>
    </div>

    <% } else if (PayStatus == "AwaitingReview") { %>
    <div class="card"><h3>We are checking your transfer</h3>
      <p><%: Lang.T("Your reference: {0}. We will confirm your payment as soon as we see the transfer, then prepare your order.", O["InstaPayRef"]) %></p></div>

    <% } else { %>
    <div class="card">
      <h3>Pay with InstaPay</h3>
      <ol class="steps-list">
        <li><%: Lang.T("Open your InstaPay app and send {0} to:", Fmt.Money(O["Total"])) %>
          <div class="instapay-box"><% if (InstaPayHandle.Length > 0) { %><strong dir="ltr"><%: InstaPayHandle %></strong><% } else { %><strong>-</strong><% } %>
            <% if (InstaPayName.Length > 0) { %><br /><small><%: InstaPayName %></small><% } %>
            <% if (InstaPayPhone.Length > 0) { %><br /><small dir="ltr"><%: InstaPayPhone %></small><% } %></div></li>
        <li>Copy the transaction reference number from the InstaPay confirmation and enter it here.</li>
      </ol>
      <form method="post" class="form-stack">
        <%= CsrfField() %><input type="hidden" name="cmd" value="instapay" />
        <label>Transaction reference<input type="text" name="ref" maxlength="80" required /></label>
        <button class="btn btn-red" type="submit">I have paid: send reference</button>
      </form>
      <p class="muted"><small><%: Lang.T("Your parts are held for {0} hours. After that the order is cancelled if no reference was sent.", HoldHours) %></small></p>
    </div>
    <% } %>
  </div>

  <aside class="card order-summary">
    <h3>Order summary</h3>
    <% foreach (System.Data.DataRow r in Items.Rows) { %>
    <p class="sum-line"><span><%: r["Name"] %> &times; <%: r["Quantity"] %></span><strong><%: Fmt.Money(r["LineTotal"]) %></strong></p>
    <% } %>
    <% if (Convert.ToDecimal(O["DeliveryFee"]) > 0) { %><p class="sum-line"><span>Delivery</span><span><%: Fmt.Money(O["DeliveryFee"]) %></span></p><% } %>
    <p class="sum-line total"><span>Total</span><strong><%: Fmt.Money(O["Total"]) %></strong></p>
    <hr />
    <p><strong><%: O["CustomerName"] %></strong><br /><span dir="ltr"><%: O["Phone"] %></span></p>
    <% if (Convert.ToString(O["DeliveryMethod"]) == "Pickup") { %><p><%: Lang.T("Pickup at {0}", O["BranchName"]) %></p>
    <% } else { %><p><%: O["City"] %><br /><%: O["Address"] %></p><% } %>
    <p class="muted"><small><%: Lang.T("Ordered {0}", Fmt.Date(O["CreatedAt"])) %></small></p>
  </aside>
</div></section>
<% } %>
</asp:Content>

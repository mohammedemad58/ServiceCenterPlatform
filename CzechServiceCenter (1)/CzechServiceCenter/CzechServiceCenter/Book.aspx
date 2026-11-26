<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Book Your Service | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Book.aspx.cs" Inherits="CzechServiceCenter.BookPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Book Your Service Online</h1><p>Choose your branch and brand, select the service and pick a time that suits you.</p></div></section>
<section class="section"><div class="container narrow">
  <ol class="steps-row steps-light">
    <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-car"/></svg></span>Select Brand</li>
    <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-wrench"/></svg></span>Choose Service</li>
    <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-calendar"/></svg></span>Pick Date &amp; Time</li>
    <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-check"/></svg></span>Confirm Booking</li>
  </ol>
  <form method="post" class="card form-grid">
    <div class="span2"><%= Flash() %></div>
    <%= CsrfField() %>
    <% if (Done) { %><div class="span2"><div class="alert alert-ok" role="alert">Thank you! Your booking request was received. We will contact you by phone to confirm it.</div></div><% } %>
    <% if (Auth.IsAuthenticated) { %>
    <label>My vehicle <small>(optional)</small>
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-car"/></svg><select name="vehicle" id="vehicle">
        <option value="">Not listed / another car</option>
        <% foreach (System.Data.DataRow v in Vehicles.Rows) { %>
        <option value="<%: v["VehicleId"] %>" data-brand="<%: v["BrandId"] %>"<%= Sel("vehicle") == Convert.ToString(v["VehicleId"]) ? " selected" : "" %>><%: v["BrandName"] %> <%: v["Model"] %> (<%: v["PlateNumber"] %>)</option>
        <% } %>
      </select></span>
    </label>
    <% } else { %>
    <label>Your name
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-user"/></svg><input type="text" name="gname" maxlength="100" value="<%: F("gname") %>" required /></span>
    </label>
    <label>Phone
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-phone"/></svg><input type="tel" name="gphone" maxlength="20" value="<%: F("gphone") %>" required /></span>
    </label>
    <label>Email <small>(optional)</small>
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-mail"/></svg><input type="email" name="gemail" maxlength="150" value="<%: F("gemail") %>" /></span>
    </label>
    <label>Car model / plate <small>(optional)</small>
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-car"/></svg><input type="text" name="gcar" maxlength="120" value="<%: F("gcar") %>" placeholder="e.g. Octavia 2019, ABC 1234" /></span>
    </label>
    <div style="position:absolute;left:-9999px" aria-hidden="true"><input type="text" name="website" tabindex="-1" autocomplete="off" /></div>
    <% } %>
    <label class="span2">Branch
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-pin"/></svg><select name="branch" id="branch" required><%= Lookups.BranchOptions(Sel("branch"), "Select Branch") %></select></span>
    </label>
    <label>Brand
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-car"/></svg><select name="brand" id="brand" required><%= Lookups.BrandOptions(Sel("brand"), "Select Brand") %></select></span>
    </label>
    <label>Service
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-wrench"/></svg><select name="service" required><%= Lookups.ServiceOptions(Sel("service"), "Select Service") %></select></span>
    </label>
    <label>Date
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-calendar"/></svg><input type="date" name="date" id="bdate" data-slots="#btime" data-branch="#branch" value="<%: Sel("date") %>" min="<%= DateTime.Today.ToString("yyyy-MM-dd") %>" required /></span>
    </label>
    <label>Time
      <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-clock"/></svg><select name="time" id="btime" data-selected="<%: Sel("time") %>" required><option value="">Select Time</option></select></span>
    </label>
    <label class="span2">Notes <small>(optional)</small>
      <textarea name="notes" rows="3" maxlength="500" placeholder="Describe the problem or anything we should know"><%: F("notes") %></textarea>
    </label>
    <p class="muted span2"><%: Lang.T("Opening hours: {0}. Your booking is confirmed once the center approves it.", Hours) %><% if (!Auth.IsAuthenticated) { %> No account needed. <a href="<%= ResolveUrl("~/Login.aspx?ReturnUrl=%2FBook.aspx") %>">Log in</a> if you want to track your bookings.<% } %></p>
    <div class="span2"><button class="btn btn-red btn-block" type="submit">Confirm Booking <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></button></div>
  </form>
</div></section>
</asp:Content>

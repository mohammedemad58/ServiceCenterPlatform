<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Contact | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="CzechServiceCenter.ContactPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Contact us</h1><p>Questions about a service or a part? Send us a message and we will reply by phone or email.</p></div></section>
<section class="section"><div class="container two-col">
  <div class="card info-card">
    <h3>Visit or call</h3>
    <p><svg class="ico" viewBox="0 0 24 24"><use href="#i-phone"/></svg> <%: Phone %></p>
    <p><svg class="ico" viewBox="0 0 24 24"><use href="#i-pin"/></svg> <%: Address %></p>
    <p><svg class="ico" viewBox="0 0 24 24"><use href="#i-clock"/></svg> <%: Lang.T("Open {0}", Hours) %></p>
    <p><strong>Our branches</strong></p>
    <% foreach (System.Data.DataRow b in Branches.Rows) { %><p><svg class="ico" viewBox="0 0 24 24"><use href="#i-pin"/></svg> <%: b["Name"] %></p><% } %>
  </div>
  <form method="post" class="card form-stack">
    <%= Flash() %>
    <%= CsrfField() %>
    <label>Full name<input type="text" name="fullName" value="<%= Val("fullName") %>" maxlength="100" required /></label>
    <label>Email<input type="email" name="email" value="<%= Val("email") %>" maxlength="150" required /></label>
    <label>Phone <small>(optional)</small><input type="tel" name="phone" value="<%= Val("phone") %>" maxlength="30" /></label>
    <label>Message<textarea name="body" rows="5" maxlength="1500" required><%= Val("body") %></textarea></label>
    <button class="btn btn-red btn-block" type="submit">Send message</button>
  </form>
</div></section>
</asp:Content>

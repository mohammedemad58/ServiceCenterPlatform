<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Log in | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CzechServiceCenter.LoginPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Log in</h1><p>Access your vehicles, bookings and service history.</p></div></section>
<section class="section"><div class="container narrow-sm">
  <form method="post" class="card form-stack" autocomplete="on">
    <%= Flash() %>
    <%= CsrfField() %>
    <label>Email<input type="email" name="email" value="<%= Val("email") %>" required autofocus autocomplete="email" /></label>
    <label>Password<input type="password" name="password" required autocomplete="current-password" /></label>
    <label class="check"><input type="checkbox" name="remember" value="1" /> Keep me signed in on this device</label>
    <button class="btn btn-red btn-block" type="submit">Log in</button>
    <p class="muted center">New customer? <a href="<%= ResolveUrl("~/Register.aspx") %>">Create an account</a></p>
  </form>
</div></section>
</asp:Content>

<%@ Page Title="Create account | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="CzechServiceCenter.RegisterPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Create your account</h1><p>Book services, reserve genuine parts and follow your car's service history.</p></div></section>
<section class="section"><div class="container narrow-sm">
  <form method="post" class="card form-stack">
    <%= Flash() %>
    <%= CsrfField() %>
    <label>Full name<input type="text" name="fullName" value="<%= Val("fullName") %>" maxlength="100" required autocomplete="name" /></label>
    <label>Email<input type="email" name="email" value="<%= Val("email") %>" maxlength="150" required autocomplete="email" /></label>
    <label>Mobile number<input type="tel" name="phone" value="<%= Val("phone") %>" maxlength="30" required autocomplete="tel" /></label>
    <label>Password <small>(at least 8 characters)</small><input type="password" name="password" minlength="8" required autocomplete="new-password" /></label>
    <label>Confirm password<input type="password" name="confirm" minlength="8" required autocomplete="new-password" /></label>
    <button class="btn btn-red btn-block" type="submit">Create account</button>
    <p class="muted center">Already registered? <a href="<%= ResolveUrl("~/Login.aspx") %>">Log in</a></p>
  </form>
</div></section>
</asp:Content>

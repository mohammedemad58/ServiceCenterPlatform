<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Test payment | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PayTest.aspx.cs" Inherits="CzechServiceCenter.PayTestPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Test payment</h1><p>Test mode: no money is charged and no card is needed.</p></div></section>
<section class="section"><div class="container">
  <form method="post" class="card form-stack narrow">
    <%= CsrfField() %>
    <p><%: Lang.T("Order #{0}", O["OrderId"]) %>: <strong><%: Fmt.Money(O["Total"]) %></strong></p>
    <button class="btn btn-red" type="submit" name="cmd" value="ok">Simulate successful payment</button>
    <button class="btn btn-outline-dark" type="submit" name="cmd" value="fail">Simulate failed payment</button>
  </form>
</div></section>
</asp:Content>

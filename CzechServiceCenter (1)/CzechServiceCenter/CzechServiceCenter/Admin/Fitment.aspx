<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Fitment | Admin" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Fitment.aspx.cs" Inherits="CzechServiceCenter.AdminFitment" %>
<asp:Content ContentPlaceHolderID="AdminContent" runat="server">
  <%= Flash() %>
  <p class="muted">Customers filter by brand, model, year and engine size. A part only appears for a car when it has a fitment line for that model. Leave the engine empty when the part fits every engine of the model.</p>
  <form method="get" class="filters card">
    <input type="search" name="q" value="<%: Q("q") %>" placeholder="Part number or name" aria-label="Find a part" />
    <button class="btn btn-red" type="submit">Find part</button>
  </form>

  <% if (Found.Rows.Count > 0) { %>
  <div class="table-wrap"><table class="table"><thead><tr><th>Part</th><th>Brand</th><th>Fitment lines</th><th></th></tr></thead><tbody>
    <% foreach (System.Data.DataRow r in Found.Rows) { %>
    <tr><td><%: r["PartNumber"] %><br /><small class="muted"><%: r["Name"] %></small></td><td><%: r["BrandName"] %></td><td><%: r["Fits"] %></td>
      <td><a class="btn btn-outline-dark btn-xs" href="Fitment.aspx?q=<%= HttpUtility.UrlEncode(Q("q")) %>&part=<%= r["PartId"] %>">Edit fitment</a></td></tr>
    <% } %>
  </tbody></table></div>
  <% } %>

  <% if (Part != null) { %>
  <h2 class="rule-title mt"><span><%: Part["PartNumber"] %> &middot; <%: Part["Name"] %></span></h2>
  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table table-admin"><thead><tr><th>Model</th><th>Years</th><th>Engine</th><th></th></tr></thead><tbody>
      <% foreach (System.Data.DataRow f in FitRows.Rows) { %>
      <tr><td><%: Part["BrandName"] %> <%: f["Model"] %></td><td><%: f["YearFrom"] %> - <%: f["YearTo"] %></td>
        <td><%: f["EngineCc"] is DBNull ? "All engines" : Convert.ToString(f["EngineCc"]) + " cc" %></td>
        <td><button class="btn btn-link btn-xs" type="submit" name="cmd" value="del:<%= f["FitId"] %>">Remove</button></td></tr>
      <% } %>
      <% if (FitRows.Rows.Count == 0) { %><tr><td colspan="4" class="empty">This part fits no model yet, so customers will not see it when they pick a car.</td></tr><% } %>
    </tbody></table></div>
    <p><button class="btn btn-outline-dark btn-sm" type="submit" name="cmd" value="allmodels" data-confirm="Add this part to every model of this brand?">Fits every model of this brand</button></p>
  </form>
  <form method="post" class="card">
    <%= CsrfField() %>
    <input type="hidden" name="cmd" value="add" />
    <div class="form-grid">
      <label>Model<select name="model" required><option value="">Select model</option>
        <% foreach (System.Data.DataRow m in ModelList.Rows) { %><option value="<%= m["ModelId"] %>"><%: m["Name"] %> (<%: m["YearFrom"] %>-<%: m["YearTo"] %>)</option><% } %></select></label>
      <label>Engine size in cc <small>(empty = all engines)</small><input type="number" name="cc" min="500" max="8000" /></label>
      <label>Year from<input type="number" name="from" min="1980" max="2100" required /></label>
      <label>Year to<input type="number" name="to" min="1980" max="2100" required /></label>
      <div class="span2"><button class="btn btn-red" type="submit">Add fitment line</button></div>
    </div>
  </form>
  <% } %>
</asp:Content>

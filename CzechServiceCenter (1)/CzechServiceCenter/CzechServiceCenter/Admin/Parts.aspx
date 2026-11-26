<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Parts & Inventory | Admin" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Parts.aspx.cs" Inherits="CzechServiceCenter.AdminParts" %>
<asp:Content ContentPlaceHolderID="AdminContent" runat="server">
  <%= Flash() %>

  <details class="card add-box" <%= (Editing != null || ShowNew) ? "open" : "" %>>
    <summary><%: Editing != null ? Lang.T("Edit part {0}", Convert.ToString(Editing["PartNumber"])) : Lang.T("Add a new part") %></summary>
    <form method="post" enctype="multipart/form-data" class="form-grid">
      <%= CsrfField() %>
      <input type="hidden" name="cmd" value="save" />
      <input type="hidden" name="partId" value="<%= Editing != null ? Convert.ToString(Editing["PartId"]) : "0" %>" />
      <label>Part number<input type="text" name="number" maxlength="40" required value="<%: Field("number", "PartNumber") %>" /></label>
      <label>Name<input type="text" name="name" maxlength="150" required value="<%: Field("name", "Name") %>" /></label>
      <label>Name (Arabic)<input type="text" name="nameAr" maxlength="200" dir="rtl" value="<%: Field("nameAr", "NameAr") %>" /></label>
      <label>Brand<select name="brand" required><%= Lookups.BrandOptions(Field("brand", "BrandId"), "Select Brand") %></select></label>
      <label>Category<input type="text" name="category" list="cats" maxlength="50" required value="<%: Field("category", "Category") %>" />
        <datalist id="cats"><option value="Filters" /><option value="Brakes" /><option value="Engine" /><option value="Suspension" /><option value="Electrical" /><option value="Fluids" /><option value="Body" /><option value="Accessories" /></datalist></label>
      <label>Price (EGP)<input type="number" name="price" min="0" step="0.01" required value="<%: Field("price", "Price") %>" /></label>
      <label>Quantity in stock<input type="number" name="qty" min="0" required value="<%: Field("qty", "Quantity") %>" /></label>
      <label class="span2">Description<textarea name="desc" rows="2" maxlength="500"><%: Field("desc", "Description") %></textarea></label>
      <label class="span2">Description (Arabic)<textarea name="descAr" rows="2" maxlength="500" dir="rtl"><%: Field("descAr", "DescriptionAr") %></textarea></label>
      <label>Image <small>(JPG, PNG or WebP, max 2 MB)</small><input type="file" name="image" accept=".jpg,.jpeg,.png,.webp" /></label>
      <label class="check"><input type="checkbox" name="active" value="1" <%= (Editing == null || Convert.ToBoolean(Editing["IsActive"])) ? "checked" : "" %> /> Visible in the catalog</label>
      <div class="span2 btn-row"><button class="btn btn-red" type="submit">Save part</button>
        <% if (Editing != null) { %><a class="btn btn-outline-dark" href="Parts.aspx">Cancel</a><% } %></div>
    </form>
  </details>

  <form method="get" class="filters card">
    <input type="search" name="q" value="<%: Q("q") %>" placeholder="Search name or part number" />
    <select name="brand"><%= Lookups.BrandOptions(Q("brand"), "All brands") %></select>
    <select name="stock"><option value="">Any stock</option><option value="low"<%= Q("stock") == "low" ? " selected" : "" %>>Low or out of stock</option></select>
    <button class="btn btn-red" type="submit">Search</button>
    <a class="btn btn-outline-dark" href="Parts.aspx?new=1">Add part</a>
  </form>
  <p class="muted results-line"><%: Lang.T("{0} parts", Total) %></p>

  <form method="post">
    <%= CsrfField() %>
    <div class="table-wrap"><table class="table table-admin">
      <thead><tr><th>Part</th><th>Brand</th><th>Category</th><th>Price</th><th>Stock</th><th>Status</th><th></th></tr></thead>
      <tbody>
      <% foreach (System.Data.DataRow r in PartRows.Rows) { string id = Convert.ToString(r["PartId"]); string css; string label = Maint.StockLabel(Convert.ToInt32(r["Quantity"]), out css); %>
      <tr>
        <td><%: r["Name"] %><br /><small class="muted"><%: r["PartNumber"] %></small></td>
        <td><%: r["BrandName"] %></td><td><%: Lang.T(Convert.ToString(r["Category"])) %></td><td><%: Fmt.Money(r["Price"]) %></td>
        <td class="stock-cell"><input type="number" name="qty_<%= id %>" value="<%: r["Quantity"] %>" min="0" />
          <button class="btn btn-outline-dark btn-xs" name="cmd" value="stock:<%= id %>" type="submit">Update</button>
          <span class="badge badge-<%= css %>"><%: label %></span></td>
        <td><%= Convert.ToBoolean(r["IsActive"]) ? "Visible" : "<span class=\"muted\">Hidden</span>" %></td>
        <td class="btn-row"><a class="btn btn-outline-dark btn-xs" href="Parts.aspx?edit=<%= id %>">Edit</a>
          <button class="btn btn-link btn-xs" name="cmd" value="delete:<%= id %>" type="submit" data-confirm="Delete this part?">Delete</button></td>
      </tr>
      <% } %>
      </tbody></table></div>
  </form>

  <% if (Pages > 1) { %>
  <nav class="pager"><% if (PageNo > 1) { %><a href="<%= PageUrl(PageNo - 1) %>">Previous</a><% } %><span><%: Lang.T("Page {0} of {1}", PageNo, Pages) %></span><% if (PageNo < Pages) { %><a href="<%= PageUrl(PageNo + 1) %>">Next</a><% } %></nav>
  <% } %>
</asp:Content>

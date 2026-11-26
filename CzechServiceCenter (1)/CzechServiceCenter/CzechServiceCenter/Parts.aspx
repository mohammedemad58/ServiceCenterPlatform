<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Genuine Parts | Czech Service Center" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Parts.aspx.cs" Inherits="CzechServiceCenter.PartsPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<section class="page-head"><div class="container"><h1>Genuine Parts</h1><p>Official spare parts for Audi, &Scaron;koda, SEAT, Volkswagen &amp; CUPRA. Choose your car, add parts to the cart and pay online. No account needed.</p></div></section>
<section class="section"><div class="container">
  <%= Flash() %>
  <% if (Q("added") == "1") { %><p class="center"><a class="btn btn-red btn-sm" href="<%= ResolveUrl("~/Cart.aspx") %>">View cart and checkout</a></p><% } %>

  <form method="get" class="card finder">
    <h3 class="finder-title">Find parts for your car</h3>
    <div class="finder-row">
      <label>Brand<select name="brand" id="fBrand" aria-label="Brand"><%= Lookups.BrandOptions(Q("brand"), "All brands") %></select></label>
      <label>Model<select name="model" id="fModel" aria-label="Model" data-ph="<%: Lang.T("All models") %>"><%= Fitment.ModelOptions(Q("brand"), Q("model")) %></select></label>
      <label>Year<select name="year" id="fYear" aria-label="Year" data-ph="<%: Lang.T("Any year") %>"><%= Fitment.YearOptions(Q("model"), Q("year")) %></select></label>
      <label>Engine size<select name="cc" id="fCc" aria-label="Engine size" data-ph="<%: Lang.T("Any engine") %>"><%= Fitment.EngineOptions(Q("model"), Q("year"), Q("cc")) %></select></label>
    </div>
    <div class="finder-row finder-row2">
      <input type="search" name="q" value="<%: Q("q") %>" placeholder="Search by name or part number" aria-label="Search parts" />
      <select name="cat" aria-label="Category"><%= Lookups.CategoryOptions(Q("cat"), "All types") %></select>
      <select name="stock" aria-label="Availability">
        <option value="">Any availability</option>
        <option value="in"<%= Q("stock") == "in" ? " selected" : "" %>>Available</option>
        <option value="low"<%= Q("stock") == "low" ? " selected" : "" %>>Low stock</option>
      </select>
      <button class="btn btn-red" type="submit">Search</button>
      <a class="btn btn-outline-dark" href="<%= ResolveUrl("~/Parts.aspx") %>">Reset</a>
    </div>
    <% if (Q("types").Length > 0) { %><input type="hidden" name="types" value="<%: Q("types") %>" /><p class="muted finder-note"><%: Lang.T("Showing only the parts needed for the chosen service.") %></p><% } %>
  </form>

  <p class="muted results-line"><%: Total == 1 ? Lang.T("1 part found") : Lang.T("{0} parts found", Total) %></p>

  <% if (PartRows.Rows.Count == 0) { %>
    <div class="card empty">No parts match your search. Try another name, brand or type.</div>
  <% } %>
  <div class="parts-grid">
    <% foreach (System.Data.DataRow r in PartRows.Rows) { string css; int stock = Convert.ToInt32(r["Quantity"]); string label = Maint.StockLabel(stock, out css); %>
    <article class="part-card">
      <div class="part-img">
        <% if (!(r["ImageUrl"] is DBNull)) { %><img src="<%: ResolveUrl(Convert.ToString(r["ImageUrl"])) %>" alt="<%: r["Name"] %>" loading="lazy" />
        <% } else { %><svg class="ico" viewBox="0 0 24 24"><use href="#i-gear"/></svg><% } %>
      </div>
      <p class="part-meta"><%: r["BrandName"] %> &middot; <%: Lang.T(Convert.ToString(r["Category"])) %></p>
      <h3><%: r["Name"] %></h3>
      <p class="muted"><%: r["PartNumber"] %></p>
      <p class="price"><%: Fmt.Money(r["Price"]) %></p>
      <div class="part-foot">
        <span class="badge badge-<%= css %>"><%: label %></span>
      </div>
      <% if (stock > 0) { %>
      <form method="post" action="<%: Request.RawUrl %>" class="add-form">
        <%= CsrfField() %>
        <input type="hidden" name="partId" value="<%: r["PartId"] %>" />
        <input type="number" name="qty" value="1" min="1" max="<%= Math.Min(stock, Cart.MaxPerLine) %>" aria-label="Quantity" />
        <button class="btn btn-red btn-sm" type="submit">Add to cart</button>
      </form>
      <% } %>
    </article>
    <% } %>
  </div>

  <% if (Pages > 1) { %>
  <nav class="pager" aria-label="Pages">
    <% if (PageNo > 1) { %><a href="<%= PageUrl(PageNo - 1) %>">Previous</a><% } %>
    <span><%: Lang.T("Page {0} of {1}", PageNo, Pages) %></span>
    <% if (PageNo < Pages) { %><a href="<%= PageUrl(PageNo + 1) %>">Next</a><% } %>
  </nav>
  <% } %>
</div></section>
</asp:Content>
<asp:Content ContentPlaceHolderID="Scripts" runat="server">
<script>
(function () {
  var root = document.body.getAttribute('data-root') || '/';
  var b = document.getElementById('fBrand'), m = document.getElementById('fModel'), y = document.getElementById('fYear'), c = document.getElementById('fCc');
  if (!b || !m || !y || !c) return;
  function fill(sel, items) {
    sel.innerHTML = '';
    var o = document.createElement('option'); o.value = ''; o.textContent = sel.getAttribute('data-ph'); sel.appendChild(o);
    items.forEach(function (i) { var x = document.createElement('option'); x.value = i.v; x.textContent = i.t; sel.appendChild(x); });
  }
  function get(q) { return fetch(root + 'Fitment.ashx?' + q, { credentials: 'same-origin' }).then(function (r) { return r.json(); }); }
  b.addEventListener('change', function () {
    fill(y, []); fill(c, []); fill(m, []);
    if (b.value) get('type=models&brand=' + encodeURIComponent(b.value)).then(function (d) { fill(m, d); });
  });
  m.addEventListener('change', function () {
    fill(y, []); fill(c, []);
    if (!m.value) return;
    get('type=years&model=' + encodeURIComponent(m.value)).then(function (d) { fill(y, d); });
    get('type=engines&model=' + encodeURIComponent(m.value)).then(function (d) { fill(c, d); });
  });
  y.addEventListener('change', function () {
    if (!m.value) return;
    var keep = c.value;
    get('type=engines&model=' + encodeURIComponent(m.value) + (y.value ? '&year=' + encodeURIComponent(y.value) : '')).then(function (d) {
      fill(c, d); c.value = keep; if (c.value !== keep) c.value = '';
    });
  });
})();
</script>
</asp:Content>

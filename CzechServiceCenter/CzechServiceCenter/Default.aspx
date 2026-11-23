<%@ Import Namespace="CzechServiceCenter" %>
<%@ Page Title="Czech Service Center | Audi, Škoda, SEAT, Volkswagen & CUPRA service" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Inherits="CzechServiceCenter.BasePage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">

<!-- HERO -->
<section class="hero">
  <div class="container hero-inner">
    <h1>Your Trusted<br />Service Partner</h1>
    <p class="hero-sub">for Audi, &Scaron;koda, SEAT,<br />Volkswagen &amp; CUPRA</p>
    <a class="btn btn-outline" href="<%= ResolveUrl("~/Book.aspx") %>">Book Your Service <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></a>
  </div>
</section>

<!-- BRANDS -->
<section class="brands" id="brands">
  <div class="container">
    <h2 class="rule-title"><span>Select Your Brand</span></h2>
    <div class="brand-grid">
      <a class="brand-card" href="<%= ResolveUrl("~/Brand.aspx") %>?b=audi">
        <svg class="bl" viewBox="0 0 120 30" aria-hidden="true"><g fill="none" stroke="#fff" stroke-width="2.6"><circle cx="21" cy="15" r="12"/><circle cx="45" cy="15" r="12"/><circle cx="69" cy="15" r="12"/><circle cx="93" cy="15" r="12"/></g></svg>
        <span class="bn audi">Audi</span>
        <i class="go"><svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></i>
      </a>
      <a class="brand-card" href="<%= ResolveUrl("~/Brand.aspx") %>?b=skoda">
        <svg class="bl" viewBox="0 0 60 60" aria-hidden="true"><circle cx="30" cy="30" r="27" fill="#fff"/><circle cx="30" cy="30" r="22" fill="#0e8a45"/><path d="M18 36c8 4 16-2 22-12l-3 12-9-2z" fill="#fff"/></svg>
        <span class="bn">&Scaron;KODA</span>
        <i class="go"><svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></i>
      </a>
      <a class="brand-card" href="<%= ResolveUrl("~/Brand.aspx") %>?b=seat">
        <svg class="bl" viewBox="0 0 60 60" aria-hidden="true"><path d="M44 12H22a9 9 0 0 0 0 18h14a3 3 0 0 1 0 6H14v8h24a11 11 0 0 0 0-22H24a3 3 0 0 1 0-6h20z" fill="#fff"/></svg>
        <span class="bn spaced">SEAT</span>
        <i class="go"><svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></i>
      </a>
      <a class="brand-card" href="<%= ResolveUrl("~/Brand.aspx") %>?b=volkswagen">
        <svg class="bl" viewBox="0 0 60 60" aria-hidden="true"><circle cx="30" cy="30" r="27" fill="none" stroke="#fff" stroke-width="3"/><path d="M14 16l12 28 4-10 4 10 12-28M22 16l8 18 8-18" fill="none" stroke="#fff" stroke-width="3" stroke-linejoin="round"/></svg>
        <span class="bn">Volkswagen</span>
        <i class="go"><svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></i>
      </a>
      <a class="brand-card" href="<%= ResolveUrl("~/Brand.aspx") %>?b=cupra">
        <svg class="bl" viewBox="0 0 60 60" aria-hidden="true"><path d="M30 6l22 12-8 32-14-14-14 14-8-32z" fill="none" stroke="#c98b5a" stroke-width="4" stroke-linejoin="round"/><path d="M30 20l9 10-9 10-9-10z" fill="#c98b5a"/></svg>
        <span class="bn spaced">CUPRA</span>
        <i class="go"><svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></i>
      </a>
    </div>
  </div>
</section>

<!-- ABOUT -->
<section class="about" id="about">
  <div class="about-img" role="img" aria-label="Czech Service Center workshop building"></div>
  <div class="about-copy">
    <h2 class="red-top">About Czech Service Center</h2>
    <p>The Czech Service Center is your specialized destination for professional maintenance and repair services for Audi, &Scaron;koda, SEAT, Volkswagen and CUPRA.</p>
    <p>We use the latest diagnostic technology and genuine parts to keep your car in perfect condition, with the highest standards of quality and care.</p>
    <ul class="features">
      <li><svg class="ico" viewBox="0 0 24 24"><use href="#i-shield"/></svg><span>Expert Technicians</span></li>
      <li><svg class="ico" viewBox="0 0 24 24"><use href="#i-gear"/></svg><span>Advanced Diagnostics</span></li>
      <li><svg class="ico" viewBox="0 0 24 24"><use href="#i-award"/></svg><span>Genuine Parts</span></li>
      <li><svg class="ico" viewBox="0 0 24 24"><use href="#i-heart"/></svg><span>Total Trust</span></li>
    </ul>
  </div>
</section>

<!-- GENUINE PARTS -->
<section class="parts-banner">
  <div class="parts-img" role="img" aria-label="Genuine spare parts"></div>
  <div class="container parts-copy">
    <div>
      <h2 class="red-top">Genuine Parts<br />for Lasting Performance</h2>
      <p>We are the official importer of genuine spare parts for Audi, &Scaron;koda, SEAT, Volkswagen &amp; CUPRA.</p>
    </div>
    <a class="btn btn-outline btn-sm" href="<%= ResolveUrl("~/Parts.aspx") %>">Learn More <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></a>
  </div>
</section>

<!-- BOOKING -->
<section class="book-home" id="book">
  <div class="container book-home-grid">
    <div>
      <h2 class="red-top">Book Your Service Online</h2>
      <p>Choose your brand, select the service, and book your appointment in just a few clicks.</p>
      <ol class="steps-row">
        <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-car"/></svg></span>Select Brand</li>
        <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-wrench"/></svg></span>Choose Service</li>
        <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-calendar"/></svg></span>Pick Date &amp; Time</li>
        <li><span class="step-ico"><svg class="ico" viewBox="0 0 24 24"><use href="#i-check"/></svg></span>Confirm Booking</li>
      </ol>
    </div>
    <form class="book-card" method="get" action="<%= ResolveUrl("~/Book.aspx") %>">
      <h3>Book Your Service</h3>
      <label>Branch
        <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-pin"/></svg><select name="branch" id="hbranch"><%= Lookups.BranchOptions("", "Select Branch") %></select></span>
      </label>
      <label>Brand
        <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-car"/></svg><select name="brand"><%= Lookups.BrandOptions("", "Select Brand") %></select></span>
      </label>
      <label>Service
        <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-wrench"/></svg><select name="service"><%= Lookups.ServiceOptions("", "Select Service") %></select></span>
      </label>
      <div class="two">
        <label>Date
          <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-calendar"/></svg><input type="date" name="date" id="hdate" data-slots="#htime" data-branch="#hbranch" min="<%= DateTime.Today.ToString("yyyy-MM-dd") %>" /></span>
        </label>
        <label>Time
          <span class="ifield"><svg class="ico" viewBox="0 0 24 24"><use href="#i-clock"/></svg><select name="time" id="htime"><option value="">Select Time</option></select></span>
        </label>
      </div>
      <button class="btn btn-red btn-block" type="submit">Book Now <svg class="ico" viewBox="0 0 24 24"><use href="#i-arrow"/></svg></button>
    </form>
  </div>
</section>

</asp:Content>

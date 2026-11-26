(function () {
  var root = document.body.getAttribute('data-root') || '/';

  // mobile navigation
  var toggle = document.querySelector('.nav-toggle');
  var nav = document.getElementById('mainNav');
  if (toggle && nav) {
    toggle.addEventListener('click', function () {
      var open = nav.classList.toggle('open');
      toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
  }

  // available time slots for the chosen date: <input type="date" data-slots="#time">
  function loadSlots(dateEl) {
    var target = document.querySelector(dateEl.getAttribute('data-slots'));
    if (!target) return;
    var wanted = target.getAttribute('data-selected') || target.value;
    target.innerHTML = '<option value="">Select Time</option>';
    if (!dateEl.value) return;
    target.disabled = true;
    fetch(root + 'Slots.ashx?date=' + encodeURIComponent(dateEl.value), { credentials: 'same-origin' })
      .then(function (r) { return r.json(); })
      .then(function (d) {
        target.disabled = false;
        if (d.closed) { target.innerHTML = '<option value="">Closed on this day</option>'; return; }
        if (!d.slots.length) { target.innerHTML = '<option value="">Fully booked</option>'; return; }
        d.slots.forEach(function (s) {
          var o = document.createElement('option');
          o.value = s; o.textContent = s;
          if (s === wanted) o.selected = true;
          target.appendChild(o);
        });
      })
      .catch(function () { target.disabled = false; });
  }
  document.querySelectorAll('input[data-slots]').forEach(function (el) {
    el.addEventListener('change', function () {
      var t = document.querySelector(el.getAttribute('data-slots'));
      if (t) t.removeAttribute('data-selected');
      loadSlots(el);
    });
    if (el.value) loadSlots(el);
  });

  // choosing a saved vehicle pre-selects its brand
  var veh = document.getElementById('vehicle');
  var brand = document.getElementById('brand');
  if (veh && brand) {
    veh.addEventListener('change', function () {
      var o = veh.options[veh.selectedIndex];
      var b = o && o.getAttribute('data-brand');
      if (b) brand.value = b;
    });
  }

  // confirm destructive actions: <button data-confirm="Are you sure?">
  document.addEventListener('click', function (e) {
    var b = e.target.closest && e.target.closest('[data-confirm]');
    if (b && !window.confirm(b.getAttribute('data-confirm'))) e.preventDefault();
  });
})();

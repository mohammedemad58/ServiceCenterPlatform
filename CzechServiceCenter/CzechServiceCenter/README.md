# Czech Service Center Platform
ASP.NET Web Forms (.NET Framework 4.8), C#, SQL Server.

## Run it
1. Open `Database/schema.sql` in SSMS and run it (creates the `CzechServiceCenter` database, tables, 5 brands, 8 services and 330 genuine parts).
2. Check the connection string `CSC` in `Web.config` (default `.\SQLEXPRESS`; a LocalDB string is in the comment).
3. Open `CzechServiceCenter.csproj` in Visual Studio 2019/2022 (ASP.NET workload) and press F5.
4. Admin login (created on first start from `Web.config`): `admin@czechservice.local` / `Admin@12345`. Change it before going live.

## What is inside
- Customers: register/login, vehicles (brand, model, year, plate, VIN, mileage), bookings, part reservations, maintenance history, next-service alerts (by date or km).
- Booking: slots come from `Slots.ashx`; capacity (`Bays`), hours and closed day are in `Web.config`. Capacity is re-checked in a serializable transaction, so a slot cannot be double booked.
- Parts: search/filter/paging, stock status (Available / Low / Out), reserving reduces stock, cancelling restores it.
- Admin (`/Admin`): dashboard, approve / cancel / reschedule bookings, workshop status (Under Inspection / In Progress / Ready for Pickup), parts CRUD with image upload and stock, maintenance records.
- Security: PBKDF2 password hashing, forms auth with roles, parameterized SQL, CSRF token on every POST.

## Images
`Content/img/hero.jpg`, `about.jpg`, `parts.jpg` are low-resolution crops of the mockup. Replace them with real photos (same file names).

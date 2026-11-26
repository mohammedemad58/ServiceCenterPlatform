/* Czech Service Center Platform - SQL Server schema + seed data
   Run in SSMS (connect to .\SQLEXPRESS or (localdb)\MSSQLLocalDB). Safe to re-run. */
IF DB_ID(N'CzechServiceCenter') IS NULL CREATE DATABASE CzechServiceCenter;
GO
USE CzechServiceCenter;
GO
IF OBJECT_ID('dbo.Users') IS NULL
CREATE TABLE dbo.Users(
  UserId INT IDENTITY(1,1) PRIMARY KEY,
  FullName NVARCHAR(100) NOT NULL,
  Email NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
  Phone NVARCHAR(30) NULL,
  PasswordHash NVARCHAR(200) NOT NULL,
  Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Customer',
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_Created DEFAULT SYSUTCDATETIME());
GO
IF OBJECT_ID('dbo.Brands') IS NULL
CREATE TABLE dbo.Brands(
  BrandId INT NOT NULL PRIMARY KEY,
  Name NVARCHAR(50) NOT NULL,
  Slug NVARCHAR(30) NOT NULL CONSTRAINT UQ_Brands_Slug UNIQUE,
  Prefix CHAR(3) NOT NULL,
  PriceFactor DECIMAL(4,2) NOT NULL DEFAULT 1,
  Description NVARCHAR(400) NULL);
GO
IF OBJECT_ID('dbo.Services') IS NULL
CREATE TABLE dbo.Services(
  ServiceId INT IDENTITY(1,1) PRIMARY KEY,
  Name NVARCHAR(100) NOT NULL,
  Description NVARCHAR(300) NULL,
  DurationMinutes INT NOT NULL DEFAULT 60,
  PriceFrom DECIMAL(10,2) NOT NULL DEFAULT 0);
GO
IF OBJECT_ID('dbo.Vehicles') IS NULL
CREATE TABLE dbo.Vehicles(
  VehicleId INT IDENTITY(1,1) PRIMARY KEY,
  UserId INT NOT NULL CONSTRAINT FK_Vehicles_User REFERENCES dbo.Users(UserId),
  BrandId INT NOT NULL CONSTRAINT FK_Vehicles_Brand REFERENCES dbo.Brands(BrandId),
  Model NVARCHAR(60) NOT NULL,
  [Year] INT NOT NULL,
  PlateNumber NVARCHAR(30) NOT NULL,
  Vin NVARCHAR(30) NULL,
  CurrentMileage INT NOT NULL DEFAULT 0,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
GO
IF OBJECT_ID('dbo.Bookings') IS NULL
CREATE TABLE dbo.Bookings(
  BookingId INT IDENTITY(1,1) PRIMARY KEY,
  UserId INT NOT NULL CONSTRAINT FK_Bookings_User REFERENCES dbo.Users(UserId),
  VehicleId INT NULL CONSTRAINT FK_Bookings_Vehicle REFERENCES dbo.Vehicles(VehicleId) ON DELETE SET NULL,
  BrandId INT NOT NULL CONSTRAINT FK_Bookings_Brand REFERENCES dbo.Brands(BrandId),
  ServiceId INT NOT NULL CONSTRAINT FK_Bookings_Service REFERENCES dbo.Services(ServiceId),
  BookingDate DATE NOT NULL,
  TimeSlot CHAR(5) NOT NULL,
  Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',          -- Pending | Approved | Completed | Cancelled
  WorkStatus NVARCHAR(30) NOT NULL DEFAULT 'Scheduled',    -- Scheduled | UnderInspection | InProgress | ReadyForPickup
  Notes NVARCHAR(500) NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Bookings_Slot')
  CREATE INDEX IX_Bookings_Slot ON dbo.Bookings(BookingDate, TimeSlot) INCLUDE (Status);
GO
IF OBJECT_ID('dbo.Parts') IS NULL
CREATE TABLE dbo.Parts(
  PartId INT IDENTITY(1,1) PRIMARY KEY,
  PartNumber NVARCHAR(40) NOT NULL CONSTRAINT UQ_Parts_Number UNIQUE,
  Name NVARCHAR(150) NOT NULL,
  BrandId INT NOT NULL CONSTRAINT FK_Parts_Brand REFERENCES dbo.Brands(BrandId),
  Category NVARCHAR(50) NOT NULL,
  Description NVARCHAR(500) NULL,
  Price DECIMAL(10,2) NOT NULL,
  Quantity INT NOT NULL CONSTRAINT DF_Parts_Qty DEFAULT 0 CONSTRAINT CK_Parts_Qty CHECK (Quantity >= 0),
  ImageUrl NVARCHAR(250) NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Parts_Brand_Category')
  CREATE INDEX IX_Parts_Brand_Category ON dbo.Parts(BrandId, Category) INCLUDE (Name, Price, Quantity);
GO
IF OBJECT_ID('dbo.PartReservations') IS NULL
CREATE TABLE dbo.PartReservations(
  ReservationId INT IDENTITY(1,1) PRIMARY KEY,
  UserId INT NOT NULL CONSTRAINT FK_Res_User REFERENCES dbo.Users(UserId),
  PartId INT NOT NULL CONSTRAINT FK_Res_Part REFERENCES dbo.Parts(PartId),
  VehicleId INT NULL CONSTRAINT FK_Res_Vehicle REFERENCES dbo.Vehicles(VehicleId) ON DELETE SET NULL,
  BookingId INT NULL CONSTRAINT FK_Res_Booking REFERENCES dbo.Bookings(BookingId),
  Quantity INT NOT NULL,
  Status NVARCHAR(20) NOT NULL DEFAULT 'Reserved',         -- Reserved | Collected | Cancelled
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
GO
IF OBJECT_ID('dbo.MaintenanceRecords') IS NULL
CREATE TABLE dbo.MaintenanceRecords(
  RecordId INT IDENTITY(1,1) PRIMARY KEY,
  VehicleId INT NOT NULL CONSTRAINT FK_Maint_Vehicle REFERENCES dbo.Vehicles(VehicleId),
  ServiceDate DATE NOT NULL,
  Mileage INT NOT NULL,
  Description NVARCHAR(500) NULL,
  PartsChanged NVARCHAR(500) NULL,
  Cost DECIMAL(10,2) NOT NULL DEFAULT 0,
  Technician NVARCHAR(100) NULL,
  NextServiceDate DATE NULL,
  NextServiceKm INT NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
GO
IF OBJECT_ID('dbo.ContactMessages') IS NULL
CREATE TABLE dbo.ContactMessages(
  MessageId INT IDENTITY(1,1) PRIMARY KEY,
  FullName NVARCHAR(100) NOT NULL,
  Email NVARCHAR(150) NOT NULL,
  Phone NVARCHAR(30) NULL,
  Body NVARCHAR(1500) NOT NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
GO

/* ---------- Seed: brands ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Brands)
INSERT INTO dbo.Brands(BrandId, Name, Slug, Prefix, PriceFactor, Description) VALUES
 (1, N'Audi',       'audi',       'AUD', 1.40, N'Progressive engineering needs precise care. Our Audi-trained technicians service every model with factory procedures.'),
 (2, N'Škoda',      'skoda',      'SKO', 1.00, N'Practical, durable and smart. Full service, inspections and repairs for the whole Škoda range.'),
 (3, N'SEAT',       'seat',       'SEA', 0.95, N'Sporty city cars and family favourites, maintained to the manufacturer schedule.'),
 (4, N'Volkswagen', 'volkswagen', 'VWG', 1.10, N'From the Polo to the Touareg: diagnostics, maintenance and repairs with genuine Volkswagen parts.'),
 (5, N'CUPRA',      'cupra',      'CUP', 1.35, N'Performance-focused service for CUPRA models, including software updates and sport brake systems.');

/* ---------- Seed: services ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Services)
INSERT INTO dbo.Services(Name, Description, DurationMinutes, PriceFrom) VALUES
 (N'Periodic Maintenance',     N'Manufacturer-scheduled inspection and service',            120, 1800),
 (N'Oil & Filter Change',      N'Genuine oil, oil filter and fluid level check',             45,  950),
 (N'Brake Service',            N'Pads, discs, fluid and brake system inspection',            90, 1500),
 (N'Computer Diagnostics',     N'Full electronic scan with the latest diagnostic tools',     45,  600),
 (N'Air Conditioning Service', N'A/C performance check, regas and cabin filter',             60, 1200),
 (N'Tires & Alignment',        N'Tire fitting, balancing and wheel alignment',               60,  700),
 (N'Battery & Electrical',     N'Battery test, replacement and electrical fault finding',    45,  500),
 (N'Suspension & Steering',    N'Shock absorbers, control arms and steering checks',        120, 1700);

/* ---------- Seed: 330 genuine parts (22 types x 5 brands x 3 vehicle classes) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Parts)
BEGIN
  ;WITH Types(Code, Name, Category, BasePrice) AS (
    SELECT * FROM (VALUES
     ('OIL', N'Engine Oil Filter',            N'Filters',    350),
     ('AIR', N'Air Filter',                   N'Filters',    420),
     ('CAB', N'Cabin Pollen Filter',          N'Filters',    380),
     ('FUE', N'Fuel Filter',                  N'Filters',    520),
     ('BPF', N'Front Brake Pads',             N'Brakes',    1450),
     ('BPR', N'Rear Brake Pads',              N'Brakes',    1250),
     ('BDF', N'Front Brake Discs (pair)',     N'Brakes',    3200),
     ('BDR', N'Rear Brake Discs (pair)',      N'Brakes',    2700),
     ('BFL', N'Brake Fluid 1L',               N'Brakes',     280),
     ('SPK', N'Spark Plugs (set of 4)',       N'Engine',     900),
     ('TBK', N'Timing Belt Kit',              N'Engine',    6800),
     ('DRB', N'Auxiliary Drive Belt',         N'Engine',     950),
     ('WPM', N'Water Pump',                   N'Engine',    3400),
     ('THS', N'Thermostat',                   N'Engine',    1100),
     ('SHF', N'Front Shock Absorber',         N'Suspension',3600),
     ('SHR', N'Rear Shock Absorber',          N'Suspension',3100),
     ('CTL', N'Control Arm',                  N'Suspension',2800),
     ('BAT', N'Battery 12V',                  N'Electrical',4200),
     ('WIP', N'Wiper Blades (pair)',          N'Electrical', 600),
     ('BLB', N'Headlight Bulb Set',           N'Electrical', 450),
     ('COO', N'Coolant G13 5L',               N'Fluids',     500),
     ('ATF', N'Transmission Fluid 1L',        N'Fluids',    1900)
    ) v(Code, Name, Category, BasePrice)),
  Classes(Code, Name, Factor) AS (
    SELECT * FROM (VALUES ('S', N'Compact', 1.0), ('M', N'Mid-size', 1.2), ('L', N'SUV / Large', 1.5)) c(Code, Name, Factor))
  INSERT INTO dbo.Parts(PartNumber, Name, BrandId, Category, Description, Price, Quantity)
  SELECT b.Prefix + '-' + t.Code + '-' + c.Code,
         b.Name + N' ' + t.Name + N' (' + c.Name + N')',
         b.BrandId, t.Category,
         N'Genuine ' + b.Name + N' ' + t.Name + N' for ' + c.Name + N' models. Factory specification with warranty.',
         CAST(ROUND(t.BasePrice * c.Factor * b.PriceFactor, 0) AS DECIMAL(10,2)),
         ABS(CHECKSUM(b.Prefix, t.Code, c.Code)) % 45
  FROM dbo.Brands b CROSS JOIN Types t CROSS JOIN Classes c;
END
GO
SELECT (SELECT COUNT(*) FROM dbo.Parts) AS PartsSeeded, (SELECT COUNT(*) FROM dbo.Services) AS Services, (SELECT COUNT(*) FROM dbo.Brands) AS Brands;
-- The first administrator is created automatically when the site starts (see Web.config appSettings AdminEmail / AdminPassword).

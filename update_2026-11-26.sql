/* Czech Service Center - update 26 Nov 2026
   1) Guest orders with prepayment (Visa via gateway / InstaPay)
   2) Vehicle fitment: filter parts by brand > model > year > engine cc
   3) Text-only service schedule by mileage (10,000 km, 20,000 km ...)
   Run once in SSMS on the CzechServiceCenter database. Safe to re-run. */
USE CzechServiceCenter;
GO

/* ---------- Part types (name + Arabic name) and TypeCode on Parts ---------- */
IF OBJECT_ID('dbo.PartTypes') IS NULL
CREATE TABLE dbo.PartTypes(Code NVARCHAR(10) NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, NameAr NVARCHAR(100) NULL);
GO
IF NOT EXISTS (SELECT 1 FROM dbo.PartTypes)
INSERT INTO dbo.PartTypes(Code, Name, NameAr) VALUES
 ('OIL', N'Engine Oil Filter', N'فلتر زيت المحرك'),
 ('AIR', N'Air Filter', N'فلتر هواء'),
 ('CAB', N'Cabin Pollen Filter', N'فلتر التكييف (الكابينة)'),
 ('FUE', N'Fuel Filter', N'فلتر بنزين'),
 ('BPF', N'Front Brake Pads', N'تيل فرامل أمامي'),
 ('BPR', N'Rear Brake Pads', N'تيل فرامل خلفي'),
 ('BDF', N'Front Brake Discs (pair)', N'دسكات فرامل أمامية (زوج)'),
 ('BDR', N'Rear Brake Discs (pair)', N'دسكات فرامل خلفية (زوج)'),
 ('BFL', N'Brake Fluid 1L', N'زيت فرامل 1 لتر'),
 ('SPK', N'Spark Plugs (set of 4)', N'بوجيهات (طقم 4)'),
 ('TBK', N'Timing Belt Kit', N'طقم سير الكاتينة'),
 ('DRB', N'Auxiliary Drive Belt', N'سير المساعدات (سير الدينامو)'),
 ('WPM', N'Water Pump', N'طلمبة مياه'),
 ('THS', N'Thermostat', N'ثرموستات'),
 ('SHF', N'Front Shock Absorber', N'مساعد أمامي'),
 ('SHR', N'Rear Shock Absorber', N'مساعد خلفي'),
 ('CTL', N'Control Arm', N'مقص (ذراع تحكم)'),
 ('BAT', N'Battery 12V', N'بطارية 12 فولت'),
 ('WIP', N'Wiper Blades (pair)', N'مساحات (زوج)'),
 ('BLB', N'Headlight Bulb Set', N'طقم لمبات الفانوس الأمامي'),
 ('COO', N'Coolant G13 5L', N'سائل تبريد (كولانت) 5 لتر'),
 ('ATF', N'Transmission Fluid 1L', N'زيت ناقل الحركة 1 لتر');
GO
IF COL_LENGTH('dbo.Parts','TypeCode') IS NULL ALTER TABLE dbo.Parts ADD TypeCode NVARCHAR(10) NULL;
GO
/* seeded part numbers look like AUD-OIL-S : prefix-type-class */
UPDATE dbo.Parts SET TypeCode = SUBSTRING(PartNumber,5,3)
 WHERE TypeCode IS NULL AND PartNumber LIKE '[A-Z][A-Z][A-Z]-[A-Z][A-Z][A-Z]-[SML]';
GO

/* ---------- Car models, engines, fitment ---------- */
IF OBJECT_ID('dbo.CarModels') IS NULL
CREATE TABLE dbo.CarModels(
  ModelId INT IDENTITY(1,1) PRIMARY KEY,
  BrandId INT NOT NULL CONSTRAINT FK_CarModels_Brand REFERENCES dbo.Brands(BrandId),
  Name NVARCHAR(60) NOT NULL,
  NameAr NVARCHAR(60) NULL,
  VehicleClass CHAR(1) NOT NULL,            -- S compact | M mid-size | L SUV/large (matches the seeded parts)
  YearFrom INT NOT NULL, YearTo INT NOT NULL,
  CONSTRAINT UQ_CarModels UNIQUE(BrandId, Name));
GO
IF OBJECT_ID('dbo.ModelEngines') IS NULL
CREATE TABLE dbo.ModelEngines(
  ModelId INT NOT NULL CONSTRAINT FK_ModelEngines_Model REFERENCES dbo.CarModels(ModelId) ON DELETE CASCADE,
  EngineCc INT NOT NULL,
  YearFrom INT NOT NULL, YearTo INT NOT NULL,
  CONSTRAINT PK_ModelEngines PRIMARY KEY(ModelId, EngineCc));
GO
IF OBJECT_ID('dbo.PartFitment') IS NULL
CREATE TABLE dbo.PartFitment(
  FitId INT IDENTITY(1,1) PRIMARY KEY,
  PartId INT NOT NULL CONSTRAINT FK_Fit_Part REFERENCES dbo.Parts(PartId) ON DELETE CASCADE,
  ModelId INT NOT NULL CONSTRAINT FK_Fit_Model REFERENCES dbo.CarModels(ModelId) ON DELETE CASCADE,
  YearFrom INT NOT NULL, YearTo INT NOT NULL,
  EngineCc INT NULL);                       -- NULL = fits every engine of that model
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Fit_Model')
  CREATE INDEX IX_Fit_Model ON dbo.PartFitment(ModelId, YearFrom, YearTo) INCLUDE (PartId, EngineCc);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Fit_Part')
  CREATE INDEX IX_Fit_Part ON dbo.PartFitment(PartId);
GO

/* SAMPLE DATA: models, years and engines below are placeholders so the filter works on day one.
   Replace them with your real catalogue data (Admin > Fitment) before selling to customers. */
IF NOT EXISTS (SELECT 1 FROM dbo.CarModels)
BEGIN
  INSERT INTO dbo.CarModels(BrandId, Name, NameAr, VehicleClass, YearFrom, YearTo) VALUES
   (1, N'A3', N'A3', 'S', 2015, 2026), (1, N'A4', N'A4', 'M', 2015, 2026), (1, N'A6', N'A6', 'M', 2015, 2026),
   (1, N'Q3', N'Q3', 'S', 2015, 2026), (1, N'Q5', N'Q5', 'M', 2015, 2026), (1, N'Q7', N'Q7', 'L', 2015, 2026),
   (2, N'Fabia', N'فابيا', 'S', 2015, 2026), (2, N'Octavia', N'أوكتافيا', 'M', 2015, 2026), (2, N'Superb', N'سوبرب', 'M', 2015, 2026),
   (2, N'Karoq', N'كاروك', 'M', 2015, 2026), (2, N'Kodiaq', N'كودياك', 'L', 2015, 2026),
   (3, N'Ibiza', N'إيبيزا', 'S', 2015, 2026), (3, N'Leon', N'ليون', 'S', 2015, 2026), (3, N'Arona', N'أرونا', 'S', 2015, 2026),
   (3, N'Ateca', N'أتيكا', 'M', 2015, 2026), (3, N'Tarraco', N'تاراكو', 'L', 2015, 2026),
   (4, N'Polo', N'بولو', 'S', 2015, 2026), (4, N'Golf', N'جولف', 'S', 2015, 2026), (4, N'Jetta', N'جيتا', 'M', 2015, 2026),
   (4, N'Passat', N'باسات', 'M', 2015, 2026), (4, N'Tiguan', N'تيجوان', 'M', 2015, 2026), (4, N'Touareg', N'طوارق', 'L', 2015, 2026),
   (5, N'Leon', N'ليون', 'S', 2015, 2026), (5, N'Formentor', N'فورمينتور', 'M', 2015, 2026), (5, N'Ateca', N'أتيكا', 'M', 2015, 2026);

  INSERT INTO dbo.ModelEngines(ModelId, EngineCc, YearFrom, YearTo)
  SELECT m.ModelId, e.Cc, m.YearFrom, m.YearTo
  FROM dbo.CarModels m
  JOIN (VALUES ('S',1000),('S',1400),('S',1600),('M',1400),('M',1600),('M',2000),('L',2000),('L',3000)) e(Cls, Cc) ON e.Cls = m.VehicleClass;
END
GO
/* every seeded part (class S/M/L) fits the models of its brand with the same class, all engines */
INSERT INTO dbo.PartFitment(PartId, ModelId, YearFrom, YearTo, EngineCc)
SELECT p.PartId, m.ModelId, m.YearFrom, m.YearTo, NULL
FROM dbo.Parts p
JOIN dbo.CarModels m ON m.BrandId = p.BrandId AND m.VehicleClass = RIGHT(p.PartNumber,1)
WHERE p.PartNumber LIKE '[A-Z][A-Z][A-Z]-[A-Z][A-Z][A-Z]-[SML]'
  AND NOT EXISTS (SELECT 1 FROM dbo.PartFitment f WHERE f.PartId = p.PartId);
GO

/* ---------- Orders (guest checkout, pay first) ---------- */
IF OBJECT_ID('dbo.Orders') IS NULL
CREATE TABLE dbo.Orders(
  OrderId INT IDENTITY(1001,1) PRIMARY KEY,
  PublicToken CHAR(32) NOT NULL CONSTRAINT UQ_Orders_Token UNIQUE,
  UserId INT NULL CONSTRAINT FK_Orders_User REFERENCES dbo.Users(UserId),
  CustomerName NVARCHAR(100) NOT NULL,
  Phone NVARCHAR(30) NOT NULL,
  Email NVARCHAR(150) NULL,
  DeliveryMethod NVARCHAR(10) NOT NULL,                 -- Pickup | Delivery
  BranchId INT NULL,
  City NVARCHAR(80) NULL,
  Address NVARCHAR(300) NULL,
  Subtotal DECIMAL(10,2) NOT NULL,
  DeliveryFee DECIMAL(10,2) NOT NULL DEFAULT 0,
  Total DECIMAL(10,2) NOT NULL,
  PaymentMethod NVARCHAR(10) NOT NULL,                  -- Card | InstaPay
  PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',-- Pending | AwaitingReview | Paid | Failed | Cancelled
  OrderStatus NVARCHAR(20) NOT NULL DEFAULT 'New',      -- New | Processing | Ready | Completed | Cancelled
  GatewayOrderId NVARCHAR(60) NULL,
  GatewayTxnId NVARCHAR(60) NULL,
  InstaPayRef NVARCHAR(80) NULL,
  Notes NVARCHAR(500) NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  PaidAt DATETIME2 NULL);
GO
IF OBJECT_ID('dbo.Branches') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Orders_Branch')
  ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_Branch FOREIGN KEY(BranchId) REFERENCES dbo.Branches(BranchId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_Status')
  CREATE INDEX IX_Orders_Status ON dbo.Orders(PaymentStatus, OrderStatus, CreatedAt);
GO
IF OBJECT_ID('dbo.OrderItems') IS NULL
CREATE TABLE dbo.OrderItems(
  ItemId INT IDENTITY(1,1) PRIMARY KEY,
  OrderId INT NOT NULL CONSTRAINT FK_OrderItems_Order REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
  PartId INT NOT NULL CONSTRAINT FK_OrderItems_Part REFERENCES dbo.Parts(PartId),
  PartNumber NVARCHAR(40) NOT NULL,
  Name NVARCHAR(150) NOT NULL,
  UnitPrice DECIMAL(10,2) NOT NULL,
  Quantity INT NOT NULL CONSTRAINT CK_OrderItems_Qty CHECK (Quantity > 0));
GO

/* ---------- Service schedule by mileage (text only) ---------- */
IF OBJECT_ID('dbo.ServicePlans') IS NULL
CREATE TABLE dbo.ServicePlans(
  PlanId INT IDENTITY(1,1) PRIMARY KEY,
  BrandId INT NOT NULL CONSTRAINT FK_Plans_Brand REFERENCES dbo.Brands(BrandId),
  IntervalKm INT NOT NULL,
  Tasks NVARCHAR(1200) NOT NULL,            -- one task per line
  TasksAr NVARCHAR(1200) NULL,
  PartCodes NVARCHAR(100) NULL,             -- comma separated PartTypes.Code
  CONSTRAINT UQ_Plans UNIQUE(BrandId, IntervalKm));
GO
/* SAMPLE schedule, same for the 5 brands. Intervals and tasks are a typical 10,000 km pattern:
   edit them to match each manufacturer's real service schedule. */
IF NOT EXISTS (SELECT 1 FROM dbo.ServicePlans)
INSERT INTO dbo.ServicePlans(BrandId, IntervalKm, Tasks, TasksAr, PartCodes)
SELECT b.BrandId, v.Km, v.Tasks, v.TasksAr, v.Codes
FROM dbo.Brands b CROSS JOIN (VALUES
 (10000,  N'Change engine oil and oil filter' + CHAR(10) + N'Check brake pads, tyres, lights and fluid levels' + CHAR(10) + N'Reset the service indicator',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'فحص تيل الفرامل والإطارات والإضاءة ومستويات السوائل' + CHAR(10) + N'تصفير عداد الصيانة', 'OIL'),
 (20000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace the cabin pollen filter' + CHAR(10) + N'Inspect brake pads and discs, tyres and suspension',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر التكييف (الكابينة)' + CHAR(10) + N'فحص تيل ودسكات الفرامل والإطارات والمساعدين', 'OIL,CAB'),
 (30000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace the air filter and cabin pollen filter' + CHAR(10) + N'Check battery, belts and coolant level',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر الهواء وفلتر التكييف' + CHAR(10) + N'فحص البطارية والسيور ومستوى سائل التبريد', 'OIL,AIR,CAB'),
 (40000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace the cabin filter and brake fluid' + CHAR(10) + N'Replace spark plugs (petrol engines)' + CHAR(10) + N'Inspect brake pads and discs',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر التكييف وزيت الفرامل' + CHAR(10) + N'تغيير البوجيهات (محركات البنزين)' + CHAR(10) + N'فحص تيل ودسكات الفرامل', 'OIL,CAB,BFL,SPK'),
 (50000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace the cabin pollen filter' + CHAR(10) + N'Check brakes, suspension and steering',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر التكييف (الكابينة)' + CHAR(10) + N'فحص الفرامل والمساعدين والدريكسيون', 'OIL,CAB'),
 (60000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace air, cabin and fuel filters' + CHAR(10) + N'Replace spark plugs and brake fluid' + CHAR(10) + N'Change transmission fluid (as specified for your gearbox)' + CHAR(10) + N'Replace brake pads if worn',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر الهواء والتكييف والبنزين' + CHAR(10) + N'تغيير البوجيهات وزيت الفرامل' + CHAR(10) + N'تغيير زيت ناقل الحركة (حسب نوع الجيربوكس)' + CHAR(10) + N'تغيير تيل الفرامل عند التآكل', 'OIL,AIR,CAB,FUE,SPK,BFL,ATF,BPF,BPR'),
 (70000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace the cabin pollen filter' + CHAR(10) + N'Check battery and electrical system',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر التكييف (الكابينة)' + CHAR(10) + N'فحص البطارية والنظام الكهربائي', 'OIL,CAB'),
 (80000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace air and cabin filters' + CHAR(10) + N'Replace spark plugs and brake fluid' + CHAR(10) + N'Inspect the timing belt and drive belt',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر الهواء والتكييف' + CHAR(10) + N'تغيير البوجيهات وزيت الفرامل' + CHAR(10) + N'فحص سير الكاتينة وسير المساعدات', 'OIL,AIR,CAB,SPK,BFL'),
 (90000,  N'Change engine oil and oil filter' + CHAR(10) + N'Replace the air filter and cabin pollen filter' + CHAR(10) + N'Check shock absorbers and control arms',
          N'تغيير زيت المحرك وفلتر الزيت' + CHAR(10) + N'تغيير فلتر الهواء وفلتر التكييف' + CHAR(10) + N'فحص المساعدين والمقصات', 'OIL,AIR,CAB'),
 (100000, N'Major service: oil and all filters' + CHAR(10) + N'Replace timing belt kit, water pump and drive belt' + CHAR(10) + N'Replace coolant, brake fluid and spark plugs' + CHAR(10) + N'Full inspection of brakes, suspension and battery',
          N'صيانة كبرى: الزيت وجميع الفلاتر' + CHAR(10) + N'تغيير طقم سير الكاتينة وطلمبة المياه وسير المساعدات' + CHAR(10) + N'تغيير سائل التبريد وزيت الفرامل والبوجيهات' + CHAR(10) + N'فحص شامل للفرامل والمساعدين والبطارية', 'OIL,AIR,CAB,FUE,SPK,BFL,COO,TBK,WPM,DRB')
) v(Km, Tasks, TasksAr, Codes);
GO
SELECT (SELECT COUNT(*) FROM dbo.CarModels) AS Models, (SELECT COUNT(*) FROM dbo.PartFitment) AS FitmentRows, (SELECT COUNT(*) FROM dbo.ServicePlans) AS ServicePlans;

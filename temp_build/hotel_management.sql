-- SQL Server schema for Hotel Management

IF DB_ID(N'HotelManagement') IS NULL
BEGIN
    CREATE DATABASE HotelManagement;
END
GO

USE HotelManagement;
GO

-- Drop order (safe for re-run)
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Invoices', 'U') IS NOT NULL DROP TABLE dbo.Invoices;
IF OBJECT_ID('dbo.ServiceUsages', 'U') IS NOT NULL DROP TABLE dbo.ServiceUsages;
IF OBJECT_ID('dbo.BookingRooms', 'U') IS NOT NULL DROP TABLE dbo.BookingRooms;
IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID('dbo.Services', 'U') IS NOT NULL DROP TABLE dbo.Services;
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL DROP TABLE dbo.Rooms;
IF OBJECT_ID('dbo.RoomTypes', 'U') IS NOT NULL DROP TABLE dbo.RoomTypes;
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL DROP TABLE dbo.Accounts;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

CREATE TABLE dbo.Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL
);

CREATE TABLE dbo.Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    Position NVARCHAR(50) NULL,
    HireDate DATE NOT NULL DEFAULT (GETDATE()),
    Status NVARCHAR(20) NOT NULL DEFAULT N'Active'
);

CREATE TABLE dbo.Accounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Salt NVARCHAR(100) NOT NULL,
    RoleId INT NOT NULL,
    EmployeeId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT (1),
    CreatedAt DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_Accounts_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId),
    CONSTRAINT FK_Accounts_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId)
);

CREATE TABLE dbo.Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    IdNumber NVARCHAR(30) NULL,
    Address NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT (SYSDATETIME())
);

CREATE TABLE dbo.RoomTypes (
    RoomTypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(50) NOT NULL UNIQUE,
    BasePrice DECIMAL(18,2) NOT NULL,
    Capacity INT NOT NULL,
    Description NVARCHAR(200) NULL
);

CREATE TABLE dbo.Rooms (
    RoomId INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber NVARCHAR(10) NOT NULL UNIQUE,
    RoomTypeId INT NOT NULL,
    Floor INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT N'Available',
    CONSTRAINT FK_Rooms_RoomTypes FOREIGN KEY (RoomTypeId) REFERENCES dbo.RoomTypes(RoomTypeId)
);

CREATE TABLE dbo.Services (
    ServiceId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL UNIQUE,
    Unit NVARCHAR(20) NOT NULL DEFAULT N'Unit',
    UnitPrice DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT (1)
);

CREATE TABLE dbo.Bookings (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    CheckInDate DATE NOT NULL,
    CheckOutDate DATE NOT NULL,
    Adults INT NOT NULL DEFAULT (1),
    Children INT NOT NULL DEFAULT (0),
    Status NVARCHAR(20) NOT NULL DEFAULT N'Pending',
    CreatedAt DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
    CreatedByAccountId INT NULL,
    CONSTRAINT FK_Bookings_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_Bookings_Accounts FOREIGN KEY (CreatedByAccountId) REFERENCES dbo.Accounts(AccountId),
    CONSTRAINT CK_Bookings_Dates CHECK (CheckOutDate > CheckInDate)
);

CREATE TABLE dbo.BookingRooms (
    BookingRoomId INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NOT NULL,
    RoomId INT NOT NULL,
    PricePerNight DECIMAL(18,2) NOT NULL,
    CheckInDate DATE NOT NULL,
    CheckOutDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT N'Reserved',
    CONSTRAINT FK_BookingRooms_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_BookingRooms_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT CK_BookingRooms_Dates CHECK (CheckOutDate > CheckInDate)
);

CREATE TABLE dbo.ServiceUsages (
    ServiceUsageId INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NOT NULL,
    ServiceId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    UsedAt DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
    AddedByAccountId INT NULL,
    CONSTRAINT FK_ServiceUsages_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_ServiceUsages_Services FOREIGN KEY (ServiceId) REFERENCES dbo.Services(ServiceId),
    CONSTRAINT FK_ServiceUsages_Accounts FOREIGN KEY (AddedByAccountId) REFERENCES dbo.Accounts(AccountId)
);

CREATE TABLE dbo.Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NOT NULL,
    InvoiceDate DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
    Subtotal DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(18,2) NOT NULL DEFAULT (0),
    Tax DECIMAL(18,2) NOT NULL DEFAULT (0),
    Total DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT N'Unpaid',
    PaidAt DATETIME2 NULL,
    PaymentMethod NVARCHAR(30) NULL,
    CreatedByAccountId INT NULL,
    CONSTRAINT FK_Invoices_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT FK_Invoices_Accounts FOREIGN KEY (CreatedByAccountId) REFERENCES dbo.Accounts(AccountId)
);

CREATE TABLE dbo.Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Method NVARCHAR(30) NOT NULL,
    PaidAt DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
    Note NVARCHAR(200) NULL,
    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId)
);

CREATE TABLE dbo.AuditLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    Entity NVARCHAR(50) NULL,
    EntityId NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_AuditLogs_Accounts FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(AccountId)
);

-- Indexes for performance
CREATE INDEX IX_Bookings_CustomerId ON dbo.Bookings(CustomerId);
CREATE INDEX IX_BookingRooms_RoomId ON dbo.BookingRooms(RoomId);
CREATE INDEX IX_ServiceUsages_BookingId ON dbo.ServiceUsages(BookingId);
CREATE INDEX IX_Invoices_BookingId ON dbo.Invoices(BookingId);

-- Views for reporting
IF OBJECT_ID('dbo.vw_DailyRevenue', 'V') IS NOT NULL DROP VIEW dbo.vw_DailyRevenue;
GO
CREATE VIEW dbo.vw_DailyRevenue AS
SELECT
    CAST(InvoiceDate AS DATE) AS RevenueDate,
    SUM(Total) AS TotalRevenue
FROM dbo.Invoices
WHERE Status = N'Paid'
GROUP BY CAST(InvoiceDate AS DATE);
GO

IF OBJECT_ID('dbo.vw_MonthlyRevenue', 'V') IS NOT NULL DROP VIEW dbo.vw_MonthlyRevenue;
GO
CREATE VIEW dbo.vw_MonthlyRevenue AS
SELECT
    DATEFROMPARTS(YEAR(InvoiceDate), MONTH(InvoiceDate), 1) AS RevenueMonth,
    SUM(Total) AS TotalRevenue
FROM dbo.Invoices
WHERE Status = N'Paid'
GROUP BY DATEFROMPARTS(YEAR(InvoiceDate), MONTH(InvoiceDate), 1);
GO

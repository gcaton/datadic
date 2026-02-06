-- Wait for SQL Server to be ready
WAITFOR DELAY '00:00:05';
GO

-- Create the sample database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SampleDB')
BEGIN
    CREATE DATABASE SampleDB;
END
GO

USE SampleDB;
GO

-- Create schemas
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Sales')
    EXEC('CREATE SCHEMA Sales');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'HR')
    EXEC('CREATE SCHEMA HR');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Inventory')
    EXEC('CREATE SCHEMA Inventory');
GO

-- =============================================
-- TABLES
-- =============================================

-- HR.Employees table
IF OBJECT_ID('HR.Employees', 'U') IS NULL
BEGIN
    CREATE TABLE HR.Employees (
        EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) UNIQUE NOT NULL,
        PhoneNumber NVARCHAR(20),
        HireDate DATE NOT NULL DEFAULT GETDATE(),
        Salary DECIMAL(10,2) NOT NULL,
        ManagerID INT NULL,
        DepartmentID INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        ModifiedDate DATETIME2 NULL
    );

    ALTER TABLE HR.Employees 
        ADD CONSTRAINT FK_Employees_Manager 
        FOREIGN KEY (ManagerID) REFERENCES HR.Employees(EmployeeID);

    EXEC sys.sp_addextendedproperty 
        @name=N'MS_Description', 
        @value=N'Employee information including contact details and employment data', 
        @level0type=N'SCHEMA', @level0name=N'HR', 
        @level1type=N'TABLE', @level1name=N'Employees';
END
GO

-- HR.Departments table
IF OBJECT_ID('HR.Departments', 'U') IS NULL
BEGIN
    CREATE TABLE HR.Departments (
        DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
        DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
        Budget DECIMAL(15,2) NOT NULL DEFAULT 0,
        ManagerEmployeeID INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Employees_Department')
BEGIN
    ALTER TABLE HR.Employees 
        ADD CONSTRAINT FK_Employees_Department 
        FOREIGN KEY (DepartmentID) REFERENCES HR.Departments(DepartmentID);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Departments_Manager')
BEGIN
    ALTER TABLE HR.Departments 
        ADD CONSTRAINT FK_Departments_Manager 
        FOREIGN KEY (ManagerEmployeeID) REFERENCES HR.Employees(EmployeeID);
END
GO

-- Sales.Customers table
IF OBJECT_ID('Sales.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE Sales.Customers (
        CustomerID INT IDENTITY(1,1) PRIMARY KEY,
        CompanyName NVARCHAR(100) NOT NULL,
        ContactName NVARCHAR(100) NOT NULL,
        ContactEmail NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20),
        Address NVARCHAR(200),
        City NVARCHAR(50),
        State NVARCHAR(50),
        ZipCode NVARCHAR(10),
        Country NVARCHAR(50) NOT NULL DEFAULT 'USA',
        CreditLimit DECIMAL(10,2) NOT NULL DEFAULT 5000.00,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT CK_Customers_CreditLimit CHECK (CreditLimit >= 0)
    );

    CREATE INDEX IX_Customers_CompanyName ON Sales.Customers(CompanyName);
    CREATE INDEX IX_Customers_City ON Sales.Customers(City, State);
END
GO

-- Sales.Orders table
IF OBJECT_ID('Sales.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Sales.Orders (
        OrderID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        EmployeeID INT NOT NULL,
        OrderDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        RequiredDate DATETIME2 NULL,
        ShippedDate DATETIME2 NULL,
        ShipAddress NVARCHAR(200),
        ShipCity NVARCHAR(50),
        ShipState NVARCHAR(50),
        ShipZipCode NVARCHAR(10),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Orders_Customer FOREIGN KEY (CustomerID) REFERENCES Sales.Customers(CustomerID),
        CONSTRAINT FK_Orders_Employee FOREIGN KEY (EmployeeID) REFERENCES HR.Employees(EmployeeID),
        CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled')),
        CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount >= 0)
    );

    CREATE INDEX IX_Orders_Customer ON Sales.Orders(CustomerID);
    CREATE INDEX IX_Orders_Employee ON Sales.Orders(EmployeeID);
    CREATE INDEX IX_Orders_OrderDate ON Sales.Orders(OrderDate DESC);
END
GO

-- Inventory.Categories table
IF OBJECT_ID('Inventory.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE Inventory.Categories (
        CategoryID INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        ParentCategoryID INT NULL,
        CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryID) REFERENCES Inventory.Categories(CategoryID)
    );
END
GO

-- Inventory.Products table
IF OBJECT_ID('Inventory.Products', 'U') IS NULL
BEGIN
    CREATE TABLE Inventory.Products (
        ProductID INT IDENTITY(1,1) PRIMARY KEY,
        ProductName NVARCHAR(100) NOT NULL,
        SKU NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        CategoryID INT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        QuantityInStock INT NOT NULL DEFAULT 0,
        ReorderLevel INT NOT NULL DEFAULT 10,
        Discontinued BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        ModifiedDate DATETIME2 NULL,
        CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryID) REFERENCES Inventory.Categories(CategoryID),
        CONSTRAINT CK_Products_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT CK_Products_QuantityInStock CHECK (QuantityInStock >= 0)
    );

    CREATE INDEX IX_Products_CategoryID ON Inventory.Products(CategoryID);
END
GO

-- Sales.OrderDetails table
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('Sales.OrderDetails', 'U') IS NULL
BEGIN
    CREATE TABLE Sales.OrderDetails (
        OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        ProductID INT NOT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        Quantity INT NOT NULL,
        Discount DECIMAL(4,2) NOT NULL DEFAULT 0,
        LineTotal AS (UnitPrice * Quantity * (1 - Discount)) PERSISTED,
        CONSTRAINT FK_OrderDetails_Order FOREIGN KEY (OrderID) REFERENCES Sales.Orders(OrderID) ON DELETE CASCADE,
        CONSTRAINT FK_OrderDetails_Product FOREIGN KEY (ProductID) REFERENCES Inventory.Products(ProductID),
        CONSTRAINT CK_OrderDetails_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderDetails_Discount CHECK (Discount >= 0 AND Discount <= 1)
    );

    CREATE INDEX IX_OrderDetails_OrderID ON Sales.OrderDetails(OrderID);
    CREATE INDEX IX_OrderDetails_ProductID ON Sales.OrderDetails(ProductID);
END
GO

-- Audit table
IF OBJECT_ID('dbo.AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog (
        AuditID BIGINT IDENTITY(1,1) PRIMARY KEY,
        TableName NVARCHAR(128) NOT NULL,
        OperationType NVARCHAR(10) NOT NULL,
        RecordID INT NULL,
        UserName NVARCHAR(128) NOT NULL DEFAULT SUSER_SNAME(),
        ChangeDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX)
    );

    CREATE INDEX IX_AuditLog_TableName ON dbo.AuditLog(TableName);
    CREATE INDEX IX_AuditLog_ChangeDate ON dbo.AuditLog(ChangeDate DESC);
END
GO

-- =============================================
-- VIEWS
-- =============================================

IF OBJECT_ID('HR.vw_ActiveEmployees', 'V') IS NOT NULL
    DROP VIEW HR.vw_ActiveEmployees;
GO

CREATE VIEW HR.vw_ActiveEmployees
AS
SELECT 
    e.EmployeeID,
    e.FirstName,
    e.LastName,
    e.Email,
    e.PhoneNumber,
    e.HireDate,
    e.Salary,
    d.DepartmentName,
    m.FirstName + ' ' + m.LastName AS ManagerName
FROM HR.Employees e
LEFT JOIN HR.Departments d ON e.DepartmentID = d.DepartmentID
LEFT JOIN HR.Employees m ON e.ManagerID = m.EmployeeID
WHERE e.IsActive = 1;
GO

IF OBJECT_ID('Sales.vw_OrderSummary', 'V') IS NOT NULL
    DROP VIEW Sales.vw_OrderSummary;
GO

CREATE VIEW Sales.vw_OrderSummary
AS
SELECT 
    o.OrderID,
    o.OrderDate,
    c.CompanyName,
    c.ContactName,
    e.FirstName + ' ' + e.LastName AS SalesRep,
    o.Status,
    COUNT(od.OrderDetailID) AS ItemCount,
    SUM(od.LineTotal) AS OrderTotal
FROM Sales.Orders o
INNER JOIN Sales.Customers c ON o.CustomerID = c.CustomerID
INNER JOIN HR.Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Sales.OrderDetails od ON o.OrderID = od.OrderID
GROUP BY o.OrderID, o.OrderDate, c.CompanyName, c.ContactName, 
         e.FirstName, e.LastName, o.Status;
GO

IF OBJECT_ID('Inventory.vw_ProductInventoryStatus', 'V') IS NOT NULL
    DROP VIEW Inventory.vw_ProductInventoryStatus;
GO

CREATE VIEW Inventory.vw_ProductInventoryStatus
AS
SELECT 
    p.ProductID,
    p.ProductName,
    p.SKU,
    c.CategoryName,
    p.UnitPrice,
    p.QuantityInStock,
    p.ReorderLevel,
    CASE 
        WHEN p.Discontinued = 1 THEN 'Discontinued'
        WHEN p.QuantityInStock = 0 THEN 'Out of Stock'
        WHEN p.QuantityInStock <= p.ReorderLevel THEN 'Low Stock'
        ELSE 'In Stock'
    END AS StockStatus
FROM Inventory.Products p
LEFT JOIN Inventory.Categories c ON p.CategoryID = c.CategoryID;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

IF OBJECT_ID('HR.usp_AddEmployee', 'P') IS NOT NULL
    DROP PROCEDURE HR.usp_AddEmployee;
GO

CREATE PROCEDURE HR.usp_AddEmployee
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(100),
    @PhoneNumber NVARCHAR(20),
    @Salary DECIMAL(10,2),
    @DepartmentID INT = NULL,
    @ManagerID INT = NULL,
    @EmployeeID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT INTO HR.Employees (FirstName, LastName, Email, PhoneNumber, Salary, DepartmentID, ManagerID)
        VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Salary, @DepartmentID, @ManagerID);
        SET @EmployeeID = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('Sales.usp_CreateOrder', 'P') IS NOT NULL
    DROP PROCEDURE Sales.usp_CreateOrder;
GO

CREATE PROCEDURE Sales.usp_CreateOrder
    @CustomerID INT,
    @EmployeeID INT,
    @RequiredDate DATETIME2 = NULL,
    @OrderID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM Sales.Customers WHERE CustomerID = @CustomerID)
            THROW 50001, 'Customer does not exist', 1;
        IF NOT EXISTS (SELECT 1 FROM HR.Employees WHERE EmployeeID = @EmployeeID AND IsActive = 1)
            THROW 50002, 'Employee does not exist or is not active', 1;
        INSERT INTO Sales.Orders (CustomerID, EmployeeID, RequiredDate)
        VALUES (@CustomerID, @EmployeeID, @RequiredDate);
        SET @OrderID = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('Sales.usp_GetSalesByEmployee', 'P') IS NOT NULL
    DROP PROCEDURE Sales.usp_GetSalesByEmployee;
GO

CREATE PROCEDURE Sales.usp_GetSalesByEmployee
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        e.EmployeeID,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        d.DepartmentName,
        COUNT(DISTINCT o.OrderID) AS OrderCount,
        SUM(od.LineTotal) AS TotalSales
    FROM HR.Employees e
    LEFT JOIN HR.Departments d ON e.DepartmentID = d.DepartmentID
    LEFT JOIN Sales.Orders o ON e.EmployeeID = o.EmployeeID 
        AND o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate
    LEFT JOIN Sales.OrderDetails od ON o.OrderID = od.OrderID
    WHERE e.IsActive = 1
    GROUP BY e.EmployeeID, e.FirstName, e.LastName, d.DepartmentName
    ORDER BY TotalSales DESC;
END
GO

-- =============================================
-- FUNCTIONS
-- =============================================

IF OBJECT_ID('HR.fn_GetEmployeeTenure', 'FN') IS NOT NULL
    DROP FUNCTION HR.fn_GetEmployeeTenure;
GO

CREATE FUNCTION HR.fn_GetEmployeeTenure(@EmployeeID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Tenure INT;
    SELECT @Tenure = DATEDIFF(YEAR, HireDate, GETDATE())
    FROM HR.Employees WHERE EmployeeID = @EmployeeID;
    RETURN ISNULL(@Tenure, 0);
END
GO

IF OBJECT_ID('HR.fn_GetEmployeeHierarchy', 'IF') IS NOT NULL
    DROP FUNCTION HR.fn_GetEmployeeHierarchy;
GO

CREATE FUNCTION HR.fn_GetEmployeeHierarchy(@ManagerID INT)
RETURNS TABLE
AS
RETURN
(
    WITH EmployeeHierarchy AS (
        SELECT EmployeeID, FirstName, LastName, ManagerID, 1 AS Level
        FROM HR.Employees WHERE ManagerID = @ManagerID
        UNION ALL
        SELECT e.EmployeeID, e.FirstName, e.LastName, e.ManagerID, eh.Level + 1
        FROM HR.Employees e
        INNER JOIN EmployeeHierarchy eh ON e.ManagerID = eh.EmployeeID
    )
    SELECT * FROM EmployeeHierarchy
);
GO

-- =============================================
-- TRIGGERS
-- =============================================

IF OBJECT_ID('HR.tr_Employees_Audit', 'TR') IS NOT NULL
    DROP TRIGGER HR.tr_Employees_Audit;
GO

CREATE TRIGGER HR.tr_Employees_Audit
ON HR.Employees
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OperationType NVARCHAR(10);
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @OperationType = 'UPDATE';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @OperationType = 'INSERT';
    ELSE
        SET @OperationType = 'DELETE';
    
    IF @OperationType = 'INSERT'
    BEGIN
        INSERT INTO dbo.AuditLog (TableName, OperationType, RecordID, NewValues)
        SELECT 'HR.Employees', @OperationType, EmployeeID,
            CONCAT('FirstName:', FirstName, ',LastName:', LastName, ',Email:', Email)
        FROM inserted;
    END
END
GO

IF OBJECT_ID('Sales.tr_OrderDetails_UpdateOrderTotal', 'TR') IS NOT NULL
    DROP TRIGGER Sales.tr_OrderDetails_UpdateOrderTotal;
GO

CREATE TRIGGER Sales.tr_OrderDetails_UpdateOrderTotal
ON Sales.OrderDetails
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE o SET TotalAmount = ISNULL((SELECT SUM(LineTotal) FROM Sales.OrderDetails WHERE OrderID = o.OrderID), 0)
    FROM Sales.Orders o
    WHERE o.OrderID IN (SELECT DISTINCT OrderID FROM inserted UNION SELECT DISTINCT OrderID FROM deleted);
END
GO

-- =============================================
-- SAMPLE DATA
-- =============================================

-- Insert Categories only if empty
IF NOT EXISTS (SELECT 1 FROM Inventory.Categories)
BEGIN
    SET IDENTITY_INSERT Inventory.Categories ON;
    INSERT INTO Inventory.Categories (CategoryID, CategoryName, Description, ParentCategoryID)
    VALUES 
        (1, 'Electronics', 'Electronic devices and accessories', NULL),
        (2, 'Computers', 'Computers and computer accessories', 1),
        (3, 'Mobile Devices', 'Smartphones and tablets', 1),
        (4, 'Furniture', 'Office and home furniture', NULL),
        (5, 'Office Supplies', 'General office supplies', NULL);
    SET IDENTITY_INSERT Inventory.Categories OFF;
END
GO

-- Insert Departments only if empty
IF NOT EXISTS (SELECT 1 FROM HR.Departments)
BEGIN
    SET IDENTITY_INSERT HR.Departments ON;
    INSERT INTO HR.Departments (DepartmentID, DepartmentName, Budget)
    VALUES 
        (1, 'Sales', 500000.00),
        (2, 'Marketing', 300000.00),
        (3, 'IT', 450000.00),
        (4, 'Human Resources', 250000.00),
        (5, 'Operations', 400000.00);
    SET IDENTITY_INSERT HR.Departments OFF;
END
GO

ALTER TABLE HR.Employees DISABLE TRIGGER tr_Employees_Audit;
GO

-- Insert Employees only if empty
IF NOT EXISTS (SELECT 1 FROM HR.Employees)
BEGIN
    SET IDENTITY_INSERT HR.Employees ON;
    INSERT INTO HR.Employees (EmployeeID, FirstName, LastName, Email, PhoneNumber, HireDate, Salary, ManagerID, DepartmentID, IsActive)
    VALUES 
        (1, 'John', 'Smith', 'john.smith@company.com', '555-0101', '2020-01-15', 95000.00, NULL, 1, 1),
        (2, 'Sarah', 'Johnson', 'sarah.johnson@company.com', '555-0102', '2020-03-20', 85000.00, 1, 1, 1),
        (3, 'Michael', 'Williams', 'michael.williams@company.com', '555-0103', '2021-05-10', 75000.00, 1, 1, 1),
        (4, 'Emily', 'Brown', 'emily.brown@company.com', '555-0104', '2019-08-01', 92000.00, NULL, 2, 1),
        (5, 'David', 'Davis', 'david.davis@company.com', '555-0105', '2021-11-15', 88000.00, NULL, 3, 1),
        (6, 'Jennifer', 'Miller', 'jennifer.miller@company.com', '555-0106', '2022-02-01', 65000.00, 5, 3, 1),
        (7, 'Robert', 'Wilson', 'robert.wilson@company.com', '555-0107', '2020-06-30', 70000.00, NULL, 4, 1),
        (8, 'Lisa', 'Moore', 'lisa.moore@company.com', '555-0108', '2021-09-12', 78000.00, NULL, 5, 1);
    SET IDENTITY_INSERT HR.Employees OFF;
END
GO

-- Update department managers only if not already set
UPDATE HR.Departments SET ManagerEmployeeID = 1 WHERE DepartmentID = 1 AND ManagerEmployeeID IS NULL;
UPDATE HR.Departments SET ManagerEmployeeID = 4 WHERE DepartmentID = 2 AND ManagerEmployeeID IS NULL;
UPDATE HR.Departments SET ManagerEmployeeID = 5 WHERE DepartmentID = 3 AND ManagerEmployeeID IS NULL;
UPDATE HR.Departments SET ManagerEmployeeID = 7 WHERE DepartmentID = 4 AND ManagerEmployeeID IS NULL;
UPDATE HR.Departments SET ManagerEmployeeID = 8 WHERE DepartmentID = 5 AND ManagerEmployeeID IS NULL;
GO

ALTER TABLE HR.Employees ENABLE TRIGGER tr_Employees_Audit;
GO

-- Insert Products only if empty
IF NOT EXISTS (SELECT 1 FROM Inventory.Products)
BEGIN
    SET IDENTITY_INSERT Inventory.Products ON;
    INSERT INTO Inventory.Products (ProductID, ProductName, SKU, Description, CategoryID, UnitPrice, QuantityInStock, ReorderLevel)
    VALUES 
        (1, 'Laptop Pro 15"', 'LAP-001', 'Professional grade laptop with 16GB RAM', 2, 1299.99, 50, 10),
        (2, 'Wireless Mouse', 'MSE-001', 'Ergonomic wireless mouse', 2, 29.99, 200, 50),
        (3, 'Mechanical Keyboard', 'KBD-001', 'RGB mechanical gaming keyboard', 2, 89.99, 75, 20),
        (4, 'Smartphone X', 'PHN-001', 'Latest flagship smartphone', 3, 899.99, 100, 25),
        (5, 'Tablet Plus', 'TAB-001', '10-inch tablet with stylus', 3, 549.99, 60, 15),
        (6, 'Office Desk', 'DSK-001', 'Adjustable standing desk', 4, 399.99, 30, 10),
        (7, 'Ergonomic Chair', 'CHR-001', 'Premium office chair with lumbar support', 4, 299.99, 45, 15),
        (8, 'Printer Laser', 'PRT-001', 'Color laser printer', 5, 249.99, 25, 8),
        (9, 'Paper Ream', 'PAP-001', 'A4 paper 500 sheets', 5, 9.99, 500, 100),
        (10, 'Monitor 27"', 'MON-001', '4K UHD monitor', 2, 449.99, 40, 12);
    SET IDENTITY_INSERT Inventory.Products OFF;
END
GO

-- Insert Customers only if empty
IF NOT EXISTS (SELECT 1 FROM Sales.Customers)
BEGIN
    SET IDENTITY_INSERT Sales.Customers ON;
    INSERT INTO Sales.Customers (CustomerID, CompanyName, ContactName, ContactEmail, Phone, Address, City, State, ZipCode, Country, CreditLimit)
    VALUES 
        (1, 'Tech Solutions Inc', 'Alice Cooper', 'alice@techsolutions.com', '555-1001', '123 Tech St', 'San Francisco', 'CA', '94102', 'USA', 50000.00),
        (2, 'Global Enterprises', 'Bob Martinez', 'bob@globalent.com', '555-1002', '456 Business Ave', 'New York', 'NY', '10001', 'USA', 75000.00),
        (3, 'Startup Hub', 'Carol White', 'carol@startuphub.com', '555-1003', '789 Innovation Blvd', 'Austin', 'TX', '78701', 'USA', 25000.00),
        (4, 'Enterprise Corp', 'Dan Black', 'dan@enterprisecorp.com', '555-1004', '321 Corporate Dr', 'Chicago', 'IL', '60601', 'USA', 100000.00),
        (5, 'Small Business LLC', 'Eve Green', 'eve@smallbiz.com', '555-1005', '654 Main St', 'Seattle', 'WA', '98101', 'USA', 15000.00);
    SET IDENTITY_INSERT Sales.Customers OFF;
END
GO

-- Insert Orders only if empty
IF NOT EXISTS (SELECT 1 FROM Sales.Orders)
BEGIN
    SET IDENTITY_INSERT Sales.Orders ON;
    INSERT INTO Sales.Orders (OrderID, CustomerID, EmployeeID, OrderDate, RequiredDate, Status)
    VALUES 
        (1, 1, 2, '2024-01-15', '2024-01-20', 'Delivered'),
        (2, 2, 2, '2024-01-20', '2024-01-25', 'Shipped'),
        (3, 3, 3, '2024-02-01', '2024-02-05', 'Processing'),
        (4, 1, 2, '2024-02-10', '2024-02-15', 'Pending'),
        (5, 4, 3, '2024-02-15', '2024-02-20', 'Pending');
    SET IDENTITY_INSERT Sales.Orders OFF;
END
GO

-- Insert OrderDetails only if empty
IF NOT EXISTS (SELECT 1 FROM Sales.OrderDetails)
BEGIN
    SET IDENTITY_INSERT Sales.OrderDetails ON;
    INSERT INTO Sales.OrderDetails (OrderDetailID, OrderID, ProductID, UnitPrice, Quantity, Discount)
    VALUES 
        (1, 1, 1, 1299.99, 5, 0.10),
        (2, 1, 2, 29.99, 10, 0.00),
        (3, 1, 3, 89.99, 5, 0.00),
        (4, 2, 4, 899.99, 3, 0.05),
        (5, 2, 5, 549.99, 2, 0.05),
        (6, 3, 6, 399.99, 10, 0.15),
        (7, 3, 7, 299.99, 10, 0.15),
        (8, 4, 1, 1299.99, 2, 0.00),
        (9, 4, 10, 449.99, 2, 0.00),
        (10, 5, 8, 249.99, 5, 0.10),
        (11, 5, 9, 9.99, 50, 0.00);
    SET IDENTITY_INSERT Sales.OrderDetails OFF;
END
GO

-- =============================================
-- CREATE DATABASE USERS AND ROLES
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'SalesRole' AND type = 'R')
    CREATE ROLE SalesRole;
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'ReadOnlyRole' AND type = 'R')
    CREATE ROLE ReadOnlyRole;
GO

GRANT SELECT, INSERT, UPDATE ON SCHEMA::Sales TO SalesRole;
GRANT SELECT ON SCHEMA::Inventory TO SalesRole;
GRANT SELECT ON SCHEMA::HR TO SalesRole;
GRANT SELECT ON SCHEMA::Sales TO ReadOnlyRole;
GRANT SELECT ON SCHEMA::Inventory TO ReadOnlyRole;
GRANT SELECT ON SCHEMA::HR TO ReadOnlyRole;
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'salesuser')
    CREATE LOGIN salesuser WITH PASSWORD = 'Sales123!';
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'salesuser')
BEGIN
    CREATE USER salesuser FOR LOGIN salesuser;
    ALTER ROLE SalesRole ADD MEMBER salesuser;
END
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'readonlyuser')
    CREATE LOGIN readonlyuser WITH PASSWORD = 'ReadOnly123!';
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'readonlyuser')
BEGIN
    CREATE USER readonlyuser FOR LOGIN readonlyuser;
    ALTER ROLE ReadOnlyRole ADD MEMBER readonlyuser;
    ALTER ROLE db_datareader ADD MEMBER readonlyuser;
END
GO

GRANT EXECUTE ON SCHEMA::Sales TO salesuser;
GO

PRINT '';
PRINT '====================================================';
PRINT 'Database SampleDB created successfully!';
PRINT '====================================================';
PRINT '';
PRINT 'Connection String:';
PRINT 'Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;';
PRINT '';
PRINT 'Test Users:';
PRINT '  salesuser / Sales123!';
PRINT '  readonlyuser / ReadOnly123!';
PRINT '';
PRINT 'Objects Created:';
PRINT '  - 8 Tables across 3 schemas (HR, Sales, Inventory)';
PRINT '  - 3 Views';
PRINT '  - 3 Stored Procedures';
PRINT '  - 2 Functions';
PRINT '  - 2 Triggers';
PRINT '  - Sample data in all tables';
PRINT '  - 2 Custom roles with permissions';
PRINT '====================================================';
GO

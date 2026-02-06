# Docker Test Environment

This directory contains a Docker Compose setup for running:
- **SQL Server 2022** with a pre-configured sample database for testing DataDic
- **Nginx web server** serving the generated documentation on http://localhost:8080

## Quick Start

Using [just](https://github.com/casey/just):

```bash
# Initialize and start SQL Server with sample database
just init

# Run datadic against the sample database
just run

# View the generated documentation
just open-docs
# Or browse to: http://localhost:8080
```

## Manual Setup

If you don't have `just` installed:

```bash
# Start SQL Server
cd .docker
docker compose up -d

# Wait for SQL Server to start (about 30 seconds)
sleep 30

# Initialize the database
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P DataDic123! -C -i /scripts/01-init-db.sql

# Run datadic
cd ..
dotnet run -- -c "Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;" -o ./output
```

## Sample Database

The `SampleDB` database includes:

### Schemas
- **HR**: Human Resources tables
- **Sales**: Sales and customer tables
- **Inventory**: Product and category tables

### Tables (8 total)
- `HR.Employees` - Employee information with self-referencing manager relationship
- `HR.Departments` - Department information
- `Sales.Customers` - Customer master data
- `Sales.Orders` - Sales orders
- `Sales.OrderDetails` - Order line items
- `Inventory.Products` - Product catalog
- `Inventory.Categories` - Product categories (hierarchical)
- `dbo.AuditLog` - Audit trail table

### Views (3 total)
- `HR.vw_ActiveEmployees` - Active employees with department info
- `Sales.vw_OrderSummary` - Order summary with customer and sales rep
- `Inventory.vw_ProductInventoryStatus` - Product inventory status

### Stored Procedures (3 total)
- `HR.usp_AddEmployee` - Add new employee
- `Sales.usp_CreateOrder` - Create new order
- `Sales.usp_GetSalesByEmployee` - Get sales statistics by employee

### Functions (2 total)
- `HR.fn_GetEmployeeTenure` - Calculate employee tenure (scalar)
- `HR.fn_GetEmployeeHierarchy` - Get employee hierarchy (table-valued)

### Triggers (2 total)
- `HR.tr_Employees_Audit` - Audit employee changes
- `Sales.tr_OrderDetails_UpdateOrderTotal` - Update order total when details change

### Users & Roles
- **SalesRole**: Can read/write to Sales schema, read HR and Inventory
- **ReadOnlyRole**: Can read all schemas
- **salesuser**: Member of SalesRole
- **readonlyuser**: Member of ReadOnlyRole

### Relationships
- Multiple foreign keys demonstrating relationships between tables
- Self-referencing relationships (Employees → Manager, Categories → Parent)
- Circular relationships (Employees ↔ Departments)
- Cascading deletes on OrderDetails

## Connection Information

**Server:** localhost,1433  
**Database:** SampleDB  
**SA Password:** DataDic123!

**Connection String:**
```
Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;
```

**Test Users:**
- Username: `salesuser`, Password: `Sales123!`
- Username: `readonlyuser`, Password: `ReadOnly123!`

**Web Server:**
- Documentation URL: http://localhost:8080
- Automatically serves content from `./output/` directory
- Nginx Alpine-based for minimal footprint

## Useful Commands

```bash
# View logs
just logs

# View web server logs
just web-logs

# View all logs
just logs-all

# Connect to SQL Server
just connect

# Stop containers
just down

# Remove everything (clean slate)
just clean

# Reset database
just reset

# Show connection info
just info
```

## Notes

- SQL Server Agent is enabled for testing job documentation
- The database initialization script is idempotent (can be run multiple times)
- Sample data includes realistic business scenarios
- All database objects have proper indexes, constraints, and extended properties
- Web server automatically serves the latest generated documentation
- Output directory (`../output/`) is volume-mapped to the web server as read-only

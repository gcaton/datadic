# Example Usage

This file contains example connection strings and usage scenarios for DataDic.

## SQL Server Examples

### Local SQL Server with Windows Authentication
```bash
dotnet run -- -c "Server=localhost;Database=AdventureWorks;Integrated Security=true;"
```

### SQL Server with SQL Authentication
```bash
dotnet run -- -c "Server=myserver;Database=MyDatabase;User Id=sa;Password=YourPassword123;"
```

### Azure SQL Database
```bash
dotnet run -- -c "Server=tcp:myserver.database.windows.net,1433;Database=MyDatabase;User Id=admin@myserver;Password=YourPassword123;Encrypt=True;"
```

### Custom Output Directory
```bash
dotnet run -- -c "Server=localhost;Database=Northwind;Integrated Security=true;" -o "./my-docs"
```

### SQL Server Express (LocalDB)
```bash
dotnet run -- -c "Server=(localdb)\mssqllocaldb;Database=MyLocalDB;Integrated Security=true;"
```

## Future PostgreSQL Examples (Coming Soon)

```bash
# PostgreSQL
dotnet run -- -c "Host=localhost;Database=mydb;Username=postgres;Password=postgres" -p postgres

# PostgreSQL on specific port
dotnet run -- -c "Host=localhost;Port=5433;Database=mydb;Username=postgres;Password=postgres" -p postgres
```

## Tips

1. **Connection String Security**: Never commit connection strings with passwords to source control. Consider using environment variables:

```bash
export DB_CONN="Server=localhost;Database=MyDB;User Id=user;Password=pass;"
dotnet run -- -c "$DB_CONN"
```

2. **Permissions Required**: 
   - Read access to system tables (sys.tables, sys.columns, etc.)
   - For jobs documentation: Access to msdb database
   - For user permissions: Access to security catalog views

3. **Large Databases**: For databases with many tables, generation may take a few minutes. Progress is shown in the console.

4. **Output Location**: The output directory will be created if it doesn't exist. Existing files will be overwritten.

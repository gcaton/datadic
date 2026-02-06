# Quick Start Guide

Get started with DataDic in 3 simple steps!

## Prerequisites

- .NET 10 SDK installed
- Docker installed and running
- `just` command runner (optional but recommended)

### Install just

```bash
# macOS
brew install just

# Linux (cargo)
cargo install just

# Or download from https://github.com/casey/just
```

## Three-Step Quick Start

### 1. Build the Application

```bash
dotnet build
```

### 2. Start Test Database

```bash
just init
```

This will:
- Start SQL Server 2022 in Docker
- Create a sample database called "SampleDB"
- Populate it with tables, views, stored procedures, functions, triggers, and data
- Create test users with permissions

Wait about 30-40 seconds for initialization to complete.

### 3. Generate Documentation

```bash
just run
```

This runs DataDic against the sample database and generates HTML documentation in `./output/`

### 4. View the Results

```bash
just open-docs
# Or open http://localhost:8080 in your browser
```

The documentation is served by an Nginx web server running in Docker, automatically displaying the latest generated content.

## What You'll See

The generated documentation includes:

- **Dashboard** - Statistics showing 8 tables, 3 views, 2 users
- **Tables Section** - All tables with:
  - Column definitions (names, types, nullability, defaults)
  - Primary keys and indexes
  - Foreign key relationships (clickable links)
  - Row counts
- **Users Section** - Database users with:
  - Roles memberships
  - Permissions
- **Jobs Section** - SQL Agent jobs (if any)

## Sample Database Schema

The test database includes realistic business scenarios:

- **HR Schema** - Employees and Departments
- **Sales Schema** - Customers, Orders, OrderDetails
- **Inventory Schema** - Products and Categories

With real-world patterns like:
- Self-referencing relationships (employee hierarchy)
- Circular foreign keys (departments ↔ employees)
- Computed columns (order line totals)
- Triggers (audit logging)
- Views (reporting)
- Stored procedures (business logic)

## Common Commands

```bash
# Full test (build + init + run)
just test

# View database logs
just logs

# View web server logs
just web-logs

# View all logs
just logs-all

# Connect to SQL Server
just connect

# Stop everything
just down

# Clean up (remove containers and data)
just clean

# Start fresh
just reset

# Show connection info
just info
```

## Manual Commands (without just)

If you don't have `just` installed:

```bash
# Start SQL Server
cd .docker && docker compose up -d && cd ..

# Wait for startup
sleep 30

# Initialize database
docker exec -it datadic-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P DataDic123! -C -i /scripts/01-init-db.sql

# Generate documentation
dotnet run -- -c "Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;" -o ./output

# Open http://localhost:8080 in your browser
```

## Testing With Your Own Database

Once you've verified the tool works with the sample database:

```bash
dotnet run -- -c "YOUR_CONNECTION_STRING_HERE" -o ./my-docs
```

Example with your own SQL Server:

```bash
dotnet run -- -c "Server=myserver;Database=MyDatabase;Integrated Security=true;" -o ./my-database-docs
```

## Troubleshooting

**SQL Server not starting?**
```bash
# Check Docker is running
docker ps

# View logs
just logs
# or
cd .docker && docker compose logs
```

**Database initialization failed?**
```bash
# Reset everything and try again
just reset
```

**Port 1433 already in use?**
Edit `.docker/docker-compose.yml` and change the port mapping:
```yaml
ports:
  - "14330:1433"  # Use 14330 instead
```

Then update connection strings accordingly.

## Next Steps

1. ✅ Explore the generated HTML documentation at http://localhost:8080
2. ✅ Check out the sample database structure
3. ✅ Try connecting to your own databases
4. ✅ See [EXAMPLES.md](EXAMPLES.md) for more connection string examples
5. ✅ Read [README.md](README.md) for full documentation

## Accessing the Documentation

The Docker setup includes an Nginx web server that automatically serves your generated documentation:

- **URL**: http://localhost:8080
- **Auto-refresh**: Just regenerate with `just run` and reload the browser
- **Browse files**: Directory listing enabled if you want to explore the structure

## Support

For issues or questions:
- Check the [README.md](README.md)
- Review [.docker/README.md](.docker/README.md) for Docker-specific info
- See [EXAMPLES.md](EXAMPLES.md) for usage examples

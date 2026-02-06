# DataDic Justfile - Task runner for common operations

# List all available commands
default:
    @just --list

# Initialize and start Docker SQL Server
init:
    @echo "Starting SQL Server container..."
    cd .docker && docker compose up -d
    @echo "Waiting for SQL Server to be ready (this may take 30-40 seconds)..."
    @sleep 35
    @echo "Initializing database..."
    cd .docker && docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P DataDic123! -C -i /scripts/01-init-db.sql
    @echo ""
    @echo "✅ SQL Server is ready!"
    @echo "✅ Web server is running!"
    @echo ""
    @echo "Connection String:"
    @echo "Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;"
    @echo ""
    @echo "Next steps:"
    @echo "  just run       - Generate documentation"
    @echo "  just open-docs - View at http://localhost:8080"

# Start Docker containers (without initialization)
up:
    cd .docker && docker compose up -d
    @echo "SQL Server started on port 1433"
    @echo "Web server started on http://localhost:8080"

# Stop Docker containers
down:
    cd .docker && docker compose down
    @echo "SQL Server stopped"

# Stop and remove containers and volumes (clean slate)
clean:
    cd .docker && docker compose down -v
    @echo "SQL Server containers and volumes removed"

# View logs from SQL Server container
logs:
    cd .docker && docker compose logs -f sqlserver

# View web server logs
web-logs:
    cd .docker && docker compose logs -f webserver

# View all logs
logs-all:
    cd .docker && docker compose logs -f

# Run datadic against the sample database
run:
    dotnet run -- -c "Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;" -o ./output
    @echo ""
    @echo "✅ Documentation generated!"
    @echo "View at: http://localhost:8080"

# Build the datadic project
build:
    dotnet build

# Restore NuGet packages
restore:
    dotnet restore

# Open the generated documentation in browser (Linux)
open-docs:
    @if [ -f ./output/index.html ]; then \
        xdg-open http://localhost:8080 2>/dev/null || echo "Open http://localhost:8080 in your browser"; \
    else \
        echo "No documentation found. Run 'just run' first."; \
    fi

# Connect to SQL Server with sqlcmd
connect:
    docker exec -it datadic-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P DataDic123! -C

# Run a quick test: build, init, and generate docs
test: build init
    @echo ""
    @echo "Running datadic against SampleDB..."
    @just run
    @echo ""
    @echo "✅ Test complete! Documentation generated in ./output/"
    @echo "Open ./output/index.html to view"

# Show database connection information
info:
    @echo "DataDic Environment Information:"
    @echo "================================"
    @echo ""
    @echo "SQL Server Connection:"
    @echo "  Server:   localhost,1433"
    @echo "  Database: SampleDB"
    @echo "  SA Password: DataDic123!"
    @echo ""
    @echo "Connection String:"
    @echo "  Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;"
    @echo ""
    @echo "Test Users:"
    @echo "  - salesuser / Sales123!"
    @echo "  - readonlyuser / ReadOnly123!"
    @echo ""
    @echo "Web Server:"
    @echo "  URL: http://localhost:8080"
    @echo "  Documentation will be available after running 'just run'"

# Reset database (clean + init)
reset: clean init
    @echo "Database reset complete"

# Check Docker container status
status:
    cd .docker && docker compose ps

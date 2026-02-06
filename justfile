# DataDic Justfile - Task runner for common operations

# List all available commands
default:
    @just --list

# Docker Management
[group('docker')]
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

[group('docker')]
up:
    cd .docker && docker compose up -d
    @echo "SQL Server started on port 1433"
    @echo "Web server started on http://localhost:8080"

[group('docker')]
down:
    cd .docker && docker compose down
    @echo "SQL Server stopped"

[group('docker')]
clean:
    cd .docker && docker compose down -v
    @echo "SQL Server containers and volumes removed"

[group('docker')]
logs:
    cd .docker && docker compose logs -f sqlserver

[group('docker')]
web-logs:
    cd .docker && docker compose logs -f webserver

[group('docker')]
logs-all:
    cd .docker && docker compose logs -f

[group('docker')]
status:
    cd .docker && docker compose ps

# Build and Development
[group('build')]
build:
    cd src && dotnet build

[group('build')]
restore:
    cd src && dotnet restore

[group('build')]
run:
    cd src && dotnet run -- -c "Server=localhost,1433;Database=SampleDB;User Id=sa;Password=DataDic123!;TrustServerCertificate=True;" -o ../output
    @echo ""
    @echo "✅ Documentation generated!"
    @echo "View at: http://localhost:8080"

# Database Management
[group('database')]
connect:
    docker exec -it datadic-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P DataDic123! -C

# Utilities
[group('utils')]
open-docs:
    @if [ -f ./output/index.html ]; then \
        xdg-open http://localhost:8080 2>/dev/null || echo "Open http://localhost:8080 in your browser"; \
    else \
        echo "No documentation found. Run 'just run' first."; \
    fi

[group('utils')]
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

# Workflows
[group('workflows')]
test: build init
    @echo ""
    @echo "Running datadic against SampleDB..."
    @just run
    @echo ""
    @echo "✅ Test complete! Documentation generated in ./output/"
    @echo "Open ./output/index.html to view"

[group('workflows')]
reset: clean init
    @echo "Database reset complete"

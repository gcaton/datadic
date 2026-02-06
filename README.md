# DataDic - Database Data Dictionary Generator

A .NET 10 console application that generates comprehensive HTML-based data dictionaries for database systems.

## Features

- **Comprehensive Database Documentation**: Automatically extracts and documents database schema, tables, columns, relationships, indexes, and more
- **ER Diagrams**: SVG-based entity-relationship diagrams with interactive zoom and download capabilities
- **Programmability Documentation**: Stored procedures, functions, triggers with full definitions
- **User & Security Documentation**: Documents database users, roles, and permissions
- **Job Documentation**: Captures SQL Server Agent jobs and their schedules
- **Interactive HTML Output**: Multi-page HTML with navigation and drill-down capabilities
- **Extensible Architecture**: Provider-based design supporting multiple database platforms
- **Beautiful UI**: Clean, modern CSS styling with responsive design

## Supported Databases

- **SQL Server** (primary implementation)
- PostgreSQL, MySQL, Oracle (coming soon via provider pattern)

## Installation

### Prerequisites

- .NET 10 SDK
- Docker (optional, for testing with sample database)

### Build from Source

```bash
git clone <repository-url>
cd datadic
dotnet restore
dotnet build
```

### Quick Test with Docker

The easiest way to test the tool is using the included Docker setup with a sample database and web server:

```bash
# Install just (task runner) - https://github.com/casey/just
# Or use manual commands from .docker/README.md

# Initialize SQL Server and sample database
just init

# Generate documentation
just run

# View in browser
just open-docs
# Or navigate to: http://localhost:8080
```

The Docker setup includes:
- **SQL Server 2022** with comprehensive sample database
- **Nginx web server** serving the generated documentation on port 8080
- **Volume mapping** so documentation is automatically available after generation

See [.docker/README.md](.docker/README.md) for detailed Docker setup information.

## Usage

### Basic Usage

```bash
dotnet run -- --connection-string "Server=myserver;Database=mydb;User Id=myuser;Password=mypass;"
```

### Command-Line Options

```
Options:
  -c, --connection-string <connection-string> (REQUIRED)  Database connection string
  -o, --output <output>                                   Output directory for HTML files [default: ./output]
  -p, --provider <provider>                               Database provider (sqlserver, postgres) [default: sqlserver]
  --version                                               Show version information
  -?, -h, --help                                          Show help and usage information
```

### Examples

#### SQL Server with Windows Authentication

```bash
dotnet run -- -c "Server=localhost;Database=AdventureWorks;Integrated Security=true;" -o ./docs
```

#### SQL Server with SQL Authentication

```bash
dotnet run -- -c "Server=myserver.database.windows.net;Database=MyDatabase;User Id=admin;Password=P@ssw0rd;" -o ./output
```

#### Specify Output Directory

```bash
dotnet run -- -c "Server=localhost;Database=MyDB;Integrated Security=true;" -o "C:\DBDocs"
```

## Output Structure

The generated documentation includes:

```
output/
├── index.html          # Main entry point with overview and statistics
├── css/
│   └── style.css       # Styling for all pages
├── tables/
│   ├── dbo_Users.html
│   ├── dbo_Orders.html
│   └── ...             # One file per table/view
├── users/
│   ├── AppUser.html
│   └── ...             # One file per database user
└── jobs/
    ├── DailyBackup.html
    └── ...             # One file per SQL Agent job
```

## Documentation Includes

### Tables & Views
- Column definitions (name, type, nullable, defaults, descriptions)
- Primary keys and identity columns
- Indexes (primary, unique, non-clustered)
- Foreign key relationships with clickable links
- Row counts

### Database Users
- User type (SQL, Windows, etc.)
- Database roles membership
- Object-level permissions (GRANT/DENY)

### SQL Server Agent Jobs
- Job descriptions and enabled status
- Schedules (frequency, start times)
- Job steps with commands

### Web Interface
When using Docker, the documentation is served via Nginx on http://localhost:8080 with:
- Clean, modern interface
- Responsive design
- Fast navigation
- Directory browsing enabled

## Architecture

The project uses a clean, extensible architecture:

- **Models**: Data structures representing database metadata
- **Providers**: Database-specific implementations (IDatabaseProvider interface)
  - `SqlServerProvider`: SQL Server implementation
  - Future: PostgresProvider, MySqlProvider, etc.
- **Generators**: HTML generation logic
  - `HtmlGenerator`: Creates multi-page HTML documentation

## Future Enhancements

- [ ] SVG/Canvas-based ER diagrams (similar to SSMS)
- [ ] PostgreSQL provider
- [ ] MySQL provider
- [ ] Oracle provider
- [ ] Export to other formats (Markdown, PDF)
- [ ] Search functionality in HTML output
- [ ] Dark mode theme
- [ ] Stored procedures and functions documentation
- [ ] Triggers documentation
- [ ] Schema comparison between databases

## Contributing

Contributions are welcome! To add support for a new database provider:

1. Implement the `IDatabaseProvider` interface
2. Add your provider to the switch statement in `Program.cs`
3. Test thoroughly with sample databases

## License

[Add your license here]

## Support

For issues, questions, or contributions, please [open an issue/contact information].

# DataDic - Database Data Dictionary Generator

## Project Overview
A .NET 10 console application that generates comprehensive HTML-based data dictionaries for database systems.

## Core Features
- **Database Connection**: Connect via connection string to generate complete database documentation
- **HTML Output**: Multi-page HTML output with sensible folder structure (index.html as entry point)
- **Table Relationships**: Interactive drill-down navigation through table relationships
- **Security Documentation**: Document database users and their access levels
- **Job Documentation**: Document SQL Server Agent jobs and their schedules
- **Extensible Architecture**: Plugin-based design to support multiple database platforms

## Target Databases
- **Primary**: SQL Server (initial implementation)
- **Future**: PostgreSQL, MySQL, Oracle, etc. (extensible design)

## Output Structure
- Multi-page HTML format
- index.html as main entry point
- Organized folder structure for tables, users, jobs, etc.
- Interactive navigation between related entities

## Future Enhancements
- **Database Diagrams**: SVG or HTML Canvas-based ER diagrams (similar to SSMS)
- Additional database platform support as needed

## Technology Stack
- .NET 10
- C# Console Application
- HTML/CSS/JavaScript for output
- ADO.NET / Dapper for database access
- Provider pattern for extensibility

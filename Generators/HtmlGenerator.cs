using DataDic.Models;
using System.Text;
using System.Web;

namespace DataDic.Generators;

public class HtmlGenerator
{
    private readonly string _outputPath;

    public HtmlGenerator(string outputPath)
    {
        _outputPath = outputPath;
    }

    public async Task GenerateAsync(DatabaseMetadata metadata)
    {
        Directory.CreateDirectory(_outputPath);
        Directory.CreateDirectory(Path.Combine(_outputPath, "tables"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "users"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "jobs"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "procedures"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "functions"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "css"));

        await GenerateCssAsync();
        await GenerateIndexAsync(metadata);
        await GenerateStatisticsAsync(metadata);
        await GenerateDiagramAsync(metadata);
        await GenerateTablesAsync(metadata);
        await GenerateUsersAsync(metadata);
        await GenerateJobsAsync(metadata);
        await GenerateProceduresAsync(metadata);
        await GenerateFunctionsAsync(metadata);
    }

    private async Task GenerateCssAsync()
    {
        var css = @"* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    line-height: 1.6;
    color: #333;
    background: #f5f5f5;
}

.container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 20px;
}

header {
    background: #2c3e50;
    color: white;
    padding: 20px 0;
    margin-bottom: 30px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

header h1 {
    margin: 0;
    font-size: 28px;
}

header p {
    margin: 5px 0 0;
    opacity: 0.9;
    font-size: 14px;
}

nav {
    background: white;
    padding: 15px;
    margin-bottom: 30px;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

nav a {
    color: #3498db;
    text-decoration: none;
    margin-right: 20px;
    font-weight: 500;
}

nav a:hover {
    text-decoration: underline;
}

.card {
    background: white;
    border-radius: 8px;
    padding: 20px;
    margin-bottom: 20px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.card h2 {
    margin-bottom: 15px;
    color: #2c3e50;
    border-bottom: 2px solid #3498db;
    padding-bottom: 10px;
}

.card h3 {
    margin: 20px 0 10px;
    color: #34495e;
    font-size: 18px;
}

table {
    width: 100%;
    border-collapse: collapse;
    margin: 15px 0;
}

table th {
    background: #ecf0f1;
    padding: 12px;
    text-align: left;
    font-weight: 600;
    border-bottom: 2px solid #bdc3c7;
}

table td {
    padding: 10px 12px;
    border-bottom: 1px solid #ecf0f1;
}

table tr:hover {
    background: #f8f9fa;
}

.badge {
    display: inline-block;
    padding: 3px 8px;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 500;
    margin-right: 5px;
}

.badge-primary {
    background: #3498db;
    color: white;
}

.badge-success {
    background: #27ae60;
    color: white;
}

.badge-info {
    background: #95a5a6;
    color: white;
}

.badge-danger {
    background: #e74c3c;
    color: white;
}

a.table-link {
    color: #3498db;
    text-decoration: none;
    font-weight: 500;
}

a.table-link:hover {
    text-decoration: underline;
}

.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 20px;
    margin-bottom: 30px;
}

.stat-card {
    background: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    text-align: center;
}

.stat-card .number {
    font-size: 32px;
    font-weight: bold;
    color: #3498db;
}

.stat-card .label {
    color: #7f8c8d;
    font-size: 14px;
    margin-top: 5px;
}

code {
    background: #f4f4f4;
    padding: 2px 6px;
    border-radius: 3px;
    font-family: monospace;
    font-size: 13px;
}

pre {
    background: #f4f4f4;
    padding: 15px;
    border-radius: 5px;
    overflow-x: auto;
    margin: 10px 0;
}

.enabled {
    color: #27ae60;
    font-weight: bold;
}

.disabled {
    color: #e74c3c;
    font-weight: bold;
}";

        await File.WriteAllTextAsync(Path.Combine(_outputPath, "css", "style.css"), css);
    }

    private async Task GenerateIndexAsync(DatabaseMetadata metadata)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>Data Dictionary - {Encode(metadata.DatabaseName)}</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine($"            <h1>Database Data Dictionary</h1>");
        html.AppendLine($"            <p>{Encode(metadata.ServerName)} / {Encode(metadata.DatabaseName)}</p>");
        html.AppendLine($"            <p style=\"font-size: 0.9em; opacity: 0.8;\">Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");

        html.AppendLine("    <div class=\"container\">");
        html.AppendLine("        <nav>");
        html.AppendLine("            <a href=\"index.html\">Home</a>");
        html.AppendLine("            <a href=\"statistics.html\">Statistics</a>");
        html.AppendLine("            <a href=\"diagram.html\">ER Diagram</a>");
        html.AppendLine("            <a href=\"#tables\">Tables</a>");
        html.AppendLine("            <a href=\"#procedures\">Procedures</a>");
        html.AppendLine("            <a href=\"#functions\">Functions</a>");
        html.AppendLine("            <a href=\"#users\">Users</a>");
        html.AppendLine("            <a href=\"#jobs\">Jobs</a>");
        html.AppendLine("        </nav>");

        html.AppendLine("        <div class=\"stats-grid\">");
        html.AppendLine("            <div class=\"stat-card\">");
        html.AppendLine($"                <div class=\"number\">{metadata.Tables.Count(t => t.Type == "USER_TABLE")}</div>");
        html.AppendLine("                <div class=\"label\">Tables</div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div class=\"stat-card\">");
        html.AppendLine($"                <div class=\"number\">{metadata.Tables.Count(t => t.Type == "VIEW")}</div>");
        html.AppendLine("                <div class=\"label\">Views</div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div class=\"stat-card\">");
        html.AppendLine($"                <div class=\"number\">{metadata.StoredProcedures.Count}</div>");
        html.AppendLine("                <div class=\"label\">Procedures</div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div class=\"stat-card\">");
        html.AppendLine($"                <div class=\"number\">{metadata.Functions.Count}</div>");
        html.AppendLine("                <div class=\"label\">Functions</div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div class=\"stat-card\">");
        html.AppendLine($"                <div class=\"number\">{metadata.Users.Count}</div>");
        html.AppendLine("                <div class=\"label\">Users</div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div class=\"stat-card\">");
        html.AppendLine($"                <div class=\"number\">{metadata.Jobs.Count}</div>");
        html.AppendLine("                <div class=\"label\">Jobs</div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class=\"card\" id=\"tables\">");
        html.AppendLine("            <h2>Tables and Views</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr>");
        html.AppendLine("                        <th>Schema</th>");
        html.AppendLine("                        <th>Name</th>");
        html.AppendLine("                        <th>Type</th>");
        html.AppendLine("                        <th>Columns</th>");
        html.AppendLine("                        <th>Row Count</th>");
        html.AppendLine("                    </tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var table in metadata.Tables.OrderBy(t => t.Schema).ThenBy(t => t.Name))
        {
            var fileName = $"{table.Schema}_{table.Name}.html";
            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td>{Encode(table.Schema)}</td>");
            html.AppendLine($"                        <td><a class=\"table-link\" href=\"tables/{fileName}\">{Encode(table.Name)}</a></td>");
            html.AppendLine($"                        <td><span class=\"badge badge-info\">{Encode(table.Type)}</span></td>");
            html.AppendLine($"                        <td>{table.Columns.Count}</td>");
            html.AppendLine($"                        <td>{table.RowCount:N0}</td>");
            html.AppendLine("                    </tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        // Stored Procedures Section
        html.AppendLine("        <div class=\"card\" id=\"procedures\">");
        html.AppendLine("            <h2>Stored Procedures</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr>");
        html.AppendLine("                        <th>Schema</th>");
        html.AppendLine("                        <th>Procedure Name</th>");
        html.AppendLine("                        <th>Parameters</th>");
        html.AppendLine("                        <th>Modified</th>");
        html.AppendLine("                    </tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var proc in metadata.StoredProcedures.OrderBy(p => p.Schema).ThenBy(p => p.Name))
        {
            var fileName = $"{proc.Schema}_{proc.Name}.html";
            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td>{Encode(proc.Schema)}</td>");
            html.AppendLine($"                        <td><a class=\"table-link\" href=\"procedures/{fileName}\">{Encode(proc.Name)}</a></td>");
            html.AppendLine($"                        <td>{proc.Parameters.Count}</td>");
            html.AppendLine($"                        <td>{proc.ModifiedDate:yyyy-MM-dd}</td>");
            html.AppendLine("                    </tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        // Functions Section
        html.AppendLine("        <div class=\"card\" id=\"functions\">");
        html.AppendLine("            <h2>Functions</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr>");
        html.AppendLine("                        <th>Schema</th>");
        html.AppendLine("                        <th>Function Name</th>");
        html.AppendLine("                        <th>Type</th>");
        html.AppendLine("                        <th>Parameters</th>");
        html.AppendLine("                        <th>Modified</th>");
        html.AppendLine("                    </tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var func in metadata.Functions.OrderBy(f => f.Schema).ThenBy(f => f.Name))
        {
            var fileName = $"{func.Schema}_{func.Name}.html";
            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td>{Encode(func.Schema)}</td>");
            html.AppendLine($"                        <td><a class=\"table-link\" href=\"functions/{fileName}\">{Encode(func.Name)}</a></td>");
            html.AppendLine($"                        <td><span class=\"badge badge-warning\">{Encode(func.Type)}</span></td>");
            html.AppendLine($"                        <td>{func.Parameters.Count}</td>");
            html.AppendLine($"                        <td>{func.ModifiedDate:yyyy-MM-dd}</td>");
            html.AppendLine("                    </tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class=\"card\" id=\"users\">");
        html.AppendLine("            <h2>Database Users</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr>");
        html.AppendLine("                        <th>User Name</th>");
        html.AppendLine("                        <th>Type</th>");
        html.AppendLine("                        <th>Roles</th>");
        html.AppendLine("                    </tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var user in metadata.Users.OrderBy(u => u.Name))
        {
            var fileName = $"{SanitizeFileName(user.Name)}.html";
            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td><a class=\"table-link\" href=\"users/{fileName}\">{Encode(user.Name)}</a></td>");
            html.AppendLine($"                        <td>{Encode(user.Type)}</td>");
            html.AppendLine($"                        <td>{string.Join(", ", user.Roles)}</td>");
            html.AppendLine("                    </tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class=\"card\" id=\"jobs\">");
        html.AppendLine("            <h2>SQL Server Agent Jobs</h2>");
        
        if (metadata.Jobs.Any())
        {
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Job Name</th>");
            html.AppendLine("                        <th>Status</th>");
            html.AppendLine("                        <th>Schedules</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var job in metadata.Jobs.OrderBy(j => j.Name))
            {
                var fileName = $"{SanitizeFileName(job.Name)}.html";
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td><a class=\"table-link\" href=\"jobs/{fileName}\">{Encode(job.Name)}</a></td>");
                html.AppendLine($"                        <td><span class=\"{(job.IsEnabled ? "enabled" : "disabled")}\">{(job.IsEnabled ? "Enabled" : "Disabled")}</span></td>");
                html.AppendLine($"                        <td>{job.Schedules.Count}</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
        }
        else
        {
            html.AppendLine("            <p>No SQL Server Agent jobs found or insufficient permissions to access msdb.</p>");
        }

        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        await File.WriteAllTextAsync(Path.Combine(_outputPath, "index.html"), html.ToString());
    }

    private async Task GenerateDiagramAsync(DatabaseMetadata metadata)
    {
        var diagramGenerator = new DiagramGenerator();
        var svgContent = diagramGenerator.GenerateSvgDiagram(metadata);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>ER Diagram - Data Dictionary</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"css/style.css\">");
        html.AppendLine("    <style>");
        html.AppendLine("        .diagram-container {");
        html.AppendLine("            background: white;");
        html.AppendLine("            border-radius: 8px;");
        html.AppendLine("            padding: 20px;");
        html.AppendLine("            margin: 20px 0;");
        html.AppendLine("            box-shadow: 0 2px 4px rgba(0,0,0,0.1);");
        html.AppendLine("            overflow-x: auto;");
        html.AppendLine("        }");
        html.AppendLine("        .diagram-controls {");
        html.AppendLine("            margin-bottom: 15px;");
        html.AppendLine("            display: flex;");
        html.AppendLine("            gap: 10px;");
        html.AppendLine("            align-items: center;");
        html.AppendLine("        }");
        html.AppendLine("        .diagram-controls button {");
        html.AppendLine("            padding: 8px 16px;");
        html.AppendLine("            background: #3498db;");
        html.AppendLine("            color: white;");
        html.AppendLine("            border: none;");
        html.AppendLine("            border-radius: 4px;");
        html.AppendLine("            cursor: pointer;");
        html.AppendLine("            font-size: 14px;");
        html.AppendLine("        }");
        html.AppendLine("        .diagram-controls button:hover {");
        html.AppendLine("            background: #2980b9;");
        html.AppendLine("        }");
        html.AppendLine("        .zoom-info {");
        html.AppendLine("            color: #7f8c8d;");
        html.AppendLine("            font-size: 14px;");
        html.AppendLine("        }");
        html.AppendLine("        #erDiagram {");
        html.AppendLine("            width: 100%;");
        html.AppendLine("            min-height: 600px;");
        html.AppendLine("            transition: transform 0.2s;");
        html.AppendLine("        }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine("            <h1>Entity-Relationship Diagram</h1>");
        html.AppendLine($"            <p>{Encode(metadata.ServerName)} / {Encode(metadata.DatabaseName)}</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");

        html.AppendLine("    <div class=\"container\">");
        html.AppendLine("        <nav>");
        html.AppendLine("            <a href=\"index.html\">← Back to Home</a>");
        html.AppendLine("        </nav>");

        html.AppendLine("        <div class=\"card\">");
        html.AppendLine("            <h2>Database Schema Diagram</h2>");
        html.AppendLine("            <p>Interactive entity-relationship diagram showing tables and their relationships. Click on any table to view details.</p>");
        html.AppendLine("            ");
        html.AppendLine("            <div class=\"diagram-controls\">");
        html.AppendLine("                <button onclick=\"zoomIn()\">🔍 Zoom In</button>");
        html.AppendLine("                <button onclick=\"zoomOut()\">🔍 Zoom Out</button>");
        html.AppendLine("                <button onclick=\"resetZoom()\">↺ Reset</button>");
        html.AppendLine("                <button onclick=\"downloadSVG()\">💾 Download SVG</button>");
        html.AppendLine("                <span class=\"zoom-info\" id=\"zoomLevel\">Zoom: 100%</span>");
        html.AppendLine("            </div>");
        html.AppendLine("            ");
        html.AppendLine("            <div class=\"diagram-container\">");
        html.AppendLine("                <div id=\"erDiagram\">");
        html.AppendLine(svgContent);
        html.AppendLine("                </div>");
        html.AppendLine("            </div>");
        html.AppendLine("            ");
        html.AppendLine("            <div style=\"margin-top: 20px; padding: 15px; background: #ecf0f1; border-radius: 4px;\">");
        html.AppendLine("                <h3 style=\"margin-bottom: 10px;\">Legend</h3>");
        html.AppendLine("                <p><strong>🔑</strong> Primary Key | <strong>🔗</strong> Foreign Key</p>");
        html.AppendLine("                <p><strong>Arrows:</strong> Relationships point from foreign key to referenced primary key</p>");
        html.AppendLine("            </div>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <script>");
        html.AppendLine("        let currentZoom = 1;");
        html.AppendLine("        const diagram = document.getElementById('erDiagram');");
        html.AppendLine("        const zoomLevelEl = document.getElementById('zoomLevel');");
        html.AppendLine("        ");
        html.AppendLine("        function zoomIn() {");
        html.AppendLine("            currentZoom = Math.min(currentZoom + 0.1, 3);");
        html.AppendLine("            updateZoom();");
        html.AppendLine("        }");
        html.AppendLine("        ");
        html.AppendLine("        function zoomOut() {");
        html.AppendLine("            currentZoom = Math.max(currentZoom - 0.1, 0.3);");
        html.AppendLine("            updateZoom();");
        html.AppendLine("        }");
        html.AppendLine("        ");
        html.AppendLine("        function resetZoom() {");
        html.AppendLine("            currentZoom = 1;");
        html.AppendLine("            updateZoom();");
        html.AppendLine("        }");
        html.AppendLine("        ");
        html.AppendLine("        function updateZoom() {");
        html.AppendLine("            diagram.style.transform = `scale(${currentZoom})`;");
        html.AppendLine("            diagram.style.transformOrigin = 'top left';");
        html.AppendLine("            zoomLevelEl.textContent = `Zoom: ${Math.round(currentZoom * 100)}%`;");
        html.AppendLine("        }");
        html.AppendLine("        ");
        html.AppendLine("        function downloadSVG() {");
        html.AppendLine("            const svg = diagram.querySelector('svg');");
        html.AppendLine("            const serializer = new XMLSerializer();");
        html.AppendLine("            const svgString = serializer.serializeToString(svg);");
        html.AppendLine("            const blob = new Blob([svgString], { type: 'image/svg+xml' });");
        html.AppendLine("            const url = URL.createObjectURL(blob);");
        html.AppendLine("            const a = document.createElement('a');");
        html.AppendLine($"            a.href = url;");
        html.AppendLine($"            a.download = '{SanitizeFileName(metadata.DatabaseName)}_er_diagram.svg';");
        html.AppendLine("            a.click();");
        html.AppendLine("            URL.revokeObjectURL(url);");
        html.AppendLine("        }");
        html.AppendLine("    </script>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        await File.WriteAllTextAsync(Path.Combine(_outputPath, "diagram.html"), html.ToString());
    }

    private async Task GenerateTablesAsync(DatabaseMetadata metadata)
    {
        foreach (var table in metadata.Tables)
        {
            await GenerateTablePageAsync(table, metadata);
        }
    }

    private async Task GenerateTablePageAsync(TableInfo table, DatabaseMetadata metadata)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>{Encode(table.Schema)}.{Encode(table.Name)} - Data Dictionary</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"../css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine($"            <h1>{Encode(table.Schema)}.{Encode(table.Name)}</h1>");
        html.AppendLine($"            <p>{Encode(table.Type)} - {table.RowCount:N0} rows");
        if (table.Description != null)
            html.AppendLine($" - {Encode(table.Description)}");
        html.AppendLine("</p>");
        html.AppendLine($"            <p style=\"font-size: 12px; opacity: 0.8;\">Created: {table.CreatedDate:yyyy-MM-dd} | Modified: {table.ModifiedDate:yyyy-MM-dd}</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");

        html.AppendLine("    <div class=\"container\">");
        html.AppendLine("        <nav>");
        html.AppendLine("            <a href=\"../index.html\">Home</a>");
        html.AppendLine("            <a href=\"#columns\">Columns</a>");
        html.AppendLine("            <a href=\"#indexes\">Indexes</a>");
        html.AppendLine("            <a href=\"#relationships\">Relationships</a>");
        if (table.CheckConstraints.Any())
            html.AppendLine("            <a href=\"#constraints\">Check Constraints</a>");
        if (table.Dependencies.Any())
            html.AppendLine("            <a href=\"#dependencies\">Dependencies</a>");
        if (table.Triggers.Any())
            html.AppendLine("            <a href=\"#triggers\">Triggers</a>");
        html.AppendLine("        </nav>");

        html.AppendLine("        <div class=\"card\" id=\"columns\">");
        html.AppendLine("            <h2>Columns</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr>");
        html.AppendLine("                        <th>Column Name</th>");
        html.AppendLine("                        <th>Data Type</th>");
        html.AppendLine("                        <th>Nullable</th>");
        html.AppendLine("                        <th>Default</th>");
        html.AppendLine("                        <th>Attributes</th>");
        html.AppendLine("                        <th>Description</th>");
        html.AppendLine("                    </tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var column in table.Columns)
        {
            var dataType = column.DataType;
            if (column.MaxLength.HasValue && column.MaxLength > 0)
                dataType += $"({(column.MaxLength == -1 ? "max" : column.MaxLength.ToString())})";
            else if (column.Precision.HasValue && column.Precision > 0)
                dataType += $"({column.Precision},{column.Scale})";

            html.AppendLine("                    <tr>");
            html.AppendLine($"                        <td><strong>{Encode(column.Name)}</strong></td>");
            html.AppendLine($"                        <td><code>{Encode(dataType)}</code>");
            if (column.Collation != null && column.Collation != "SQL_Latin1_General_CP1_CI_AS")
                html.AppendLine($"<br/><small style=\"color: #666;\">Collation: {Encode(column.Collation)}</small>");
            html.AppendLine("</td>");
            html.AppendLine($"                        <td>{(column.IsNullable ? "NULL" : "NOT NULL")}</td>");
            html.AppendLine($"                        <td>{Encode(column.DefaultValue ?? "")}</td>");
            html.AppendLine("                        <td>");
            if (column.IsPrimaryKey)
                html.AppendLine("                            <span class=\"badge badge-primary\">PK</span>");
            if (column.IsIdentity)
                html.AppendLine("                            <span class=\"badge badge-success\">Identity</span>");
            if (column.IsComputed)
            {
                html.AppendLine("                            <span class=\"badge badge-info\">Computed</span>");
                if (column.ComputedDefinition != null)
                    html.AppendLine($"<br/><small style=\"color: #666;\">{Encode(column.ComputedDefinition)}</small>");
            }
            html.AppendLine("                        </td>");
            html.AppendLine($"                        <td>{Encode(column.Description ?? "")}</td>");
            html.AppendLine("                    </tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        if (table.Indexes.Any())
        {
            html.AppendLine("        <div class=\"card\" id=\"indexes\">");
            html.AppendLine("            <h2>Indexes</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Index Name</th>");
            html.AppendLine("                        <th>Type</th>");
            html.AppendLine("                        <th>Columns</th>");
            html.AppendLine("                        <th>Details</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var index in table.Indexes)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(index.Name)}</td>");
                html.AppendLine("                        <td>");
                if (index.IsPrimaryKey)
                    html.AppendLine("                            <span class=\"badge badge-primary\">Primary Key</span>");
                else if (index.IsUnique)
                    html.AppendLine("                            <span class=\"badge badge-success\">Unique</span>");
                else
                    html.AppendLine("                            <span class=\"badge badge-info\">Index</span>");
                if (index.IsClustered)
                    html.AppendLine("                            <span class=\"badge badge-secondary\">Clustered</span>");
                html.AppendLine("                        </td>");
                
                var columns = string.Join(", ", index.Columns.Select(c => c.Name + (c.IsDescending ? " DESC" : "")));
                html.AppendLine($"                        <td>{columns}</td>");
                
                html.AppendLine("                        <td>");
                if (index.IncludedColumns.Any())
                    html.AppendLine($"<strong>Included:</strong> {string.Join(", ", index.IncludedColumns)}<br/>");
                if (index.FilterDefinition != null)
                    html.AppendLine($"<strong>Filter:</strong> {Encode(index.FilterDefinition)}");
                html.AppendLine("                        </td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        if (table.ForeignKeys.Any())
        {
            html.AppendLine("        <div class=\"card\" id=\"relationships\">");
            html.AppendLine("            <h2>Foreign Key Relationships</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Constraint Name</th>");
            html.AppendLine("                        <th>Referenced Table</th>");
            html.AppendLine("                        <th>Column Mapping</th>");
            html.AppendLine("                        <th>Actions</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var fk in table.ForeignKeys)
            {
                var refTableFile = $"{fk.ReferencedSchema}_{fk.ReferencedTable}.html";
                var mappings = string.Join(", ", fk.ColumnMappings.Select(m => $"{m.Column} → {m.ReferencedColumn}"));
                
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(fk.Name)}");
                if (fk.IsDisabled)
                    html.AppendLine("<br/><span class=\"badge badge-danger\">Disabled</span>");
                html.AppendLine("</td>");
                html.AppendLine($"                        <td><a class=\"table-link\" href=\"{refTableFile}\">{Encode(fk.ReferencedSchema)}.{Encode(fk.ReferencedTable)}</a></td>");
                html.AppendLine($"                        <td>{Encode(mappings)}</td>");
                html.AppendLine("                        <td>");
                html.AppendLine($"<strong>On Delete:</strong> {Encode(fk.OnDeleteAction)}<br/>");
                html.AppendLine($"<strong>On Update:</strong> {Encode(fk.OnUpdateAction)}");
                html.AppendLine("                        </td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        // Check Constraints Section
        if (table.CheckConstraints.Any())
        {
            html.AppendLine("        <div class=\"card\" id=\"constraints\">");
            html.AppendLine("            <h2>Check Constraints</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Constraint Name</th>");
            html.AppendLine("                        <th>Definition</th>");
            html.AppendLine("                        <th>Status</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var constraint in table.CheckConstraints)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(constraint.Name)}</td>");
                html.AppendLine($"                        <td><code>{Encode(constraint.Definition)}</code></td>");
                html.AppendLine("                        <td>");
                if (constraint.IsDisabled)
                    html.AppendLine("                            <span class=\"badge badge-danger\">Disabled</span>");
                else
                    html.AppendLine("                            <span class=\"badge badge-success\">Enabled</span>");
                html.AppendLine("                        </td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        // Dependencies Section
        if (table.Dependencies.Any())
        {
            html.AppendLine("        <div class=\"card\" id=\"dependencies\">");
            html.AppendLine("            <h2>Dependencies</h2>");
            
            var usedDependencies = table.Dependencies.Where(d => d.DependencyType == "USES").ToList();
            var usedByDependencies = table.Dependencies.Where(d => d.DependencyType == "USED_BY").ToList();
            
            if (usedDependencies.Any())
            {
                html.AppendLine("            <h3>Objects Used By This " + table.Type + "</h3>");
                html.AppendLine("            <table>");
                html.AppendLine("                <thead>");
                html.AppendLine("                    <tr>");
                html.AppendLine("                        <th>Object</th>");
                html.AppendLine("                        <th>Type</th>");
                html.AppendLine("                    </tr>");
                html.AppendLine("                </thead>");
                html.AppendLine("                <tbody>");

                foreach (var dep in usedDependencies)
                {
                    html.AppendLine("                    <tr>");
                    html.AppendLine($"                        <td>{Encode(dep.ReferencedSchema)}.{Encode(dep.ReferencedObject)}</td>");
                    html.AppendLine($"                        <td><span class=\"badge badge-info\">{Encode(dep.ReferencedType)}</span></td>");
                    html.AppendLine("                    </tr>");
                }

                html.AppendLine("                </tbody>");
                html.AppendLine("            </table>");
            }
            
            if (usedByDependencies.Any())
            {
                html.AppendLine("            <h3>Objects That Use This " + table.Type + "</h3>");
                html.AppendLine("            <table>");
                html.AppendLine("                <thead>");
                html.AppendLine("                    <tr>");
                html.AppendLine("                        <th>Object</th>");
                html.AppendLine("                        <th>Type</th>");
                html.AppendLine("                    </tr>");
                html.AppendLine("                </thead>");
                html.AppendLine("                <tbody>");

                foreach (var dep in usedByDependencies)
                {
                    html.AppendLine("                    <tr>");
                    html.AppendLine($"                        <td>{Encode(dep.ReferencedSchema)}.{Encode(dep.ReferencedObject)}</td>");
                    html.AppendLine($"                        <td><span class=\"badge badge-info\">{Encode(dep.ReferencedType)}</span></td>");
                    html.AppendLine("                    </tr>");
                }

                html.AppendLine("                </tbody>");
                html.AppendLine("            </table>");
            }
            
            html.AppendLine("        </div>");
        }

        // Triggers Section
        if (table.Triggers.Any())
        {
            html.AppendLine("        <div class=\"card\" id=\"triggers\">");
            html.AppendLine("            <h2>Triggers</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Trigger Name</th>");
            html.AppendLine("                        <th>Type</th>");
            html.AppendLine("                        <th>Events</th>");
            html.AppendLine("                        <th>Status</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var trigger in table.Triggers)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(trigger.Name)}</td>");
                html.AppendLine($"                        <td><span class=\"badge badge-info\">{Encode(trigger.Type)}</span></td>");
                html.AppendLine($"                        <td>{Encode(trigger.Events)}</td>");
                html.AppendLine("                        <td>");
                if (trigger.IsEnabled)
                    html.AppendLine("                            <span class=\"badge badge-success\">Enabled</span>");
                else
                    html.AppendLine("                            <span class=\"badge badge-danger\">Disabled</span>");
                html.AppendLine("                        </td>");
                html.AppendLine("                    </tr>");
                
                // Add definition in expandable row
                if (!string.IsNullOrWhiteSpace(trigger.Definition))
                {
                    html.AppendLine("                    <tr>");
                    html.AppendLine("                        <td colspan=\"4\">");
                    html.AppendLine("                            <details>");
                    html.AppendLine("                                <summary>Definition</summary>");
                    html.AppendLine($"                                <pre style=\"background: #f5f5f5; padding: 10px; border-radius: 4px; overflow-x: auto;\">{Encode(trigger.Definition)}</pre>");
                    html.AppendLine("                            </details>");
                    html.AppendLine("                        </td>");
                    html.AppendLine("                    </tr>");
                }
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        // View Definition Section
        if (table.Type == "VIEW" && !string.IsNullOrWhiteSpace(table.Definition))
        {
            html.AppendLine("        <div class=\"card\" id=\"view-definition\">");
            html.AppendLine("            <h2>View Definition</h2>");
            html.AppendLine($"            <pre style=\"background: #f5f5f5; padding: 15px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap;\">{Encode(table.Definition)}</pre>");
            html.AppendLine("        </div>");
        }

        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var fileName = $"{table.Schema}_{table.Name}.html";
        await File.WriteAllTextAsync(Path.Combine(_outputPath, "tables", fileName), html.ToString());
    }

    private async Task GenerateUsersAsync(DatabaseMetadata metadata)
    {
        foreach (var user in metadata.Users)
        {
            await GenerateUserPageAsync(user);
        }
    }

    private async Task GenerateUserPageAsync(UserInfo user)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>{Encode(user.Name)} - User Details</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"../css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine($"            <h1>User: {Encode(user.Name)}</h1>");
        html.AppendLine($"            <p>{Encode(user.Type)}</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");

        html.AppendLine("    <div class=\"container\">");
        html.AppendLine("        <nav>");
        html.AppendLine("            <a href=\"../index.html\">Home</a>");
        html.AppendLine("            <a href=\"#roles\">Roles</a>");
        html.AppendLine("            <a href=\"#permissions\">Permissions</a>");
        html.AppendLine("        </nav>");

        html.AppendLine("        <div class=\"card\" id=\"roles\">");
        html.AppendLine("            <h2>Database Roles</h2>");
        if (user.Roles.Any())
        {
            html.AppendLine("            <ul>");
            foreach (var role in user.Roles)
            {
                html.AppendLine($"                <li><span class=\"badge badge-primary\">{Encode(role)}</span></li>");
            }
            html.AppendLine("            </ul>");
        }
        else
        {
            html.AppendLine("            <p>No roles assigned.</p>");
        }
        html.AppendLine("        </div>");

        html.AppendLine("        <div class=\"card\" id=\"permissions\">");
        html.AppendLine("            <h2>Permissions</h2>");
        if (user.Permissions.Any())
        {
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Object</th>");
            html.AppendLine("                        <th>Permission</th>");
            html.AppendLine("                        <th>Grant Type</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var perm in user.Permissions)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(perm.ObjectName)}</td>");
                html.AppendLine($"                        <td>{Encode(perm.Permission)}</td>");
                html.AppendLine($"                        <td><span class=\"badge badge-{(perm.GrantType == "GRANT" ? "success" : "danger")}\">{Encode(perm.GrantType)}</span></td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
        }
        else
        {
            html.AppendLine("            <p>No explicit permissions assigned.</p>");
        }
        html.AppendLine("        </div>");

        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var fileName = $"{SanitizeFileName(user.Name)}.html";
        await File.WriteAllTextAsync(Path.Combine(_outputPath, "users", fileName), html.ToString());
    }

    private async Task GenerateJobsAsync(DatabaseMetadata metadata)
    {
        foreach (var job in metadata.Jobs)
        {
            await GenerateJobPageAsync(job);
        }
    }

    private async Task GenerateJobPageAsync(JobInfo job)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>{Encode(job.Name)} - Job Details</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"../css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine($"            <h1>Job: {Encode(job.Name)}</h1>");
        html.AppendLine($"            <p><span class=\"{(job.IsEnabled ? "enabled" : "disabled")}\">{(job.IsEnabled ? "Enabled" : "Disabled")}</span></p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");

        html.AppendLine("    <div class=\"container\">");
        html.AppendLine("        <nav>");
        html.AppendLine("            <a href=\"../index.html\">Home</a>");
        html.AppendLine("            <a href=\"#description\">Description</a>");
        html.AppendLine("            <a href=\"#schedules\">Schedules</a>");
        html.AppendLine("            <a href=\"#steps\">Steps</a>");
        html.AppendLine("        </nav>");

        html.AppendLine("        <div class=\"card\" id=\"description\">");
        html.AppendLine("            <h2>Description</h2>");
        html.AppendLine($"            <p>{Encode(job.Description)}</p>");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class=\"card\" id=\"schedules\">");
        html.AppendLine("            <h2>Schedules</h2>");
        if (job.Schedules.Any())
        {
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Schedule Name</th>");
            html.AppendLine("                        <th>Status</th>");
            html.AppendLine("                        <th>Frequency</th>");
            html.AppendLine("                        <th>Start Time</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var schedule in job.Schedules)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(schedule.Name)}</td>");
                html.AppendLine($"                        <td><span class=\"{(schedule.IsEnabled ? "enabled" : "disabled")}\">{(schedule.IsEnabled ? "Enabled" : "Disabled")}</span></td>");
                html.AppendLine($"                        <td>{Encode(schedule.FrequencyType)}</td>");
                html.AppendLine($"                        <td>{Encode(schedule.ActiveStartTime)}</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
        }
        else
        {
            html.AppendLine("            <p>No schedules configured.</p>");
        }
        html.AppendLine("        </div>");

        html.AppendLine("        <div class=\"card\" id=\"steps\">");
        html.AppendLine("            <h2>Job Steps</h2>");
        if (job.Steps.Any())
        {
            foreach (var step in job.Steps)
            {
                html.AppendLine("            <div class=\"card\">");
                html.AppendLine($"                <h3>Step {step.StepId}: {Encode(step.Name)}</h3>");
                html.AppendLine($"                <p><strong>Subsystem:</strong> {Encode(step.Subsystem)}</p>");
                html.AppendLine("                <p><strong>Command:</strong></p>");
                html.AppendLine($"                <pre>{Encode(step.Command)}</pre>");
                html.AppendLine("            </div>");
            }
        }
        else
        {
            html.AppendLine("            <p>No job steps configured.</p>");
        }
        html.AppendLine("        </div>");

        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var fileName = $"{SanitizeFileName(job.Name)}.html";
        await File.WriteAllTextAsync(Path.Combine(_outputPath, "jobs", fileName), html.ToString());
    }

    private async Task GenerateProceduresAsync(DatabaseMetadata metadata)
    {
        foreach (var proc in metadata.StoredProcedures)
        {
            await GenerateProcedurePageAsync(proc);
        }
    }

    private async Task GenerateProcedurePageAsync(StoredProcedureInfo proc)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>{Encode(proc.Schema)}.{Encode(proc.Name)} - Stored Procedure</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"../css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine("            <h1>Stored Procedure: {Encode(proc.Schema)}.{Encode(proc.Name)}</h1>");
        html.AppendLine("            <p><a href=\"../index.html\">← Back to Index</a></p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");
        html.AppendLine("    <div class=\"container\">");
        
        html.AppendLine("        <div class=\"card\">");
        html.AppendLine("            <h2>Information</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <tr><th>Schema</th><td>{Encode(proc.Schema)}</td></tr>");
        html.AppendLine("                <tr><th>Name</th><td>{Encode(proc.Name)}</td></tr>");
        html.AppendLine($"                <tr><th>Created</th><td>{proc.CreatedDate:yyyy-MM-dd HH:mm:ss}</td></tr>");
        html.AppendLine($"                <tr><th>Modified</th><td>{proc.ModifiedDate:yyyy-MM-dd HH:mm:ss}</td></tr>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        if (proc.Parameters.Any())
        {
            html.AppendLine("        <div class=\"card\">");
            html.AppendLine("            <h2>Parameters</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Parameter</th>");
            html.AppendLine("                        <th>Data Type</th>");
            html.AppendLine("                        <th>Direction</th>");
            html.AppendLine("                        <th>Default</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var param in proc.Parameters)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(param.Name)}</td>");
                html.AppendLine($"                        <td>{Encode(param.DataType)}{(param.MaxLength.HasValue && param.MaxLength > 0 ? $"({param.MaxLength})" : "")}</td>");
                html.AppendLine($"                        <td>{(param.IsOutput ? "OUTPUT" : "INPUT")}</td>");
                html.AppendLine($"                        <td>{(param.DefaultValue ?? "")}</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        html.AppendLine("        <div class=\"card\">");
        html.AppendLine("            <h2>Definition</h2>");
        html.AppendLine($"            <pre style=\"background: #f5f5f5; padding: 15px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap;\">{Encode(proc.Definition)}</pre>");
        html.AppendLine("        </div>");

        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var fileName = $"{proc.Schema}_{proc.Name}.html";
        await File.WriteAllTextAsync(Path.Combine(_outputPath, "procedures", fileName), html.ToString());
    }

    private async Task GenerateFunctionsAsync(DatabaseMetadata metadata)
    {
        foreach (var func in metadata.Functions)
        {
            await GenerateFunctionPageAsync(func);
        }
    }

    private async Task GenerateFunctionPageAsync(FunctionInfo func)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"    <title>{Encode(func.Schema)}.{Encode(func.Name)} - Function</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"../css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine($"            <h1>Function: {Encode(func.Schema)}.{Encode(func.Name)}</h1>");
        html.AppendLine("            <p><a href=\"../index.html\">← Back to Index</a></p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");
        html.AppendLine("    <div class=\"container\">");
        
        html.AppendLine("        <div class=\"card\">");
        html.AppendLine("            <h2>Information</h2>");
        html.AppendLine("            <table>");
        html.AppendLine($"                <tr><th>Schema</th><td>{Encode(func.Schema)}</td></tr>");
        html.AppendLine($"                <tr><th>Name</th><td>{Encode(func.Name)}</td></tr>");
        html.AppendLine($"                <tr><th>Type</th><td><span class=\"badge badge-warning\">{Encode(func.Type)}</span></td></tr>");
        html.AppendLine($"                <tr><th>Created</th><td>{func.CreatedDate:yyyy-MM-dd HH:mm:ss}</td></tr>");
        html.AppendLine($"                <tr><th>Modified</th><td>{func.ModifiedDate:yyyy-MM-dd HH:mm:ss}</td></tr>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");

        if (func.Parameters.Any())
        {
            html.AppendLine("        <div class=\"card\">");
            html.AppendLine("            <h2>Parameters</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Parameter</th>");
            html.AppendLine("                        <th>Data Type</th>");
            html.AppendLine("                        <th>Default</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var param in func.Parameters)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(param.Name)}</td>");
                html.AppendLine($"                        <td>{Encode(param.DataType)}{(param.MaxLength.HasValue && param.MaxLength > 0 ? $"({param.MaxLength})" : "")}</td>");
                html.AppendLine($"                        <td>{(param.DefaultValue ?? "")}</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
        }

        html.AppendLine("        <div class=\"card\">");
        html.AppendLine("            <h2>Definition</h2>");
        html.AppendLine($"            <pre style=\"background: #f5f5f5; padding: 15px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap;\">{Encode(func.Definition)}</pre>");
        html.AppendLine("        </div>");

        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var fileName = $"{func.Schema}_{func.Name}.html";
        await File.WriteAllTextAsync(Path.Combine(_outputPath, "functions", fileName), html.ToString());
    }

    private async Task GenerateStatisticsAsync(DatabaseMetadata metadata)
    {
        if (metadata.Statistics == null) return;

        var stats = metadata.Statistics;
        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>Database Statistics - Data Dictionary</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"css/style.css\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("    <header>");
        html.AppendLine("        <div class=\"container\">");
        html.AppendLine($"            <h1>Database Statistics</h1>");
        html.AppendLine($"            <p>{Encode(metadata.ServerName)} / {Encode(metadata.DatabaseName)}</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </header>");

        html.AppendLine("    <div class=\"container\">");
        html.AppendLine("        <nav>");
        html.AppendLine("            <a href=\"index.html\">Home</a>");
        html.AppendLine("            <a href=\"statistics.html\" class=\"active\">Statistics</a>");
        html.AppendLine("            <a href=\"diagram.html\">ER Diagram</a>");
        html.AppendLine("            <a href=\"#tables\">Tables</a>");
        html.AppendLine("            <a href=\"#procedures\">Procedures</a>");
        html.AppendLine("            <a href=\"#functions\">Functions</a>");
        html.AppendLine("            <a href=\"#users\">Users</a>");
        html.AppendLine("            <a href=\"#jobs\">Jobs</a>");
        html.AppendLine("        </nav>");

        // Database Size Overview
        html.AppendLine("        <section>");
        html.AppendLine("            <h2>📊 Database Size Overview</h2>");
        html.AppendLine("            <div class=\"stats-grid\">");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.DatabaseSizeMB:N2} MB</div>");
        html.AppendLine("                    <div class=\"label\">Total Database Size</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.DataSizeMB:N2} MB</div>");
        html.AppendLine("                    <div class=\"label\">Data Size</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.LogSizeMB:N2} MB</div>");
        html.AppendLine("                    <div class=\"label\">Log Size</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.UnallocatedSpaceMB:N2} MB</div>");
        html.AppendLine("                    <div class=\"label\">Unallocated Space</div>");
        html.AppendLine("                </div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </section>");

        // Object Counts
        html.AppendLine("        <section>");
        html.AppendLine("            <h2>🔢 Database Objects</h2>");
        html.AppendLine("            <div class=\"stats-grid\">");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.TotalTables}</div>");
        html.AppendLine("                    <div class=\"label\">Tables</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.TotalViews}</div>");
        html.AppendLine("                    <div class=\"label\">Views</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.TotalStoredProcedures}</div>");
        html.AppendLine("                    <div class=\"label\">Stored Procedures</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.TotalFunctions}</div>");
        html.AppendLine("                    <div class=\"label\">Functions</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.TotalTriggers}</div>");
        html.AppendLine("                    <div class=\"label\">Triggers</div>");
        html.AppendLine("                </div>");
        html.AppendLine("                <div class=\"stat-card\">");
        html.AppendLine($"                    <div class=\"number\">{stats.TotalIndexes}</div>");
        html.AppendLine("                    <div class=\"label\">Indexes</div>");
        html.AppendLine("                </div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </section>");

        // Largest Tables
        if (stats.LargestTables.Any())
        {
            html.AppendLine("        <section>");
            html.AppendLine("            <h2>💾 Top 10 Largest Tables</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Schema</th>");
            html.AppendLine("                        <th>Table Name</th>");
            html.AppendLine("                        <th>Row Count</th>");
            html.AppendLine("                        <th>Total Space (MB)</th>");
            html.AppendLine("                        <th>Data Space (MB)</th>");
            html.AppendLine("                        <th>Index Space (MB)</th>");
            html.AppendLine("                        <th>Unused Space (MB)</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var table in stats.LargestTables)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{Encode(table.Schema)}</td>");
                html.AppendLine($"                        <td><a href=\"tables/{Encode(table.Schema)}_{Encode(table.TableName)}.html\">{Encode(table.TableName)}</a></td>");
                html.AppendLine($"                        <td>{table.RowCount:N0}</td>");
                html.AppendLine($"                        <td>{table.TotalSpaceMB:N2}</td>");
                html.AppendLine($"                        <td>{table.DataSpaceMB:N2}</td>");
                html.AppendLine($"                        <td>{table.IndexSpaceMB:N2}</td>");
                html.AppendLine($"                        <td>{table.UnusedSpaceMB:N2}</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </section>");
        }

        // Top Queries
        if (stats.TopQueries.Any())
        {
            html.AppendLine("        <section>");
            html.AppendLine("            <h2>⚡ Top 10 Queries by Total Elapsed Time</h2>");
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Query Text</th>");
            html.AppendLine("                        <th>Execution Count</th>");
            html.AppendLine("                        <th>Total Time (ms)</th>");
            html.AppendLine("                        <th>Avg Time (ms)</th>");
            html.AppendLine("                        <th>Total Reads</th>");
            html.AppendLine("                        <th>Avg Reads</th>");
            html.AppendLine("                        <th>Last Execution</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");

            foreach (var query in stats.TopQueries)
            {
                var truncatedQuery = query.QueryText.Length > 100 
                    ? query.QueryText.Substring(0, 100) + "..." 
                    : query.QueryText;

                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td><code title=\"{Encode(query.QueryText)}\">{Encode(truncatedQuery)}</code></td>");
                html.AppendLine($"                        <td>{query.ExecutionCount:N0}</td>");
                html.AppendLine($"                        <td>{query.TotalElapsedTimeMs:N2}</td>");
                html.AppendLine($"                        <td>{query.AvgElapsedTimeMs:N2}</td>");
                html.AppendLine($"                        <td>{query.TotalLogicalReads:N0}</td>");
                html.AppendLine($"                        <td>{query.AvgLogicalReads:N2}</td>");
                html.AppendLine($"                        <td>{query.LastExecutionTime:yyyy-MM-dd HH:mm:ss}</td>");
                html.AppendLine("                    </tr>");
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </section>");
        }

        html.AppendLine($"        <footer><p>Generated on {stats.CollectedAt:yyyy-MM-dd HH:mm:ss} UTC</p></footer>");
        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        await File.WriteAllTextAsync(Path.Combine(_outputPath, "statistics.html"), html.ToString());
    }

    private string Encode(string text)
    {
        return HttpUtility.HtmlEncode(text);
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}

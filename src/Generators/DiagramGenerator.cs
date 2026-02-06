using System.Text;
using DataDic.Models;

namespace DataDic.Generators;

public class DiagramGenerator
{
    private const int TABLE_WIDTH = 250;
    private const int COLUMN_HEIGHT = 25;
    private const int HEADER_HEIGHT = 35;
    private const int PADDING = 20;
    private const int SCHEMA_SPACING = 100;

    public string GenerateSvgDiagram(DatabaseMetadata metadata)
    {
        // Only include actual tables (not views) for ER diagram
        var tables = metadata.Tables.Where(t => t.Type == "USER_TABLE").ToList();
        
        if (!tables.Any())
            return GenerateEmptyDiagram();

        // Calculate layout positions
        var layout = CalculateLayout(tables);
        
        // Calculate SVG dimensions
        var maxX = layout.Values.Max(p => p.X) + TABLE_WIDTH + PADDING * 2;
        var maxY = layout.Values.Max(p => p.Y) + layout.Keys.Max(t => GetTableHeight(t)) + PADDING * 2;

        var svg = new StringBuilder();
        svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {maxX} {maxY}\" width=\"100%\" height=\"{maxY}\" style=\"border: 1px solid #ddd; background: #f9f9f9;\">");
        
        // Add styles
        svg.AppendLine(GenerateStyles());
        
        // Add definitions for markers (arrows)
        svg.AppendLine(GenerateMarkerDefs());
        
        // Draw relationships first (so they appear behind tables)
        foreach (var table in tables)
        {
            foreach (var fk in table.ForeignKeys)
            {
                var referencedTable = tables.FirstOrDefault(t => 
                    t.Schema == fk.ReferencedSchema && t.Name == fk.ReferencedTable);
                
                if (referencedTable != null && layout.ContainsKey(table) && layout.ContainsKey(referencedTable))
                {
                    svg.AppendLine(GenerateRelationship(table, referencedTable, layout[table], layout[referencedTable], fk));
                }
            }
        }
        
        // Draw tables
        foreach (var table in tables)
        {
            if (layout.ContainsKey(table))
            {
                svg.AppendLine(GenerateTable(table, layout[table]));
            }
        }
        
        svg.AppendLine("</svg>");
        
        return svg.ToString();
    }

    private Dictionary<TableInfo, (int X, int Y)> CalculateLayout(List<TableInfo> tables)
    {
        var layout = new Dictionary<TableInfo, (int X, int Y)>();
        
        // Group tables by schema
        var schemaGroups = tables.GroupBy(t => t.Schema).OrderBy(g => g.Key).ToList();
        
        int schemaX = PADDING;
        
        foreach (var schemaGroup in schemaGroups)
        {
            int currentY = PADDING + 50; // Leave space for schema label
            int maxWidth = 0;
            
            foreach (var table in schemaGroup.OrderBy(t => t.Name))
            {
                layout[table] = (schemaX, currentY);
                currentY += GetTableHeight(table) + PADDING;
                maxWidth = Math.Max(maxWidth, TABLE_WIDTH);
            }
            
            schemaX += maxWidth + SCHEMA_SPACING;
        }
        
        return layout;
    }

    private int GetTableHeight(TableInfo table)
    {
        // Header + columns (max 15 visible) + footer
        int visibleColumns = Math.Min(table.Columns.Count, 15);
        return HEADER_HEIGHT + (visibleColumns * COLUMN_HEIGHT) + 10;
    }

    private string GenerateStyles()
    {
        return @"
    <style>
        .table-box {
            cursor: pointer;
            transition: filter 0.2s;
        }
        .table-box:hover {
            filter: drop-shadow(0 4px 8px rgba(0,0,0,0.2));
        }
        .table-header {
            fill: #2c3e50;
            stroke: #34495e;
            stroke-width: 2;
        }
        .table-body {
            fill: white;
            stroke: #34495e;
            stroke-width: 2;
        }
        .table-name {
            fill: white;
            font-family: 'Segoe UI', Arial, sans-serif;
            font-size: 14px;
            font-weight: bold;
        }
        .schema-name {
            fill: #95a5a6;
            font-family: 'Segoe UI', Arial, sans-serif;
            font-size: 11px;
        }
        .column-name {
            fill: #2c3e50;
            font-family: 'Consolas', 'Courier New', monospace;
            font-size: 12px;
        }
        .column-type {
            fill: #7f8c8d;
            font-family: 'Consolas', 'Courier New', monospace;
            font-size: 11px;
        }
        .pk-icon {
            fill: #f39c12;
            font-weight: bold;
            font-size: 12px;
        }
        .fk-icon {
            fill: #3498db;
            font-weight: bold;
            font-size: 12px;
        }
        .relationship-line {
            stroke: #3498db;
            stroke-width: 2;
            fill: none;
            opacity: 0.6;
        }
        .relationship-line:hover {
            stroke-width: 3;
            opacity: 1;
        }
        .schema-label {
            fill: #2c3e50;
            font-family: 'Segoe UI', Arial, sans-serif;
            font-size: 16px;
            font-weight: bold;
        }
    </style>";
    }

    private string GenerateMarkerDefs()
    {
        return @"
    <defs>
        <marker id=""arrowhead"" markerWidth=""10"" markerHeight=""10"" refX=""8"" refY=""3"" orient=""auto"">
            <polygon points=""0 0, 10 3, 0 6"" fill=""#3498db"" />
        </marker>
        <marker id=""one"" markerWidth=""10"" markerHeight=""10"" refX=""0"" refY=""5"" orient=""auto"">
            <line x1=""0"" y1=""0"" x2=""0"" y2=""10"" stroke=""#3498db"" stroke-width=""2""/>
        </marker>
        <marker id=""many"" markerWidth=""15"" markerHeight=""10"" refX=""0"" refY=""5"" orient=""auto"">
            <line x1=""0"" y1=""5"" x2=""10"" y2=""0"" stroke=""#3498db"" stroke-width=""2""/>
            <line x1=""0"" y1=""5"" x2=""10"" y2=""10"" stroke=""#3498db"" stroke-width=""2""/>
        </marker>
    </defs>";
    }

    private string GenerateTable(TableInfo table, (int X, int Y) position)
    {
        var svg = new StringBuilder();
        var tableHeight = GetTableHeight(table);
        
        // Create clickable group
        svg.AppendLine($"    <a href=\"tables/{SanitizeFileName(table.Schema)}_{SanitizeFileName(table.Name)}.html\">");
        svg.AppendLine($"        <g class=\"table-box\" transform=\"translate({position.X},{position.Y})\">");
        
        // Table header
        svg.AppendLine($"            <rect class=\"table-header\" width=\"{TABLE_WIDTH}\" height=\"{HEADER_HEIGHT}\" rx=\"5\" ry=\"5\"/>");
        svg.AppendLine($"            <rect class=\"table-header\" width=\"{TABLE_WIDTH}\" height=\"{HEADER_HEIGHT / 2}\" y=\"{HEADER_HEIGHT / 2}\"/>");
        
        // Schema name
        svg.AppendLine($"            <text class=\"schema-name\" x=\"10\" y=\"15\">{Encode(table.Schema)}</text>");
        
        // Table name
        svg.AppendLine($"            <text class=\"table-name\" x=\"10\" y=\"30\">{Encode(table.Name)}</text>");
        
        // Table body
        var bodyHeight = tableHeight - HEADER_HEIGHT;
        svg.AppendLine($"            <rect class=\"table-body\" y=\"{HEADER_HEIGHT}\" width=\"{TABLE_WIDTH}\" height=\"{bodyHeight}\" rx=\"0\" ry=\"0\"/>");
        svg.AppendLine($"            <rect class=\"table-body\" y=\"{HEADER_HEIGHT}\" width=\"{TABLE_WIDTH}\" height=\"5\"/>");
        
        // Columns (max 15 visible)
        var visibleColumns = table.Columns.Take(15).ToList();
        int yPos = HEADER_HEIGHT + 20;
        
        foreach (var column in visibleColumns)
        {
            // Primary key icon
            if (column.IsPrimaryKey)
            {
                svg.AppendLine($"            <text class=\"pk-icon\" x=\"10\" y=\"{yPos}\">🔑</text>");
            }
            
            // Foreign key icon (check if this column is part of any FK)
            var isForeignKey = table.ForeignKeys.Any(fk => 
                fk.ColumnMappings.Any(cm => cm.Column == column.Name));
            
            if (isForeignKey && !column.IsPrimaryKey)
            {
                svg.AppendLine($"            <text class=\"fk-icon\" x=\"10\" y=\"{yPos}\">🔗</text>");
            }
            
            // Column name
            var xOffset = column.IsPrimaryKey || isForeignKey ? 30 : 10;
            var columnName = column.Name.Length > 20 ? column.Name.Substring(0, 17) + "..." : column.Name;
            svg.AppendLine($"            <text class=\"column-name\" x=\"{xOffset}\" y=\"{yPos}\">{Encode(columnName)}</text>");
            
            // Data type
            var dataType = GetShortDataType(column);
            svg.AppendLine($"            <text class=\"column-type\" x=\"{TABLE_WIDTH - 10}\" y=\"{yPos}\" text-anchor=\"end\">{Encode(dataType)}</text>");
            
            yPos += COLUMN_HEIGHT;
        }
        
        // Show "..." if there are more columns
        if (table.Columns.Count > 15)
        {
            svg.AppendLine($"            <text class=\"column-type\" x=\"{TABLE_WIDTH / 2}\" y=\"{yPos}\" text-anchor=\"middle\">... {table.Columns.Count - 15} more columns</text>");
        }
        
        svg.AppendLine("        </g>");
        svg.AppendLine("    </a>");
        
        return svg.ToString();
    }

    private string GenerateRelationship(TableInfo fromTable, TableInfo toTable, 
        (int X, int Y) fromPos, (int X, int Y) toPos, ForeignKeyInfo fk)
    {
        // Calculate connection points (right side of from table, left side of to table)
        int fromX = fromPos.X + TABLE_WIDTH;
        int fromY = fromPos.Y + HEADER_HEIGHT + (GetColumnIndex(fromTable, fk.ColumnMappings[0].Column) * COLUMN_HEIGHT) + COLUMN_HEIGHT / 2;
        
        int toX = toPos.X;
        int toY = toPos.Y + HEADER_HEIGHT + (GetColumnIndex(toTable, fk.ColumnMappings[0].ReferencedColumn) * COLUMN_HEIGHT) + COLUMN_HEIGHT / 2;
        
        // Create a curved path
        var midX = (fromX + toX) / 2;
        
        var path = $"M {fromX},{fromY} C {midX},{fromY} {midX},{toY} {toX},{toY}";
        
        return $"    <path class=\"relationship-line\" d=\"{path}\" marker-end=\"url(#arrowhead)\" marker-start=\"url(#many)\"><title>{Encode(fk.Name)}</title></path>";
    }

    private int GetColumnIndex(TableInfo table, string columnName)
    {
        var index = table.Columns.FindIndex(c => c.Name == columnName);
        return index >= 0 ? Math.Min(index, 14) : 0;
    }

    private string GetShortDataType(ColumnInfo column)
    {
        var dataType = column.DataType.ToLower();
        
        if (column.MaxLength.HasValue && column.MaxLength > 0 && column.MaxLength < 8000)
        {
            return $"{dataType}({column.MaxLength})";
        }
        else if (column.Precision.HasValue && column.Scale.HasValue)
        {
            return $"{dataType}({column.Precision},{column.Scale})";
        }
        else if (dataType.Contains("varchar") && column.MaxLength == -1)
        {
            return $"{dataType}(max)";
        }
        
        return dataType;
    }

    private string GenerateEmptyDiagram()
    {
        return @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 800 400"" width=""100%"" height=""400"">
    <rect width=""800"" height=""400"" fill=""#f9f9f9"" stroke=""#ddd""/>
    <text x=""400"" y=""200"" text-anchor=""middle"" font-family=""Arial"" font-size=""18"" fill=""#95a5a6"">
        No tables available for ER diagram
    </text>
</svg>";
    }

    private string Encode(string text)
    {
        return text.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\"", "&quot;")
                   .Replace("'", "&apos;");
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}

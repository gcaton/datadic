using DataDic.Providers;
using DataDic.Generators;

// Parse command line arguments
string? connectionString = null;
string output = "./output";
string provider = "sqlserver";

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-c" or "--connection-string":
            connectionString = args[++i];
            break;
        case "-o" or "--output":
            output = args[++i];
            break;
        case "-p" or "--provider":
            provider = args[++i];
            break;
        case "-h" or "--help":
            ShowHelp();
            return 0;
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: Connection string is required");
    ShowHelp();
    return 1;
}

try
{
    Console.WriteLine("=== DataDic - Database Data Dictionary Generator ===");
    Console.WriteLine();

    IDatabaseProvider dbProvider = provider.ToLower() switch
    {
        "sqlserver" => new SqlServerProvider(),
        _ => throw new ArgumentException($"Unsupported provider: {provider}")
    };

    Console.WriteLine($"Provider: {dbProvider.ProviderName}");
    Console.WriteLine("Connecting to database...");
    
    var metadata = await dbProvider.GetMetadataAsync(connectionString);
    
    Console.WriteLine($"Connected to: {metadata.ServerName} / {metadata.DatabaseName}");
    Console.WriteLine();
    Console.WriteLine($"Found {metadata.Tables.Count} tables/views");
    Console.WriteLine($"Found {metadata.StoredProcedures.Count} stored procedures");
    Console.WriteLine($"Found {metadata.Functions.Count} functions");
    Console.WriteLine($"Found {metadata.Users.Count} users");
    Console.WriteLine($"Found {metadata.Jobs.Count} jobs");
    Console.WriteLine();
    Console.WriteLine("Generating HTML documentation...");

    var htmlGenerator = new HtmlGenerator(output);
    await htmlGenerator.GenerateAsync(metadata);

    Console.WriteLine($"Documentation generated successfully!");
    Console.WriteLine($"Output location: {Path.GetFullPath(output)}");
    Console.WriteLine($"Open {Path.Combine(Path.GetFullPath(output), "index.html")} in your browser");
    return 0;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    Console.ResetColor();
    return 1;
}

static void ShowHelp()
{
    Console.WriteLine("DataDic - Database Data Dictionary Generator");
    Console.WriteLine();
    Console.WriteLine("Usage: datadic [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -c, --connection-string <string>  Database connection string (required)");
    Console.WriteLine("  -o, --output <path>               Output directory (default: ./output)");
    Console.WriteLine("  -p, --provider <name>             Database provider: sqlserver (default)");
    Console.WriteLine("  -h, --help                        Show help");
}

using System.CommandLine;
using DataDic.Providers;
using DataDic.Generators;

var connectionStringOption = new Option<string>(
    name: "--connection-string",
    description: "Database connection string")
{
    IsRequired = true
};
connectionStringOption.AddAlias("-c");

var outputPathOption = new Option<string>(
    name: "--output",
    description: "Output directory for HTML files",
    getDefaultValue: () => "./output");
outputPathOption.AddAlias("-o");

var providerOption = new Option<string>(
    name: "--provider",
    description: "Database provider (sqlserver, postgres)",
    getDefaultValue: () => "sqlserver");
providerOption.AddAlias("-p");

var rootCommand = new RootCommand("DataDic - Database Data Dictionary Generator");
rootCommand.AddOption(connectionStringOption);
rootCommand.AddOption(outputPathOption);
rootCommand.AddOption(providerOption);

rootCommand.SetHandler(async (string connectionString, string outputPath, string provider) =>
{
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

        var htmlGenerator = new HtmlGenerator(outputPath);
        await htmlGenerator.GenerateAsync(metadata);

        Console.WriteLine($"Documentation generated successfully!");
        Console.WriteLine($"Output location: {Path.GetFullPath(outputPath)}");
        Console.WriteLine($"Open {Path.Combine(Path.GetFullPath(outputPath), "index.html")} in your browser");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, connectionStringOption, outputPathOption, providerOption);

return await rootCommand.InvokeAsync(args);

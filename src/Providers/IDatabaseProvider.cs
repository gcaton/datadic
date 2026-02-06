using DataDic.Models;

namespace DataDic.Providers;

public interface IDatabaseProvider
{
    Task<DatabaseMetadata> GetMetadataAsync(string connectionString);
    string ProviderName { get; }
}

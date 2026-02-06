using DataDic.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataDic.Providers;

public class SqlServerProvider : IDatabaseProvider
{
    public string ProviderName => "SQL Server";

    public async Task<DatabaseMetadata> GetMetadataAsync(string connectionString)
    {
        var metadata = new DatabaseMetadata();

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        metadata.DatabaseName = connection.Database;
        metadata.ServerName = connection.DataSource;

        metadata.Tables = await GetTablesAsync(connection);
        metadata.Users = await GetUsersAsync(connection);
        metadata.Jobs = await GetJobsAsync(connection);
        metadata.StoredProcedures = await GetStoredProceduresAsync(connection);
        metadata.Functions = await GetFunctionsAsync(connection);
        metadata.Statistics = await GetStatisticsAsync(connection);

        return metadata;
    }

    private async Task<List<TableInfo>> GetTablesAsync(SqlConnection connection)
    {
        var tables = new List<TableInfo>();

        var query = @"
            SELECT 
                s.name AS SchemaName,
                t.name AS TableName,
                t.type_desc AS TableType
            FROM sys.tables t
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            UNION ALL
            SELECT 
                s.name AS SchemaName,
                v.name AS TableName,
                'VIEW' AS TableType
            FROM sys.views v
            INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
            ORDER BY SchemaName, TableName";

        using (var cmd = new SqlCommand(query, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var table = new TableInfo
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2)
                };
                tables.Add(table);
            }
        }

        foreach (var table in tables)
        {
            table.Columns = await GetColumnsAsync(connection, table.Schema, table.Name);
            table.ForeignKeys = await GetForeignKeysAsync(connection, table.Schema, table.Name);
            table.Indexes = await GetIndexesAsync(connection, table.Schema, table.Name);
            table.Triggers = await GetTriggersAsync(connection, table.Schema, table.Name);
            table.RowCount = await GetRowCountAsync(connection, table.Schema, table.Name);
            
            // Get view definition if it's a view
            if (table.Type == "VIEW")
            {
                table.Definition = await GetViewDefinitionAsync(connection, table.Schema, table.Name);
            }
        }

        return tables;
    }

    private async Task<List<ColumnInfo>> GetColumnsAsync(SqlConnection connection, string schema, string tableName)
    {
        var columns = new List<ColumnInfo>();

        var query = @"
            SELECT 
                c.name AS ColumnName,
                t.name AS DataType,
                c.max_length AS MaxLength,
                c.precision AS Precision,
                c.scale AS Scale,
                c.is_nullable AS IsNullable,
                c.is_identity AS IsIdentity,
                ISNULL(dc.definition, '') AS DefaultValue,
                ISNULL(ep.value, '') AS Description,
                ISNULL(pk.is_primary_key, 0) AS IsPrimaryKey
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            INNER JOIN sys.tables tb ON c.object_id = tb.object_id
            INNER JOIN sys.schemas s ON tb.schema_id = s.schema_id
            LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
            LEFT JOIN sys.extended_properties ep ON ep.major_id = c.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description'
            LEFT JOIN (
                SELECT ic.object_id, ic.column_id, 1 AS is_primary_key
                FROM sys.index_columns ic
                INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                WHERE i.is_primary_key = 1
            ) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
            WHERE s.name = @Schema AND tb.name = @TableName
            ORDER BY c.column_id";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Schema", schema);
        cmd.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString(0),
                DataType = reader.GetString(1),
                MaxLength = reader.IsDBNull(2) ? null : reader.GetInt16(2),
                Precision = reader.IsDBNull(3) ? null : reader.GetByte(3),
                Scale = reader.IsDBNull(4) ? null : reader.GetByte(4),
                IsNullable = reader.GetBoolean(5),
                IsIdentity = reader.GetBoolean(6),
                DefaultValue = reader.IsDBNull(7) ? null : reader.GetString(7),
                Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                IsPrimaryKey = reader.GetInt32(9) == 1
            });
        }

        return columns;
    }

    private async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(SqlConnection connection, string schema, string tableName)
    {
        var foreignKeys = new List<ForeignKeyInfo>();

        var query = @"
            SELECT 
                fk.name AS FKName,
                SCHEMA_NAME(rt.schema_id) AS ReferencedSchema,
                rt.name AS ReferencedTable,
                c.name AS ColumnName,
                rc.name AS ReferencedColumnName
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            INNER JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
            INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
            INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
            WHERE s.name = @Schema AND t.name = @TableName
            ORDER BY fk.name, fkc.constraint_column_id";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Schema", schema);
        cmd.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await cmd.ExecuteReaderAsync();

        var fkDict = new Dictionary<string, ForeignKeyInfo>();

        while (await reader.ReadAsync())
        {
            var fkName = reader.GetString(0);

            if (!fkDict.ContainsKey(fkName))
            {
                fkDict[fkName] = new ForeignKeyInfo
                {
                    Name = fkName,
                    ReferencedSchema = reader.GetString(1),
                    ReferencedTable = reader.GetString(2)
                };
            }

            fkDict[fkName].ColumnMappings.Add((reader.GetString(3), reader.GetString(4)));
        }

        return fkDict.Values.ToList();
    }

    private async Task<List<IndexInfo>> GetIndexesAsync(SqlConnection connection, string schema, string tableName)
    {
        var indexes = new List<IndexInfo>();

        var query = @"
            SELECT 
                i.name AS IndexName,
                i.is_unique AS IsUnique,
                i.is_primary_key AS IsPrimaryKey,
                c.name AS ColumnName
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = @Schema AND t.name = @TableName AND i.name IS NOT NULL
            ORDER BY i.name, ic.key_ordinal";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Schema", schema);
        cmd.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await cmd.ExecuteReaderAsync();

        var indexDict = new Dictionary<string, IndexInfo>();

        while (await reader.ReadAsync())
        {
            var indexName = reader.GetString(0);

            if (!indexDict.ContainsKey(indexName))
            {
                indexDict[indexName] = new IndexInfo
                {
                    Name = indexName,
                    IsUnique = reader.GetBoolean(1),
                    IsPrimaryKey = reader.GetBoolean(2)
                };
            }

            indexDict[indexName].Columns.Add(reader.GetString(3));
        }

        return indexDict.Values.ToList();
    }

    private async Task<long> GetRowCountAsync(SqlConnection connection, string schema, string tableName)
    {
        try
        {
            var query = @"
                SELECT SUM(p.rows) 
                FROM sys.partitions p
                INNER JOIN sys.tables t ON p.object_id = t.object_id
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = @Schema AND t.name = @TableName AND p.index_id IN (0, 1)";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Schema", schema);
            cmd.Parameters.AddWithValue("@TableName", tableName);

            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<List<UserInfo>> GetUsersAsync(SqlConnection connection)
    {
        var users = new List<UserInfo>();

        var query = @"
            SELECT 
                dp.name AS UserName,
                dp.type_desc AS UserType
            FROM sys.database_principals dp
            WHERE dp.type IN ('S', 'U', 'G')
            AND dp.name NOT IN ('dbo', 'guest', 'INFORMATION_SCHEMA', 'sys')
            ORDER BY dp.name";

        using (var cmd = new SqlCommand(query, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                users.Add(new UserInfo
                {
                    Name = reader.GetString(0),
                    Type = reader.GetString(1)
                });
            }
        }

        foreach (var user in users)
        {
            user.Roles = await GetUserRolesAsync(connection, user.Name);
            user.Permissions = await GetUserPermissionsAsync(connection, user.Name);
        }

        return users;
    }

    private async Task<List<string>> GetUserRolesAsync(SqlConnection connection, string userName)
    {
        var roles = new List<string>();

        var query = @"
            SELECT r.name
            FROM sys.database_role_members rm
            INNER JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
            INNER JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id
            WHERE m.name = @UserName";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@UserName", userName);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

    private async Task<List<PermissionInfo>> GetUserPermissionsAsync(SqlConnection connection, string userName)
    {
        var permissions = new List<PermissionInfo>();

        var query = @"
            SELECT 
                OBJECT_NAME(p.major_id) AS ObjectName,
                p.permission_name AS Permission,
                p.state_desc AS GrantType
            FROM sys.database_permissions p
            INNER JOIN sys.database_principals dp ON p.grantee_principal_id = dp.principal_id
            WHERE dp.name = @UserName AND p.major_id > 0
            ORDER BY ObjectName, Permission";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@UserName", userName);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            permissions.Add(new PermissionInfo
            {
                ObjectName = reader.IsDBNull(0) ? "DATABASE" : reader.GetString(0),
                Permission = reader.GetString(1),
                GrantType = reader.GetString(2)
            });
        }

        return permissions;
    }

    private async Task<List<JobInfo>> GetJobsAsync(SqlConnection connection)
    {
        var jobs = new List<JobInfo>();

        try
        {
            var query = @"
                SELECT 
                    j.name AS JobName,
                    j.enabled AS IsEnabled,
                    ISNULL(j.description, '') AS Description
                FROM msdb.dbo.sysjobs j
                ORDER BY j.name";

            using (var cmd = new SqlCommand(query, connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    jobs.Add(new JobInfo
                    {
                        Name = reader.GetString(0),
                        IsEnabled = reader.GetByte(1) == 1,
                        Description = reader.GetString(2)
                    });
                }
            }

            foreach (var job in jobs)
            {
                job.Schedules = await GetJobSchedulesAsync(connection, job.Name);
                job.Steps = await GetJobStepsAsync(connection, job.Name);
            }
        }
        catch
        {
            // If msdb is not accessible, return empty list
        }

        return jobs;
    }

    private async Task<List<JobScheduleInfo>> GetJobSchedulesAsync(SqlConnection connection, string jobName)
    {
        var schedules = new List<JobScheduleInfo>();

        try
        {
            var query = @"
                SELECT 
                    s.name AS ScheduleName,
                    s.enabled AS IsEnabled,
                    CASE s.freq_type
                        WHEN 1 THEN 'Once'
                        WHEN 4 THEN 'Daily'
                        WHEN 8 THEN 'Weekly'
                        WHEN 16 THEN 'Monthly'
                        WHEN 32 THEN 'Monthly relative'
                        WHEN 64 THEN 'When SQL Server Agent starts'
                        WHEN 128 THEN 'When computer is idle'
                        ELSE 'Unknown'
                    END AS FrequencyType,
                    CAST(s.freq_interval AS VARCHAR(10)) AS FrequencyInterval,
                    RIGHT('0' + CAST(s.active_start_time / 10000 AS VARCHAR(2)), 2) + ':' +
                    RIGHT('0' + CAST((s.active_start_time % 10000) / 100 AS VARCHAR(2)), 2) AS ActiveStartTime
                FROM msdb.dbo.sysjobs j
                INNER JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
                INNER JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
                WHERE j.name = @JobName";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@JobName", jobName);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                schedules.Add(new JobScheduleInfo
                {
                    Name = reader.GetString(0),
                    IsEnabled = reader.GetByte(1) == 1,
                    FrequencyType = reader.GetString(2),
                    FrequencyInterval = reader.GetString(3),
                    ActiveStartTime = reader.GetString(4)
                });
            }
        }
        catch
        {
            // If query fails, return empty list
        }

        return schedules;
    }

    private async Task<List<JobStepInfo>> GetJobStepsAsync(SqlConnection connection, string jobName)
    {
        var steps = new List<JobStepInfo>();

        try
        {
            var query = @"
                SELECT 
                    s.step_id AS StepId,
                    s.step_name AS StepName,
                    s.subsystem AS Subsystem,
                    s.command AS Command
                FROM msdb.dbo.sysjobs j
                INNER JOIN msdb.dbo.sysjobsteps s ON j.job_id = s.job_id
                WHERE j.name = @JobName
                ORDER BY s.step_id";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@JobName", jobName);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                steps.Add(new JobStepInfo
                {
                    StepId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Subsystem = reader.GetString(2),
                    Command = reader.GetString(3)
                });
            }
        }
        catch
        {
            // If query fails, return empty list
        }

        return steps;
    }

    private async Task<List<TriggerInfo>> GetTriggersAsync(SqlConnection connection, string schema, string tableName)
    {
        var triggers = new List<TriggerInfo>();

        var query = @"
            SELECT 
                tr.name AS TriggerName,
                CASE WHEN tr.is_instead_of_trigger = 1 THEN 'INSTEAD OF' ELSE 'AFTER' END AS TriggerType,
                STUFF((
                    SELECT ', ' + type_desc
                    FROM (
                        SELECT CASE WHEN OBJECTPROPERTY(tr.object_id, 'ExecIsInsertTrigger') = 1 THEN 'INSERT' END AS type_desc
                        UNION ALL
                        SELECT CASE WHEN OBJECTPROPERTY(tr.object_id, 'ExecIsUpdateTrigger') = 1 THEN 'UPDATE' END
                        UNION ALL
                        SELECT CASE WHEN OBJECTPROPERTY(tr.object_id, 'ExecIsDeleteTrigger') = 1 THEN 'DELETE' END
                    ) events
                    WHERE type_desc IS NOT NULL
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Events,
                tr.is_disabled,
                OBJECT_DEFINITION(tr.object_id) AS Definition
            FROM sys.triggers tr
            INNER JOIN sys.tables t ON tr.parent_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = @Schema AND t.name = @TableName";

        using (var cmd = new SqlCommand(query, connection))
        {
            cmd.Parameters.AddWithValue("@Schema", schema);
            cmd.Parameters.AddWithValue("@TableName", tableName);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    triggers.Add(new TriggerInfo
                    {
                        Name = reader.GetString(0),
                        Type = reader.GetString(1),
                        Events = reader.GetString(2),
                        IsEnabled = !reader.GetBoolean(3),
                        Definition = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                    });
                }
            }
        }

        return triggers;
    }

    private async Task<string?> GetViewDefinitionAsync(SqlConnection connection, string schema, string viewName)
    {
        var query = @"
            SELECT OBJECT_DEFINITION(OBJECT_ID(@Schema + '.' + @ViewName))";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Schema", schema);
        cmd.Parameters.AddWithValue("@ViewName", viewName);

        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    private async Task<List<StoredProcedureInfo>> GetStoredProceduresAsync(SqlConnection connection)
    {
        var procedures = new List<StoredProcedureInfo>();

        var query = @"
            SELECT 
                s.name AS SchemaName,
                p.name AS ProcedureName,
                p.create_date AS CreatedDate,
                p.modify_date AS ModifiedDate,
                OBJECT_DEFINITION(p.object_id) AS Definition
            FROM sys.procedures p
            INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
            WHERE p.is_ms_shipped = 0
            ORDER BY s.name, p.name";

        using (var cmd = new SqlCommand(query, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var proc = new StoredProcedureInfo
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    CreatedDate = reader.GetDateTime(2),
                    ModifiedDate = reader.GetDateTime(3),
                    Definition = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                };
                procedures.Add(proc);
            }
        }

        // Get parameters for each procedure
        foreach (var proc in procedures)
        {
            proc.Parameters = await GetProcedureParametersAsync(connection, proc.Schema, proc.Name);
        }

        return procedures;
    }

    private async Task<List<FunctionInfo>> GetFunctionsAsync(SqlConnection connection)
    {
        var functions = new List<FunctionInfo>();

        var query = @"
            SELECT 
                s.name AS SchemaName,
                o.name AS FunctionName,
                o.type_desc AS FunctionType,
                o.create_date AS CreatedDate,
                o.modify_date AS ModifiedDate,
                OBJECT_DEFINITION(o.object_id) AS Definition
            FROM sys.objects o
            INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
            WHERE o.type IN ('FN', 'IF', 'TF') -- Scalar, Inline Table-Valued, Table-Valued
            AND o.is_ms_shipped = 0
            ORDER BY s.name, o.name";

        using (var cmd = new SqlCommand(query, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var func = new FunctionInfo
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedDate = reader.GetDateTime(3),
                    ModifiedDate = reader.GetDateTime(4),
                    Definition = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                };
                functions.Add(func);
            }
        }

        // Get parameters for each function
        foreach (var func in functions)
        {
            func.Parameters = await GetFunctionParametersAsync(connection, func.Schema, func.Name);
        }

        return functions;
    }

    private async Task<List<ParameterInfo>> GetProcedureParametersAsync(SqlConnection connection, string schema, string procName)
    {
        var parameters = new List<ParameterInfo>();

        var query = @"
            SELECT 
                p.name AS ParameterName,
                TYPE_NAME(p.user_type_id) AS DataType,
                p.max_length AS MaxLength,
                p.is_output AS IsOutput
            FROM sys.parameters p
            WHERE p.object_id = OBJECT_ID(@Schema + '.' + @ProcName)
            ORDER BY p.parameter_id";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Schema", schema);
        cmd.Parameters.AddWithValue("@ProcName", procName);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            parameters.Add(new ParameterInfo
            {
                Name = reader.GetString(0),
                DataType = reader.GetString(1),
                MaxLength = reader.IsDBNull(2) ? null : (int?)reader.GetInt16(2),
                IsOutput = reader.GetBoolean(3),
                DefaultValue = null
            });
        }

        return parameters;
    }

    private async Task<List<ParameterInfo>> GetFunctionParametersAsync(SqlConnection connection, string schema, string funcName)
    {
        // Same implementation as procedure parameters
        return await GetProcedureParametersAsync(connection, schema, funcName);
    }

    private async Task<DatabaseStatistics> GetStatisticsAsync(SqlConnection connection)
    {
        var stats = new DatabaseStatistics
        {
            CollectedAt = DateTime.UtcNow
        };

        // Get database size information
        var sizeQuery = @"
            -- Get data and log file sizes from database files
            SELECT 
                SUM(size * 8.0 / 1024) AS DatabaseSizeMB,
                SUM(CASE WHEN type = 0 THEN size * 8.0 / 1024 ELSE 0 END) AS DataSizeMB,
                SUM(CASE WHEN type = 1 THEN size * 8.0 / 1024 ELSE 0 END) AS LogSizeMB
            FROM sys.database_files;
            
            -- Get unallocated space
            SELECT 
                (SUM(size) * 8.0 / 1024) - SUM(FILEPROPERTY(name, 'SpaceUsed') * 8.0 / 1024) AS UnallocatedSpaceMB
            FROM sys.database_files
            WHERE type = 0;";

        using (var cmd = new SqlCommand(sizeQuery, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                stats.DatabaseSizeMB = reader.GetDecimal(0);
                stats.DataSizeMB = reader.GetDecimal(1);
                stats.LogSizeMB = reader.GetDecimal(2);
            }

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                stats.UnallocatedSpaceMB = reader.GetDecimal(0);
            }
        }

        // Get object counts
        var countQuery = @"
            SELECT 
                (SELECT COUNT(*) FROM sys.tables) AS TotalTables,
                (SELECT COUNT(*) FROM sys.views) AS TotalViews,
                (SELECT COUNT(*) FROM sys.procedures WHERE is_ms_shipped = 0) AS TotalStoredProcedures,
                (SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN', 'IF', 'TF')) AS TotalFunctions,
                (SELECT COUNT(*) FROM sys.triggers WHERE parent_class = 1) AS TotalTriggers,
                (SELECT COUNT(*) FROM sys.indexes WHERE type > 0) AS TotalIndexes";

        using (var cmd = new SqlCommand(countQuery, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                stats.TotalTables = reader.GetInt32(0);
                stats.TotalViews = reader.GetInt32(1);
                stats.TotalStoredProcedures = reader.GetInt32(2);
                stats.TotalFunctions = reader.GetInt32(3);
                stats.TotalTriggers = reader.GetInt32(4);
                stats.TotalIndexes = reader.GetInt32(5);
            }
        }

        // Get largest tables
        stats.LargestTables = await GetLargestTablesAsync(connection);

        // Get top queries
        stats.TopQueries = await GetTopQueriesAsync(connection);

        return stats;
    }

    private async Task<List<TableSizeInfo>> GetLargestTablesAsync(SqlConnection connection)
    {
        var tables = new List<TableSizeInfo>();

        var query = @"
            SELECT TOP 10
                s.name AS SchemaName,
                t.name AS TableName,
                p.rows AS [RowCount],
                CAST(ROUND((SUM(a.total_pages) * 8) / 1024.00, 2) AS DECIMAL(18,2)) AS TotalSpaceMB,
                CAST(ROUND((SUM(a.used_pages) * 8) / 1024.00, 2) AS DECIMAL(18,2)) AS DataSpaceMB,
                CAST(ROUND((SUM(CASE WHEN i.index_id > 1 THEN a.used_pages ELSE 0 END) * 8) / 1024.00, 2) AS DECIMAL(18,2)) AS IndexSpaceMB,
                CAST(ROUND((SUM(a.total_pages - a.used_pages) * 8) / 1024.00, 2) AS DECIMAL(18,2)) AS UnusedSpaceMB
            FROM sys.tables t
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            INNER JOIN sys.indexes i ON t.object_id = i.object_id
            INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
            INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
            WHERE t.is_ms_shipped = 0
            GROUP BY s.name, t.name, p.rows
            ORDER BY TotalSpaceMB DESC";

        using var cmd = new SqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(new TableSizeInfo
            {
                Schema = reader.GetString(0),
                TableName = reader.GetString(1),
                RowCount = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                TotalSpaceMB = reader.GetDecimal(3),
                DataSpaceMB = reader.GetDecimal(4),
                IndexSpaceMB = reader.GetDecimal(5),
                UnusedSpaceMB = reader.GetDecimal(6)
            });
        }

        return tables;
    }

    private async Task<List<QueryStatInfo>> GetTopQueriesAsync(SqlConnection connection)
    {
        var queries = new List<QueryStatInfo>();

        var query = @"
            SELECT TOP 10
                SUBSTRING(qt.text, (qs.statement_start_offset/2) + 1,
                    ((CASE qs.statement_end_offset
                        WHEN -1 THEN DATALENGTH(qt.text)
                        ELSE qs.statement_end_offset
                    END - qs.statement_start_offset)/2) + 1) AS QueryText,
                qs.execution_count AS ExecutionCount,
                CAST(qs.total_elapsed_time / 1000.0 AS DECIMAL(18,2)) AS TotalElapsedTimeMs,
                CAST((qs.total_elapsed_time / qs.execution_count) / 1000.0 AS DECIMAL(18,2)) AS AvgElapsedTimeMs,
                CAST(qs.total_logical_reads AS DECIMAL(18,2)) AS TotalLogicalReads,
                CAST(qs.total_logical_reads / qs.execution_count AS DECIMAL(18,2)) AS AvgLogicalReads,
                qs.last_execution_time AS LastExecutionTime
            FROM sys.dm_exec_query_stats qs
            CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
            WHERE qt.text NOT LIKE '%sys.dm_exec_query_stats%'
                AND qt.text NOT LIKE '%sp_spaceused%'
            ORDER BY qs.total_elapsed_time DESC";

        try
        {
            using var cmd = new SqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                queries.Add(new QueryStatInfo
                {
                    QueryText = reader.GetString(0).Trim(),
                    ExecutionCount = reader.GetInt64(1),
                    TotalElapsedTimeMs = reader.GetDecimal(2),
                    AvgElapsedTimeMs = reader.GetDecimal(3),
                    TotalLogicalReads = reader.GetDecimal(4),
                    AvgLogicalReads = reader.GetDecimal(5),
                    LastExecutionTime = reader.GetDateTime(6)
                });
            }
        }
        catch
        {
            // DMVs may not be accessible, return empty list
        }

        return queries;
    }
}

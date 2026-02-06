namespace DataDic.Models;

public class DatabaseMetadata
{
    public string DatabaseName { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public List<TableInfo> Tables { get; set; } = new();
    public List<UserInfo> Users { get; set; } = new();
    public List<JobInfo> Jobs { get; set; } = new();
    public List<StoredProcedureInfo> StoredProcedures { get; set; } = new();
    public List<FunctionInfo> Functions { get; set; } = new();
}

public class TableInfo
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // TABLE, VIEW
    public List<ColumnInfo> Columns { get; set; } = new();
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
    public List<IndexInfo> Indexes { get; set; } = new();
    public List<TriggerInfo> Triggers { get; set; } = new();
    public long RowCount { get; set; }
    public string? Definition { get; set; } // For views
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsIdentity { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
}

public class ForeignKeyInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReferencedSchema { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public List<(string Column, string ReferencedColumn)> ColumnMappings { get; set; } = new();
}

public class IndexInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public List<string> Columns { get; set; } = new();
}

public class UserInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // SQL_USER, WINDOWS_USER, etc.
    public List<string> Roles { get; set; } = new();
    public List<PermissionInfo> Permissions { get; set; } = new();
}

public class PermissionInfo
{
    public string ObjectName { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public string GrantType { get; set; } = string.Empty; // GRANT, DENY
}

public class JobInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<JobScheduleInfo> Schedules { get; set; } = new();
    public List<JobStepInfo> Steps { get; set; } = new();
}

public class JobScheduleInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string FrequencyType { get; set; } = string.Empty;
    public string FrequencyInterval { get; set; } = string.Empty;
    public string ActiveStartTime { get; set; } = string.Empty;
}

public class JobStepInfo
{
    public int StepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subsystem { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public class StoredProcedureInfo
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class FunctionInfo
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // SCALAR, TABLE_VALUED, etc.
    public string Definition { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class TriggerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // AFTER, INSTEAD OF
    public string Events { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public bool IsEnabled { get; set; }
    public string Definition { get; set; } = string.Empty;
}

public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsOutput { get; set; }
    public string? DefaultValue { get; set; }
}

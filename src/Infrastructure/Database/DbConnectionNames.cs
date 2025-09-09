namespace Monolith.Database;

public abstract class DbConnectionNames
{
    public const string SQLITE = "SqliteConnection"; // Sqlite

    public const string POSTGRESQL = "PostgreConnection"; // PostgreSql

    public const string DEFAULT = "DefaultConnection"; // MSSQL

    public const string IDENTITY = SQLITE;

    public const string CATALOG = SQLITE;
}
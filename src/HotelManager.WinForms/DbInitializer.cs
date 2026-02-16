using System.Data.SqlClient;
using System.Text;

namespace HotelManager.WinForms;

public static class DbInitializer
{
    public static void EnsureDatabase(string connectionString, string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Không tìm thấy file script: {scriptPath}");
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Missing database name in connection string.");
        }

        builder.InitialCatalog = "master";
        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        if (!DatabaseExists(connection, databaseName))
        {
            var script = File.ReadAllText(scriptPath, Encoding.UTF8);
            foreach (var batch in SplitBatches(script))
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                using var command = new SqlCommand(batch, connection);
                command.CommandTimeout = 60;
                command.ExecuteNonQuery();
            }
        }

        EnsureEmployeeProfileColumns(connection, databaseName);
        EnsureAttendanceTable(connection, databaseName);
        EnsureUserAccess(builder.ConnectionString, databaseName);
    }

    private static IEnumerable<string> SplitBatches(string script)
    {
        var sb = new StringBuilder();
        using var reader = new StringReader(script);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                yield return sb.ToString();
                sb.Clear();
                continue;
            }

            sb.AppendLine(line);
        }

        if (sb.Length > 0)
        {
            yield return sb.ToString();
        }
    }

    private static bool DatabaseExists(SqlConnection connection, string databaseName)
    {
        using var command = new SqlCommand("SELECT DB_ID(@dbName);", connection);
        command.Parameters.AddWithValue("@dbName", databaseName);
        var result = command.ExecuteScalar();
        return result != DBNull.Value && result is not null;
    }

    private static void EnsureEmployeeProfileColumns(SqlConnection connection, string databaseName)
    {
        using var command = new SqlCommand(
            $@"
USE [{databaseName}];

IF COL_LENGTH('dbo.Employees', 'AvatarPath') IS NULL
BEGIN
    ALTER TABLE dbo.Employees ADD AvatarPath NVARCHAR(300) NULL;
END

IF COL_LENGTH('dbo.Employees', 'BankName') IS NULL
BEGIN
    ALTER TABLE dbo.Employees ADD BankName NVARCHAR(100) NULL;
END

IF COL_LENGTH('dbo.Employees', 'BankAccountNumber') IS NULL
BEGIN
    ALTER TABLE dbo.Employees ADD BankAccountNumber NVARCHAR(50) NULL;
END

IF COL_LENGTH('dbo.Employees', 'BankAccountName') IS NULL
BEGIN
    ALTER TABLE dbo.Employees ADD BankAccountName NVARCHAR(120) NULL;
END",
            connection);

        command.ExecuteNonQuery();
    }

    private static void EnsureAttendanceTable(SqlConnection connection, string databaseName)
    {
        using var command = new SqlCommand(
            $@"
USE [{databaseName}];

IF OBJECT_ID('dbo.AttendanceRecords', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceRecords
    (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId INT NOT NULL,
        WorkDate DATE NOT NULL,
        CheckInTime DATETIME2 NULL,
        CheckOutTime DATETIME2 NULL,
        Note NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_AttendanceRecords_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId),
        CONSTRAINT UQ_AttendanceRecords_EmployeeDate UNIQUE (EmployeeId, WorkDate)
    );
END",
            connection);

        command.ExecuteNonQuery();
    }

    private static void EnsureUserAccess(string masterConnectionString, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = databaseName
        };

        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        var login = GetCurrentLogin(connection);
        if (string.IsNullOrWhiteSpace(login))
        {
            return;
        }

        var mappedUser = GetMappedUserForLogin(connection, login);
        var dbUserName = mappedUser;

        if (string.IsNullOrWhiteSpace(dbUserName))
        {
            var quotedLogin = QuoteIdentifier(login);
            var userExistsByName = CheckUserExists(connection, login);
            if (!userExistsByName)
            {
                ExecuteNonQuery(connection, $"CREATE USER {quotedLogin} FOR LOGIN {quotedLogin};");
            }
            else
            {
                ExecuteNonQuery(connection, $"ALTER USER {quotedLogin} WITH LOGIN = {quotedLogin};");
            }

            dbUserName = login;
        }

        if (!IsDbOwnerMember(connection, dbUserName!))
        {
            ExecuteNonQuery(connection, $"ALTER ROLE [db_owner] ADD MEMBER {QuoteIdentifier(dbUserName!)};");
        }
    }

    private static string GetCurrentLogin(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT SUSER_SNAME();", connection);
        return command.ExecuteScalar()?.ToString() ?? string.Empty;
    }

    private static bool CheckUserExists(SqlConnection connection, string login)
    {
        using var command = new SqlCommand("SELECT 1 FROM sys.database_principals WHERE name = @login;", connection);
        command.Parameters.AddWithValue("@login", login);
        var result = command.ExecuteScalar();
        return result != null;
    }

    private static string? GetMappedUserForLogin(SqlConnection connection, string login)
    {
        const string sql = @"
SELECT TOP 1 dp.name
FROM sys.database_principals dp
JOIN sys.server_principals sp ON sp.sid = dp.sid
WHERE sp.name = @login
  AND dp.type IN ('S', 'U', 'G');";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@login", login);
        return command.ExecuteScalar() as string;
    }

    private static bool IsDbOwnerMember(SqlConnection connection, string login)
    {
        const string sql = @"
SELECT 1
FROM sys.database_role_members rm
JOIN sys.database_principals role_principal ON role_principal.principal_id = rm.role_principal_id
JOIN sys.database_principals member_principal ON member_principal.principal_id = rm.member_principal_id
WHERE role_principal.name = N'db_owner'
  AND member_principal.name = @login;";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@login", login);
        var result = command.ExecuteScalar();
        return result is not null;
    }

    private static void ExecuteNonQuery(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private static string QuoteIdentifier(string identifier)
    {
        return $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
    }
}


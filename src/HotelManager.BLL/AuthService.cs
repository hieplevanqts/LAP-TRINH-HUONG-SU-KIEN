using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class AuthService
{
    public LoginResult Authenticate(string username, string password)
    {
        const string sql = @"
SELECT TOP 1
    a.AccountId,
    a.Username,
    a.PasswordHash,
    a.Salt,
    a.IsActive,
    e.EmployeeId,
    e.FullName,
    r.RoleName
FROM Accounts a
JOIN Employees e ON e.EmployeeId = a.EmployeeId
JOIN Roles r ON r.RoleId = a.RoleId
WHERE a.Username = @Username;";

        var data = Db.ExecuteQuery(sql, new SqlParameter("@Username", username.Trim()));
        if (data.Rows.Count == 0)
        {
            return LoginResult.Failed("Sai tên đăng nhập hoặc mật khẩu.");
        }

        var row = data.Rows[0];
        var isActive = row.Field<bool>("IsActive");
        if (!isActive)
        {
            return LoginResult.Failed("Tài khoản đang bị khóa.");
        }

        var storedHash = row.Field<string>("PasswordHash") ?? string.Empty;
        var storedSalt = row.Field<string>("Salt") ?? string.Empty;
        var inputHash = HashPassword(password, storedSalt);
        if (!string.Equals(storedHash, inputHash, StringComparison.Ordinal))
        {
            return LoginResult.Failed("Sai tên đăng nhập hoặc mật khẩu.");
        }

        return LoginResult.Success(
            row.Field<int>("AccountId"),
            row.Field<int>("EmployeeId"),
            row.Field<string>("FullName") ?? string.Empty,
            row.Field<string>("Username") ?? string.Empty,
            row.Field<string>("RoleName") ?? string.Empty
        );
    }

    public RegisterResult RegisterEmployee(RegisterEmployeeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return RegisterResult.Failed("Vui lòng nhập đầy đủ thông tin bắt buộc.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return RegisterResult.Failed("Mật khẩu xác nhận không khớp.");
        }

        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            if (UsernameExists(connection, transaction, request.Username.Trim()))
            {
                transaction.Rollback();
                return RegisterResult.Failed("Tên đăng nhập đã tồn tại.");
            }

            var roleId = EnsureDefaultRole(connection, transaction);
            var employeeId = InsertEmployee(connection, transaction, request);

            var salt = GenerateSalt();
            var hash = HashPassword(request.Password, salt);
            InsertAccount(connection, transaction, request.Username.Trim(), hash, salt, roleId, employeeId);

            transaction.Commit();
            return RegisterResult.Success(employeeId);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static bool UsernameExists(SqlConnection connection, SqlTransaction transaction, string username)
    {
        const string sql = "SELECT 1 FROM Accounts WHERE Username = @Username;";
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Username", username);
        return command.ExecuteScalar() is not null;
    }

    private static int EnsureDefaultRole(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = N'Staff')
BEGIN
    INSERT INTO Roles (RoleName, Description)
    VALUES (N'Staff', N'Nhân viên hệ thống');
END

SELECT TOP 1 RoleId FROM Roles WHERE RoleName = N'Staff';";

        using var command = new SqlCommand(sql, connection, transaction);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static int InsertEmployee(SqlConnection connection, SqlTransaction transaction, RegisterEmployeeRequest request)
    {
        const string sql = @"
INSERT INTO Employees (FullName, Phone, Email, Position, HireDate, Status)
VALUES (@FullName, @Phone, @Email, @Position, @HireDate, N'Active');
SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@FullName", request.FullName.Trim());
        command.Parameters.AddWithValue("@Phone", (object?)request.Phone?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@Email", (object?)request.Email?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@Position", (object?)request.Position?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("@HireDate", DateTime.Today);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void InsertAccount(
        SqlConnection connection,
        SqlTransaction transaction,
        string username,
        string passwordHash,
        string salt,
        int roleId,
        int employeeId)
    {
        const string sql = @"
INSERT INTO Accounts (Username, PasswordHash, Salt, RoleId, EmployeeId, IsActive, CreatedAt)
VALUES (@Username, @PasswordHash, @Salt, @RoleId, @EmployeeId, 1, SYSDATETIME());";

        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@Salt", salt);
        command.Parameters.AddWithValue("@RoleId", roleId);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        command.ExecuteNonQuery();
    }

    private static string GenerateSalt()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var payload = $"{salt}:{password}";
        var bytes = Encoding.UTF8.GetBytes(payload);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}

public sealed record RegisterEmployeeRequest(
    string FullName,
    string? Phone,
    string? Email,
    string? Position,
    string Username,
    string Password,
    string ConfirmPassword);

public sealed class RegisterResult
{
    public bool IsSuccess { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public int? EmployeeId { get; private init; }

    public static RegisterResult Success(int employeeId) =>
        new() { IsSuccess = true, Message = "Đăng ký thành công.", EmployeeId = employeeId };

    public static RegisterResult Failed(string message) =>
        new() { IsSuccess = false, Message = message };
}

public sealed class LoginResult
{
    public bool IsSuccess { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public int AccountId { get; private init; }
    public int EmployeeId { get; private init; }
    public string FullName { get; private init; } = string.Empty;
    public string Username { get; private init; } = string.Empty;
    public string RoleName { get; private init; } = string.Empty;

    public static LoginResult Success(int accountId, int employeeId, string fullName, string username, string roleName) =>
        new()
        {
            IsSuccess = true,
            Message = "Đăng nhập thành công.",
            AccountId = accountId,
            EmployeeId = employeeId,
            FullName = fullName,
            Username = username,
            RoleName = roleName
        };

    public static LoginResult Failed(string message) =>
        new()
        {
            IsSuccess = false,
            Message = message
        };
}


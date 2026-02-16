using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class EmployeeProfileService
{
    public EmployeeProfileDto? GetProfile(int employeeId)
    {
        const string sql = @"
SELECT TOP 1
    e.EmployeeId,
    e.FullName,
    e.Phone,
    e.Email,
    e.Position,
    e.AvatarPath,
    e.BankName,
    e.BankAccountNumber,
    e.BankAccountName,
    a.Username
FROM Employees e
LEFT JOIN Accounts a ON a.EmployeeId = e.EmployeeId
WHERE e.EmployeeId = @EmployeeId;";

        var table = Db.ExecuteQuery(sql, new SqlParameter("@EmployeeId", employeeId));
        if (table.Rows.Count == 0)
        {
            return null;
        }

        var row = table.Rows[0];
        return new EmployeeProfileDto
        {
            EmployeeId = row.Field<int>("EmployeeId"),
            FullName = row.Field<string>("FullName") ?? string.Empty,
            Phone = row.Field<string>("Phone"),
            Email = row.Field<string>("Email"),
            Position = row.Field<string>("Position"),
            AvatarPath = row.Field<string>("AvatarPath"),
            BankName = row.Field<string>("BankName"),
            BankAccountNumber = row.Field<string>("BankAccountNumber"),
            BankAccountName = row.Field<string>("BankAccountName"),
            Username = row.Field<string>("Username") ?? string.Empty
        };
    }

    public void UpdateProfile(EmployeeProfileDto profile)
    {
        const string sql = @"
UPDATE Employees
SET FullName = @FullName,
    Phone = @Phone,
    Email = @Email,
    Position = @Position,
    AvatarPath = @AvatarPath,
    BankName = @BankName,
    BankAccountNumber = @BankAccountNumber,
    BankAccountName = @BankAccountName
WHERE EmployeeId = @EmployeeId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@EmployeeId", profile.EmployeeId),
            new SqlParameter("@FullName", profile.FullName.Trim()),
            new SqlParameter("@Phone", (object?)profile.Phone?.Trim() ?? DBNull.Value),
            new SqlParameter("@Email", (object?)profile.Email?.Trim() ?? DBNull.Value),
            new SqlParameter("@Position", (object?)profile.Position?.Trim() ?? DBNull.Value),
            new SqlParameter("@AvatarPath", (object?)profile.AvatarPath?.Trim() ?? DBNull.Value),
            new SqlParameter("@BankName", (object?)profile.BankName?.Trim() ?? DBNull.Value),
            new SqlParameter("@BankAccountNumber", (object?)profile.BankAccountNumber?.Trim() ?? DBNull.Value),
            new SqlParameter("@BankAccountName", (object?)profile.BankAccountName?.Trim() ?? DBNull.Value)
        );
    }
}

public sealed class EmployeeProfileDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? AvatarPath { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string Username { get; set; } = string.Empty;
}

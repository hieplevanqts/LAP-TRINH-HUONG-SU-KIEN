using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class CustomerService
{
    public DataTable GetCustomers()
    {
        const string sql = @"
SELECT CustomerId, FullName, Phone, Email, IdNumber, Address, CreatedAt
FROM Customers
ORDER BY CustomerId DESC;";

        return Db.ExecuteQuery(sql);
    }

    public void AddCustomer(string fullName, string? phone, string? email, string? idNumber, string? address)
    {
        const string sql = @"
INSERT INTO Customers (FullName, Phone, Email, IdNumber, Address)
VALUES (@FullName, @Phone, @Email, @IdNumber, @Address);";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Phone", (object?)phone ?? DBNull.Value),
            new SqlParameter("@Email", (object?)email ?? DBNull.Value),
            new SqlParameter("@IdNumber", (object?)idNumber ?? DBNull.Value),
            new SqlParameter("@Address", (object?)address ?? DBNull.Value)
        );
    }

    public void UpdateCustomer(int customerId, string fullName, string? phone, string? email, string? idNumber, string? address)
    {
        const string sql = @"
UPDATE Customers
SET FullName = @FullName,
    Phone = @Phone,
    Email = @Email,
    IdNumber = @IdNumber,
    Address = @Address
WHERE CustomerId = @CustomerId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@CustomerId", customerId),
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Phone", (object?)phone ?? DBNull.Value),
            new SqlParameter("@Email", (object?)email ?? DBNull.Value),
            new SqlParameter("@IdNumber", (object?)idNumber ?? DBNull.Value),
            new SqlParameter("@Address", (object?)address ?? DBNull.Value)
        );
    }

    public void DeleteCustomer(int customerId)
    {
        const string sql = "DELETE FROM Customers WHERE CustomerId = @CustomerId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@CustomerId", customerId)
        );
    }
}

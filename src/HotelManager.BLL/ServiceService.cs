using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class ServiceService
{
    public DataTable GetServices(bool onlyActive = false)
    {
        const string sql = @"
SELECT ServiceId, ServiceName, UnitPrice, IsActive
FROM Services
WHERE (@OnlyActive = 0 OR IsActive = 1)
ORDER BY ServiceName;";

        return Db.ExecuteQuery(sql, new SqlParameter("@OnlyActive", onlyActive ? 1 : 0));
    }

    public void AddService(string serviceName, decimal unitPrice, bool isActive)
    {
        const string sql = @"
INSERT INTO Services (ServiceName, UnitPrice, IsActive)
VALUES (@ServiceName, @UnitPrice, @IsActive);";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@ServiceName", serviceName),
            new SqlParameter("@UnitPrice", unitPrice),
            new SqlParameter("@IsActive", isActive)
        );
    }

    public void UpdateService(int serviceId, string serviceName, decimal unitPrice, bool isActive)
    {
        const string sql = @"
UPDATE Services
SET ServiceName = @ServiceName,
    UnitPrice = @UnitPrice,
    IsActive = @IsActive
WHERE ServiceId = @ServiceId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@ServiceId", serviceId),
            new SqlParameter("@ServiceName", serviceName),
            new SqlParameter("@UnitPrice", unitPrice),
            new SqlParameter("@IsActive", isActive)
        );
    }

    public void DeleteService(int serviceId)
    {
        const string sql = "DELETE FROM Services WHERE ServiceId = @ServiceId;";
        Db.ExecuteNonQuery(sql, new SqlParameter("@ServiceId", serviceId));
    }
}

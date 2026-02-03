using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class RoomTypeService
{
    public DataTable GetRoomTypes()
    {
        const string sql = @"
SELECT RoomTypeId, TypeName, BasePrice, Capacity, Description
FROM RoomTypes
ORDER BY TypeName;";

        return Db.ExecuteQuery(sql);
    }

    public void AddRoomType(string typeName, decimal basePrice, int capacity, string? description)
    {
        const string sql = @"
INSERT INTO RoomTypes (TypeName, BasePrice, Capacity, Description)
VALUES (@TypeName, @BasePrice, @Capacity, @Description);";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@TypeName", typeName),
            new SqlParameter("@BasePrice", basePrice),
            new SqlParameter("@Capacity", capacity),
            new SqlParameter("@Description", (object?)description ?? DBNull.Value)
        );
    }

    public void UpdateRoomType(int roomTypeId, string typeName, decimal basePrice, int capacity, string? description)
    {
        const string sql = @"
UPDATE RoomTypes
SET TypeName = @TypeName,
    BasePrice = @BasePrice,
    Capacity = @Capacity,
    Description = @Description
WHERE RoomTypeId = @RoomTypeId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@RoomTypeId", roomTypeId),
            new SqlParameter("@TypeName", typeName),
            new SqlParameter("@BasePrice", basePrice),
            new SqlParameter("@Capacity", capacity),
            new SqlParameter("@Description", (object?)description ?? DBNull.Value)
        );
    }

    public void DeleteRoomType(int roomTypeId)
    {
        const string sql = "DELETE FROM RoomTypes WHERE RoomTypeId = @RoomTypeId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@RoomTypeId", roomTypeId)
        );
    }
}

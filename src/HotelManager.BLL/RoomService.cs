using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class RoomService
{
    public DataTable GetRooms()
    {
        const string sql = @"
SELECT r.RoomId,
       r.RoomNumber,
       r.RoomTypeId,
       rt.TypeName AS RoomType,
       r.Floor,
       r.Status
FROM Rooms r
JOIN RoomTypes rt ON rt.RoomTypeId = r.RoomTypeId
ORDER BY r.RoomNumber;";

        return Db.ExecuteQuery(sql);
    }

    public DataTable GetRoomTypes()
    {
        const string sql = @"
SELECT RoomTypeId, TypeName
FROM RoomTypes
ORDER BY TypeName;";

        return Db.ExecuteQuery(sql);
    }

    public void AddRoom(string roomNumber, int roomTypeId, int floor, string status)
    {
        const string sql = @"
INSERT INTO Rooms (RoomNumber, RoomTypeId, Floor, Status)
VALUES (@RoomNumber, @RoomTypeId, @Floor, @Status);";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@RoomNumber", roomNumber),
            new SqlParameter("@RoomTypeId", roomTypeId),
            new SqlParameter("@Floor", floor),
            new SqlParameter("@Status", status)
        );
    }

    public void UpdateRoom(int roomId, string roomNumber, int roomTypeId, int floor, string status)
    {
        const string sql = @"
UPDATE Rooms
SET RoomNumber = @RoomNumber,
    RoomTypeId = @RoomTypeId,
    Floor = @Floor,
    Status = @Status
WHERE RoomId = @RoomId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@RoomId", roomId),
            new SqlParameter("@RoomNumber", roomNumber),
            new SqlParameter("@RoomTypeId", roomTypeId),
            new SqlParameter("@Floor", floor),
            new SqlParameter("@Status", status)
        );
    }

    public void DeleteRoom(int roomId)
    {
        const string sql = "DELETE FROM Rooms WHERE RoomId = @RoomId;";

        Db.ExecuteNonQuery(
            sql,
            new SqlParameter("@RoomId", roomId)
        );
    }
}

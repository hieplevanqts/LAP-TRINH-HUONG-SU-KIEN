using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class BookingService
{
    public DataTable GetAllBookings()
    {
        const string sql = @"
SELECT b.BookingId, c.FullName, b.CheckInDate, b.CheckOutDate, b.Status
FROM Bookings b
JOIN Customers c ON c.CustomerId = b.CustomerId
ORDER BY b.BookingId DESC";

        return Db.ExecuteQuery(sql);
    }

    public DataTable GetBookingHistory(DateTime fromDate, DateTime toDate, string? status)
    {
        const string sql = @"
SELECT b.BookingId,
       c.FullName,
       b.CheckInDate,
       b.CheckOutDate,
       b.Adults,
       b.Children,
       b.Status,
       b.CreatedAt
FROM Bookings b
JOIN Customers c ON c.CustomerId = b.CustomerId
WHERE b.CreatedAt >= @FromDate AND b.CreatedAt < DATEADD(day, 1, @ToDate)
  AND (@Status IS NULL OR @Status = N'All' OR b.Status = @Status)
ORDER BY b.CreatedAt DESC, b.BookingId DESC;";

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@FromDate", fromDate.Date),
            new SqlParameter("@ToDate", toDate.Date),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value)
        );
    }

    public DataTable GetCustomers()
    {
        const string sql = @"
SELECT CustomerId, FullName, Phone
FROM Customers
ORDER BY FullName;";

        return Db.ExecuteQuery(sql);
    }

    public DataTable GetRooms(bool onlyAvailable)
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
WHERE (@OnlyAvailable = 0 OR r.Status = N'Available')
ORDER BY r.RoomNumber;";

        return Db.ExecuteQuery(sql, new SqlParameter("@OnlyAvailable", onlyAvailable ? 1 : 0));
    }

    public int CreateBooking(int customerId, DateTime checkIn, DateTime checkOut, int adults, int children, int? createdByAccountId)
    {
        const string sql = @"
INSERT INTO Bookings (CustomerId, CheckInDate, CheckOutDate, Adults, Children, Status, CreatedByAccountId)
VALUES (@CustomerId, @CheckInDate, @CheckOutDate, @Adults, @Children, N'Pending', @CreatedBy)
SELECT SCOPE_IDENTITY();";

        var result = Db.ExecuteScalar(
            sql,
            new SqlParameter("@CustomerId", customerId),
            new SqlParameter("@CheckInDate", checkIn.Date),
            new SqlParameter("@CheckOutDate", checkOut.Date),
            new SqlParameter("@Adults", adults),
            new SqlParameter("@Children", children),
            new SqlParameter("@CreatedBy", (object?)createdByAccountId ?? DBNull.Value)
        );

        return Convert.ToInt32(result);
    }

    public int CreateBookingWithRooms(int customerId, DateTime checkIn, DateTime checkOut, int adults, int children, int? createdByAccountId, IReadOnlyList<int> roomIds)
    {
        return CreateBookingWithRoomsAndServices(customerId, checkIn, checkOut, adults, children, createdByAccountId, roomIds, Array.Empty<ServiceSelection>());
    }

    public int CreateBookingWithRoomsAndServices(
        int customerId,
        DateTime checkIn,
        DateTime checkOut,
        int adults,
        int children,
        int? createdByAccountId,
        IReadOnlyList<int> roomIds,
        IReadOnlyList<ServiceSelection> services)
    {
        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertBookingSql = @"
INSERT INTO Bookings (CustomerId, CheckInDate, CheckOutDate, Adults, Children, Status, CreatedByAccountId)
VALUES (@CustomerId, @CheckInDate, @CheckOutDate, @Adults, @Children, N'Pending', @CreatedBy)
SELECT SCOPE_IDENTITY();";

            using var insertBookingCmd = new SqlCommand(insertBookingSql, connection, transaction);
            insertBookingCmd.Parameters.AddWithValue("@CustomerId", customerId);
            insertBookingCmd.Parameters.AddWithValue("@CheckInDate", checkIn.Date);
            insertBookingCmd.Parameters.AddWithValue("@CheckOutDate", checkOut.Date);
            insertBookingCmd.Parameters.AddWithValue("@Adults", adults);
            insertBookingCmd.Parameters.AddWithValue("@Children", children);
            insertBookingCmd.Parameters.AddWithValue("@CreatedBy", (object?)createdByAccountId ?? DBNull.Value);

            var bookingId = Convert.ToInt32(insertBookingCmd.ExecuteScalar());

            const string insertRoomSql = @"
INSERT INTO BookingRooms (BookingId, RoomId, PricePerNight, CheckInDate, CheckOutDate, Status)
SELECT @BookingId, r.RoomId, rt.BasePrice, @CheckInDate, @CheckOutDate, N'Reserved'
FROM Rooms r
JOIN RoomTypes rt ON rt.RoomTypeId = r.RoomTypeId
WHERE r.RoomId = @RoomId;";

            const string updateRoomSql = @"
UPDATE Rooms
SET Status = N'Occupied'
WHERE RoomId = @RoomId;";

            foreach (var roomId in roomIds)
            {
                using var insertRoomCmd = new SqlCommand(insertRoomSql, connection, transaction);
                insertRoomCmd.Parameters.AddWithValue("@BookingId", bookingId);
                insertRoomCmd.Parameters.AddWithValue("@RoomId", roomId);
                insertRoomCmd.Parameters.AddWithValue("@CheckInDate", checkIn.Date);
                insertRoomCmd.Parameters.AddWithValue("@CheckOutDate", checkOut.Date);
                insertRoomCmd.ExecuteNonQuery();

                using var updateRoomCmd = new SqlCommand(updateRoomSql, connection, transaction);
                updateRoomCmd.Parameters.AddWithValue("@RoomId", roomId);
                updateRoomCmd.ExecuteNonQuery();
            }

            const string insertServiceSql = @"
INSERT INTO ServiceUsages (BookingId, ServiceId, Quantity, UnitPrice)
SELECT @BookingId, s.ServiceId, @Quantity, s.UnitPrice
FROM Services s
WHERE s.ServiceId = @ServiceId;";

            foreach (var service in services)
            {
                using var insertServiceCmd = new SqlCommand(insertServiceSql, connection, transaction);
                insertServiceCmd.Parameters.AddWithValue("@BookingId", bookingId);
                insertServiceCmd.Parameters.AddWithValue("@ServiceId", service.ServiceId);
                insertServiceCmd.Parameters.AddWithValue("@Quantity", service.Quantity);
                insertServiceCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return bookingId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

public sealed record ServiceSelection(int ServiceId, int Quantity);

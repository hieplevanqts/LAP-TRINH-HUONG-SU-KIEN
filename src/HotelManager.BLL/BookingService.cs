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
}

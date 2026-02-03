using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class PaymentService
{
    public DataTable GetBookingsForPayment()
    {
        const string sql = @"
SELECT b.BookingId,
       c.FullName,
       b.CheckInDate,
       b.CheckOutDate,
       b.Status,
       ISNULL(r.RoomTotal, 0) + ISNULL(s.ServiceTotal, 0) AS Subtotal
FROM Bookings b
JOIN Customers c ON c.CustomerId = b.CustomerId
LEFT JOIN (
    SELECT BookingId, SUM(PricePerNight * DATEDIFF(day, CheckInDate, CheckOutDate)) AS RoomTotal
    FROM BookingRooms
    GROUP BY BookingId
) r ON r.BookingId = b.BookingId
LEFT JOIN (
    SELECT BookingId, SUM(Quantity * UnitPrice) AS ServiceTotal
    FROM ServiceUsages
    GROUP BY BookingId
) s ON s.BookingId = b.BookingId
WHERE NOT EXISTS (
    SELECT 1
    FROM Invoices i
    WHERE i.BookingId = b.BookingId AND i.Status = N'Paid'
)
ORDER BY b.BookingId DESC;";

        return Db.ExecuteQuery(sql);
    }

    public void PayBooking(int bookingId, decimal discount, decimal tax, string method, string? note)
    {
        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string subtotalSql = @"
SELECT ISNULL(r.RoomTotal, 0) + ISNULL(s.ServiceTotal, 0)
FROM (
    SELECT BookingId, SUM(PricePerNight * DATEDIFF(day, CheckInDate, CheckOutDate)) AS RoomTotal
    FROM BookingRooms
    WHERE BookingId = @BookingId
    GROUP BY BookingId
) r
FULL JOIN (
    SELECT BookingId, SUM(Quantity * UnitPrice) AS ServiceTotal
    FROM ServiceUsages
    WHERE BookingId = @BookingId
    GROUP BY BookingId
) s ON s.BookingId = r.BookingId;";

            using var subtotalCmd = new SqlCommand(subtotalSql, connection, transaction);
            subtotalCmd.Parameters.AddWithValue("@BookingId", bookingId);
            var subtotal = Convert.ToDecimal(subtotalCmd.ExecuteScalar());

            var total = subtotal - discount + tax;
            if (total < 0)
            {
                total = 0;
            }

            const string invoiceSql = @"
INSERT INTO Invoices (BookingId, Subtotal, Discount, Tax, Total, Status, PaidAt, PaymentMethod)
VALUES (@BookingId, @Subtotal, @Discount, @Tax, @Total, N'Paid', SYSDATETIME(), @Method);
SELECT SCOPE_IDENTITY();";

            using var invoiceCmd = new SqlCommand(invoiceSql, connection, transaction);
            invoiceCmd.Parameters.AddWithValue("@BookingId", bookingId);
            invoiceCmd.Parameters.AddWithValue("@Subtotal", subtotal);
            invoiceCmd.Parameters.AddWithValue("@Discount", discount);
            invoiceCmd.Parameters.AddWithValue("@Tax", tax);
            invoiceCmd.Parameters.AddWithValue("@Total", total);
            invoiceCmd.Parameters.AddWithValue("@Method", method);
            var invoiceId = Convert.ToInt32(invoiceCmd.ExecuteScalar());

            const string paymentSql = @"
INSERT INTO Payments (InvoiceId, Amount, Method, PaidAt, Note)
VALUES (@InvoiceId, @Amount, @Method, SYSDATETIME(), @Note);";

            using var paymentCmd = new SqlCommand(paymentSql, connection, transaction);
            paymentCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
            paymentCmd.Parameters.AddWithValue("@Amount", total);
            paymentCmd.Parameters.AddWithValue("@Method", method);
            paymentCmd.Parameters.AddWithValue("@Note", (object?)note ?? DBNull.Value);
            paymentCmd.ExecuteNonQuery();

            const string updateBookingSql = "UPDATE Bookings SET Status = N'Paid' WHERE BookingId = @BookingId;";
            using var updateBookingCmd = new SqlCommand(updateBookingSql, connection, transaction);
            updateBookingCmd.Parameters.AddWithValue("@BookingId", bookingId);
            updateBookingCmd.ExecuteNonQuery();

            const string updateRoomsSql = @"
UPDATE Rooms
SET Status = N'Available'
WHERE RoomId IN (SELECT RoomId FROM BookingRooms WHERE BookingId = @BookingId);";

            using var updateRoomsCmd = new SqlCommand(updateRoomsSql, connection, transaction);
            updateRoomsCmd.Parameters.AddWithValue("@BookingId", bookingId);
            updateRoomsCmd.ExecuteNonQuery();

            const string updateBookingRoomsSql = @"
UPDATE BookingRooms
SET Status = N'CheckedOut'
WHERE BookingId = @BookingId;";

            using var updateBookingRoomsCmd = new SqlCommand(updateBookingRoomsSql, connection, transaction);
            updateBookingRoomsCmd.Parameters.AddWithValue("@BookingId", bookingId);
            updateBookingRoomsCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

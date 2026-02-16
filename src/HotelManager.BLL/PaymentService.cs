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

    public DataTable GetBookingServiceUsages(int bookingId)
    {
        const string sql = @"
SELECT su.ServiceUsageId,
       su.ServiceId,
       s.ServiceName,
       su.Quantity,
       su.UnitPrice,
       su.Quantity * su.UnitPrice AS Amount
FROM ServiceUsages su
JOIN Services s ON s.ServiceId = su.ServiceId
WHERE su.BookingId = @BookingId
ORDER BY su.ServiceUsageId DESC;";

        return Db.ExecuteQuery(sql, new SqlParameter("@BookingId", bookingId));
    }

    public void AddServiceUsage(int bookingId, int serviceId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0.");
        }

        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string validateBookingSql = @"
SELECT COUNT(1)
FROM Bookings
WHERE BookingId = @BookingId
  AND Status = N'Pending';";

            using var validateBookingCmd = new SqlCommand(validateBookingSql, connection, transaction);
            validateBookingCmd.Parameters.AddWithValue("@BookingId", bookingId);
            var bookingExists = Convert.ToInt32(validateBookingCmd.ExecuteScalar()) > 0;
            if (!bookingExists)
            {
                throw new InvalidOperationException("Chỉ có thể thêm dịch vụ cho đặt phòng đang chờ thanh toán.");
            }

            const string validateServiceSql = @"
SELECT COUNT(1)
FROM Services
WHERE ServiceId = @ServiceId
  AND IsActive = 1;";

            using var validateServiceCmd = new SqlCommand(validateServiceSql, connection, transaction);
            validateServiceCmd.Parameters.AddWithValue("@ServiceId", serviceId);
            var serviceExists = Convert.ToInt32(validateServiceCmd.ExecuteScalar()) > 0;
            if (!serviceExists)
            {
                throw new InvalidOperationException("Dịch vụ không tồn tại hoặc đã ngưng áp dụng.");
            }

            const string insertSql = @"
INSERT INTO ServiceUsages (BookingId, ServiceId, Quantity, UnitPrice)
SELECT @BookingId, s.ServiceId, @Quantity, s.UnitPrice
FROM Services s
WHERE s.ServiceId = @ServiceId;";

            using var insertCmd = new SqlCommand(insertSql, connection, transaction);
            insertCmd.Parameters.AddWithValue("@BookingId", bookingId);
            insertCmd.Parameters.AddWithValue("@ServiceId", serviceId);
            insertCmd.Parameters.AddWithValue("@Quantity", quantity);
            insertCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

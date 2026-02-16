using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class PaymentService
{
    public DataTable GetBookingsForPayment(bool useActualCheckoutPricing)
    {
        const string sql = @"
SELECT b.BookingId,
       c.FullName,
       b.CheckInDate,
       b.CheckOutDate,
       b.Status,
       e.EmployeeId AS CreatedByEmployeeId,
       e.FullName AS CreatedByEmployeeName,
       ISNULL(r.RoomTotal, 0) + ISNULL(s.ServiceTotal, 0) AS Subtotal
FROM Bookings b
JOIN Customers c ON c.CustomerId = b.CustomerId
LEFT JOIN Accounts a ON a.AccountId = b.CreatedByAccountId
LEFT JOIN Employees e ON e.EmployeeId = a.EmployeeId
LEFT JOIN (
    SELECT BookingId,
           SUM(
               CASE
                   WHEN @CheckoutAt > CAST(CheckOutDate AS datetime2) THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   WHEN @UseActualCheckoutPricing = 1 THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   ELSE PricePerNight * DATEDIFF(day, CheckInDate, CheckOutDate)
               END
           ) AS RoomTotal
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

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@UseActualCheckoutPricing", useActualCheckoutPricing ? 1 : 0),
            new SqlParameter("@CheckoutAt", DateTime.Now)
        );
    }

    public void PayBooking(int bookingId, decimal discount, decimal tax, string method, string? note, bool useActualCheckoutPricing, int? paidByAccountId)
    {
        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string subtotalSql = @"
SELECT ISNULL(r.RoomTotal, 0) + ISNULL(s.ServiceTotal, 0)
FROM (
    SELECT BookingId,
           SUM(
               CASE
                   WHEN @CheckoutAt > CAST(CheckOutDate AS datetime2) THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   WHEN @UseActualCheckoutPricing = 1 THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   ELSE PricePerNight * DATEDIFF(day, CheckInDate, CheckOutDate)
               END
           ) AS RoomTotal
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
            subtotalCmd.Parameters.AddWithValue("@UseActualCheckoutPricing", useActualCheckoutPricing ? 1 : 0);
            subtotalCmd.Parameters.AddWithValue("@CheckoutAt", DateTime.Now);
            var subtotal = Convert.ToDecimal(subtotalCmd.ExecuteScalar());

            var total = subtotal - discount + tax;
            if (total < 0)
            {
                total = 0;
            }

            const string invoiceSql = @"
INSERT INTO Invoices (BookingId, Subtotal, Discount, Tax, Total, Status, PaidAt, PaymentMethod, CreatedByAccountId)
VALUES (@BookingId, @Subtotal, @Discount, @Tax, @Total, N'Paid', SYSDATETIME(), @Method, @CreatedByAccountId);
SELECT SCOPE_IDENTITY();";

            using var invoiceCmd = new SqlCommand(invoiceSql, connection, transaction);
            invoiceCmd.Parameters.AddWithValue("@BookingId", bookingId);
            invoiceCmd.Parameters.AddWithValue("@Subtotal", subtotal);
            invoiceCmd.Parameters.AddWithValue("@Discount", discount);
            invoiceCmd.Parameters.AddWithValue("@Tax", tax);
            invoiceCmd.Parameters.AddWithValue("@Total", total);
            invoiceCmd.Parameters.AddWithValue("@Method", method);
            invoiceCmd.Parameters.AddWithValue("@CreatedByAccountId", (object?)paidByAccountId ?? DBNull.Value);
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
       COALESCE(su.CustomServiceName, s.ServiceName) AS ServiceName,
       su.Quantity,
       su.UnitPrice,
       su.Quantity * su.UnitPrice AS Amount
FROM ServiceUsages su
JOIN Services s ON s.ServiceId = su.ServiceId
WHERE su.BookingId = @BookingId
ORDER BY su.ServiceUsageId DESC;";

        return Db.ExecuteQuery(sql, new SqlParameter("@BookingId", bookingId));
    }

    public DataRow? GetPendingBillHeader(int bookingId, bool useActualCheckoutPricing)
    {
        const string sql = @"
SELECT b.BookingId,
       c.FullName,
       c.Phone,
       c.Email,
       b.CheckInDate,
       b.CheckOutDate,
       e.EmployeeId AS CreatedByEmployeeId,
       e.FullName AS CreatedByEmployeeName,
       ISNULL(r.RoomTotal, 0) AS RoomTotal,
       ISNULL(s.ServiceTotal, 0) AS ServiceTotal,
       ISNULL(r.RoomTotal, 0) + ISNULL(s.ServiceTotal, 0) AS Subtotal
FROM Bookings b
JOIN Customers c ON c.CustomerId = b.CustomerId
LEFT JOIN Accounts a ON a.AccountId = b.CreatedByAccountId
LEFT JOIN Employees e ON e.EmployeeId = a.EmployeeId
LEFT JOIN (
    SELECT BookingId,
           SUM(
               CASE
                   WHEN @CheckoutAt > CAST(CheckOutDate AS datetime2) THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   WHEN @UseActualCheckoutPricing = 1 THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   ELSE PricePerNight * DATEDIFF(day, CheckInDate, CheckOutDate)
               END
           ) AS RoomTotal
    FROM BookingRooms
    WHERE BookingId = @BookingId
    GROUP BY BookingId
) r ON r.BookingId = b.BookingId
LEFT JOIN (
    SELECT BookingId, SUM(Quantity * UnitPrice) AS ServiceTotal
    FROM ServiceUsages
    WHERE BookingId = @BookingId
    GROUP BY BookingId
) s ON s.BookingId = b.BookingId
WHERE b.BookingId = @BookingId;";

        var table = Db.ExecuteQuery(
            sql,
            new SqlParameter("@BookingId", bookingId),
            new SqlParameter("@UseActualCheckoutPricing", useActualCheckoutPricing ? 1 : 0),
            new SqlParameter("@CheckoutAt", DateTime.Now)
        );

        return table.Rows.Count > 0 ? table.Rows[0] : null;
    }

    public DataTable GetPendingBillRoomLines(int bookingId, bool useActualCheckoutPricing)
    {
        const string sql = @"
SELECT r.RoomNumber,
       rt.TypeName,
       br.PricePerNight,
       CAST(
           CASE
               WHEN @CheckoutAt > CAST(br.CheckOutDate AS datetime2) THEN
                   CASE
                       WHEN DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                       ELSE CAST(DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0
                   END
               WHEN @UseActualCheckoutPricing = 1 THEN
                   CASE
                       WHEN DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                       ELSE CAST(DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0
                   END
               ELSE CAST(DATEDIFF(day, br.CheckInDate, br.CheckOutDate) AS decimal(18,6))
           END
       AS decimal(18,2)) AS ChargeDays,
       CAST(
           CASE
               WHEN @CheckoutAt > CAST(br.CheckOutDate AS datetime2) THEN
                   CASE
                       WHEN DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                       ELSE br.PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                   END
               WHEN @UseActualCheckoutPricing = 1 THEN
                   CASE
                       WHEN DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                       ELSE br.PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(br.CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                   END
               ELSE br.PricePerNight * DATEDIFF(day, br.CheckInDate, br.CheckOutDate)
           END
       AS decimal(18,2)) AS LineTotal
FROM BookingRooms br
JOIN Rooms r ON r.RoomId = br.RoomId
JOIN RoomTypes rt ON rt.RoomTypeId = r.RoomTypeId
WHERE br.BookingId = @BookingId
ORDER BY r.RoomNumber;";

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@BookingId", bookingId),
            new SqlParameter("@UseActualCheckoutPricing", useActualCheckoutPricing ? 1 : 0),
            new SqlParameter("@CheckoutAt", DateTime.Now)
        );
    }

    public void AddServiceUsage(int bookingId, int serviceId, int quantity, int? addedByAccountId)
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
  AND IsActive = 1
  AND ISNULL(IsCustom, 0) = 0;";

            using var validateServiceCmd = new SqlCommand(validateServiceSql, connection, transaction);
            validateServiceCmd.Parameters.AddWithValue("@ServiceId", serviceId);
            var serviceExists = Convert.ToInt32(validateServiceCmd.ExecuteScalar()) > 0;
            if (!serviceExists)
            {
                throw new InvalidOperationException("Dịch vụ không tồn tại hoặc đã ngưng áp dụng.");
            }

            const string insertSql = @"
INSERT INTO ServiceUsages (BookingId, ServiceId, Quantity, UnitPrice, AddedByAccountId)
SELECT @BookingId, s.ServiceId, @Quantity, s.UnitPrice, @AddedByAccountId
FROM Services s
WHERE s.ServiceId = @ServiceId;";

            using var insertCmd = new SqlCommand(insertSql, connection, transaction);
            insertCmd.Parameters.AddWithValue("@BookingId", bookingId);
            insertCmd.Parameters.AddWithValue("@ServiceId", serviceId);
            insertCmd.Parameters.AddWithValue("@Quantity", quantity);
            insertCmd.Parameters.AddWithValue("@AddedByAccountId", (object?)addedByAccountId ?? DBNull.Value);
            insertCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void AddCustomServiceUsage(int bookingId, string serviceName, decimal unitPrice, int quantity, int? addedByAccountId)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name is required.", nameof(serviceName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0.");
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than 0.");
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

            const string getMetaServiceSql = @"
SELECT TOP 1 ServiceId
FROM Services
WHERE ServiceName = N'__BOOKING_CUSTOM_SERVICE__'
  AND ISNULL(IsCustom, 0) = 1;";

            int metaServiceId;
            using (var getMetaServiceCmd = new SqlCommand(getMetaServiceSql, connection, transaction))
            {
                var scalar = getMetaServiceCmd.ExecuteScalar();
                if (scalar is null || scalar == DBNull.Value)
                {
                    const string insertMetaServiceSql = @"
INSERT INTO Services (ServiceName, Unit, UnitPrice, IsActive, IsCustom, BookingScopeId)
VALUES (N'__BOOKING_CUSTOM_SERVICE__', N'Item', 0, 0, 1, NULL);
SELECT CAST(SCOPE_IDENTITY() AS int);";

                    using var insertMetaServiceCmd = new SqlCommand(insertMetaServiceSql, connection, transaction);
                    metaServiceId = Convert.ToInt32(insertMetaServiceCmd.ExecuteScalar());
                }
                else
                {
                    metaServiceId = Convert.ToInt32(scalar);
                }
            }

            const string insertUsageSql = @"
INSERT INTO ServiceUsages (BookingId, ServiceId, CustomServiceName, Quantity, UnitPrice, AddedByAccountId)
VALUES (@BookingId, @ServiceId, @CustomServiceName, @Quantity, @UnitPrice, @AddedByAccountId);";

            using var insertUsageCmd = new SqlCommand(insertUsageSql, connection, transaction);
            insertUsageCmd.Parameters.AddWithValue("@BookingId", bookingId);
            insertUsageCmd.Parameters.AddWithValue("@ServiceId", metaServiceId);
            insertUsageCmd.Parameters.AddWithValue("@CustomServiceName", serviceName.Trim());
            insertUsageCmd.Parameters.AddWithValue("@Quantity", quantity);
            insertUsageCmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
            insertUsageCmd.Parameters.AddWithValue("@AddedByAccountId", (object?)addedByAccountId ?? DBNull.Value);
            insertUsageCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void UpdateServiceUsageQuantity(int serviceUsageId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0.");
        }

        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sql = @"
UPDATE su
SET su.Quantity = @Quantity
FROM ServiceUsages su
JOIN Bookings b ON b.BookingId = su.BookingId
WHERE su.ServiceUsageId = @ServiceUsageId
  AND b.Status = N'Pending';";

            using var cmd = new SqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@ServiceUsageId", serviceUsageId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);

            var affected = cmd.ExecuteNonQuery();
            if (affected <= 0)
            {
                throw new InvalidOperationException("Không thể cập nhật dịch vụ cho đặt phòng đã thanh toán.");
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void DeleteServiceUsage(int serviceUsageId)
    {
        using var connection = Db.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sql = @"
DELETE su
FROM ServiceUsages su
JOIN Bookings b ON b.BookingId = su.BookingId
WHERE su.ServiceUsageId = @ServiceUsageId
  AND b.Status = N'Pending';";

            using var cmd = new SqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@ServiceUsageId", serviceUsageId);

            var affected = cmd.ExecuteNonQuery();
            if (affected <= 0)
            {
                throw new InvalidOperationException("Không thể xóa dịch vụ cho đặt phòng đã thanh toán.");
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public decimal GetBookingSubtotal(int bookingId, bool useActualCheckoutPricing)
    {
        const string sql = @"
SELECT ISNULL(r.RoomTotal, 0) + ISNULL(s.ServiceTotal, 0)
FROM (
    SELECT BookingId,
           SUM(
               CASE
                   WHEN @CheckoutAt > CAST(CheckOutDate AS datetime2) THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   WHEN @UseActualCheckoutPricing = 1 THEN
                       CASE
                           WHEN DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) <= 0 THEN 0
                           ELSE PricePerNight * (CAST(DATEDIFF_BIG(second, CAST(CheckInDate AS datetime2), @CheckoutAt) AS decimal(18,6)) / 86400.0)
                       END
                   ELSE PricePerNight * DATEDIFF(day, CheckInDate, CheckOutDate)
               END
           ) AS RoomTotal
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

        var result = Db.ExecuteScalar(
            sql,
            new SqlParameter("@BookingId", bookingId),
            new SqlParameter("@UseActualCheckoutPricing", useActualCheckoutPricing ? 1 : 0),
            new SqlParameter("@CheckoutAt", DateTime.Now)
        );
        return Convert.ToDecimal(result ?? 0m);
    }
}

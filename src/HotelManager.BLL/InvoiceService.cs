using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class InvoiceService
{
    public DataTable GetInvoices(DateTime fromDate, DateTime toDate)
    {
        const string sql = @"
SELECT i.InvoiceId,
       i.BookingId,
       c.FullName,
       i.InvoiceDate,
       i.Subtotal,
       i.Discount,
       i.Tax,
       i.Total,
       i.Status,
       i.PaymentMethod,
       i.PaidAt,
       ep.EmployeeId AS PaidByEmployeeId,
       ep.FullName AS PaidByEmployeeName
FROM Invoices i
JOIN Bookings b ON b.BookingId = i.BookingId
JOIN Customers c ON c.CustomerId = b.CustomerId
LEFT JOIN Accounts ap ON ap.AccountId = i.CreatedByAccountId
LEFT JOIN Employees ep ON ep.EmployeeId = ap.EmployeeId
WHERE i.InvoiceDate >= @FromDate AND i.InvoiceDate < DATEADD(day, 1, @ToDate)
ORDER BY i.InvoiceDate DESC, i.InvoiceId DESC;";

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@FromDate", fromDate.Date),
            new SqlParameter("@ToDate", toDate.Date)
        );
    }

    public DataRow? GetInvoiceHeader(int invoiceId)
    {
        const string sql = @"
SELECT i.InvoiceId,
       i.BookingId,
       c.FullName,
       c.Phone,
       c.Email,
       b.CheckInDate,
       b.CheckOutDate,
       eb.EmployeeId AS BookedByEmployeeId,
       eb.FullName AS BookedByEmployeeName,
       i.InvoiceDate,
       i.Subtotal,
       i.Discount,
       i.Tax,
       i.Total,
       i.Status,
       i.PaymentMethod,
       i.PaidAt,
       ep.EmployeeId AS PaidByEmployeeId,
       ep.FullName AS PaidByEmployeeName
FROM Invoices i
JOIN Bookings b ON b.BookingId = i.BookingId
JOIN Customers c ON c.CustomerId = b.CustomerId
LEFT JOIN Accounts ab ON ab.AccountId = b.CreatedByAccountId
LEFT JOIN Employees eb ON eb.EmployeeId = ab.EmployeeId
LEFT JOIN Accounts ap ON ap.AccountId = i.CreatedByAccountId
LEFT JOIN Employees ep ON ep.EmployeeId = ap.EmployeeId
WHERE i.InvoiceId = @InvoiceId;";

        var table = Db.ExecuteQuery(sql, new SqlParameter("@InvoiceId", invoiceId));
        return table.Rows.Count > 0 ? table.Rows[0] : null;
    }

    public DataTable GetInvoiceRoomLines(int invoiceId)
    {
        const string sql = @"
SELECT r.RoomNumber,
       rt.TypeName,
       br.PricePerNight,
       DATEDIFF(day, br.CheckInDate, br.CheckOutDate) AS Nights,
       (br.PricePerNight * DATEDIFF(day, br.CheckInDate, br.CheckOutDate)) AS LineTotal
FROM BookingRooms br
JOIN Rooms r ON r.RoomId = br.RoomId
JOIN RoomTypes rt ON rt.RoomTypeId = r.RoomTypeId
WHERE br.BookingId = (SELECT BookingId FROM Invoices WHERE InvoiceId = @InvoiceId)
ORDER BY r.RoomNumber;";

        return Db.ExecuteQuery(sql, new SqlParameter("@InvoiceId", invoiceId));
    }

    public DataTable GetInvoiceServiceLines(int invoiceId)
    {
        const string sql = @"
SELECT COALESCE(su.CustomServiceName, s.ServiceName) AS ServiceName,
       su.Quantity,
       su.UnitPrice,
       su.Quantity * su.UnitPrice AS Amount
FROM ServiceUsages su
JOIN Services s ON s.ServiceId = su.ServiceId
WHERE su.BookingId = (SELECT BookingId FROM Invoices WHERE InvoiceId = @InvoiceId)
ORDER BY su.ServiceUsageId;";

        return Db.ExecuteQuery(sql, new SqlParameter("@InvoiceId", invoiceId));
    }
}

using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class ReportService
{
    public DataTable GetDailyRevenue(DateTime fromDate, DateTime toDate)
    {
        const string sql = @"
SELECT RevenueDate, TotalRevenue
FROM vw_DailyRevenue
WHERE RevenueDate >= @FromDate AND RevenueDate <= @ToDate
ORDER BY RevenueDate;";

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@FromDate", fromDate.Date),
            new SqlParameter("@ToDate", toDate.Date)
        );
    }

    public DataTable GetMonthlyRevenue(DateTime fromDate, DateTime toDate)
    {
        const string sql = @"
SELECT RevenueMonth, TotalRevenue
FROM vw_MonthlyRevenue
WHERE RevenueMonth >= DATEFROMPARTS(YEAR(@FromDate), MONTH(@FromDate), 1)
  AND RevenueMonth <= DATEFROMPARTS(YEAR(@ToDate), MONTH(@ToDate), 1)
ORDER BY RevenueMonth;";

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@FromDate", fromDate.Date),
            new SqlParameter("@ToDate", toDate.Date)
        );
    }
}

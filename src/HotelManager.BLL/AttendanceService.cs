using System.Data;
using System.Data.SqlClient;
using HotelManager.DAL;

namespace HotelManager.BLL;

public sealed class AttendanceService
{
    public DataTable GetMonthlyAttendance(int employeeId, int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        const string sql = @"
;WITH Days AS
(
    SELECT @StartDate AS WorkDate
    UNION ALL
    SELECT DATEADD(DAY, 1, WorkDate)
    FROM Days
    WHERE WorkDate < @EndDate
)
SELECT
    d.WorkDate,
    ar.CheckInTime,
    ar.CheckOutTime,
    CASE
        WHEN ar.CheckInTime IS NOT NULL AND ar.CheckOutTime IS NOT NULL
            THEN CAST(DATEDIFF(MINUTE, ar.CheckInTime, ar.CheckOutTime) / 60.0 AS DECIMAL(10,2))
        ELSE NULL
    END AS WorkHours,
    ar.Note
FROM Days d
LEFT JOIN AttendanceRecords ar
    ON ar.EmployeeId = @EmployeeId
   AND ar.WorkDate = d.WorkDate
ORDER BY d.WorkDate
OPTION (MAXRECURSION 400);";

        return Db.ExecuteQuery(
            sql,
            new SqlParameter("@EmployeeId", employeeId),
            new SqlParameter("@StartDate", start.Date),
            new SqlParameter("@EndDate", end.Date));
    }
}

namespace HotelManager.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
ApplicationConfiguration.Initialize();

var localDbConnection = @"Server=(localdb)\MSSQLLocalDB;Database=HotelManagement;Trusted_Connection=True;TrustServerCertificate=True;";
HotelManager.DAL.Db.SetConnectionString(localDbConnection);

var scriptPath = Path.Combine(AppContext.BaseDirectory, "hotel_management.sql");
try
{
    DbInitializer.EnsureDatabase(localDbConnection, scriptPath);
}
catch (Exception ex)
{
    MessageBox.Show($"Khởi tạo CSDL thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

Application.Run(new MainForm());
    }
}

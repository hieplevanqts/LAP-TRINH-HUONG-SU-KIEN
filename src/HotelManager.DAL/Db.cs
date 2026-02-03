using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HotelManager.DAL;

public static class Db
{
    public static SqlConnection GetOpenConnection()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["HotelDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string 'HotelDb'.");
        }

        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using var connection = GetOpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        return command.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        using var connection = GetOpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        return command.ExecuteScalar();
    }

    public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
    {
        using var connection = GetOpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);

        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }
}

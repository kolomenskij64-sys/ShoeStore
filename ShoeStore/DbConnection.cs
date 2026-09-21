using System.Data.SqlClient;

namespace ShoeStore
{
    public static class DbConnection
    {
        // 🔧 СТРОКА ПОДКЛЮЧЕНИЯ - подставь свой сервер и БД!
        private static string connectionString =
            @"Server=DESKTOP-HLP1J1G;Database=[ShoeStore];Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
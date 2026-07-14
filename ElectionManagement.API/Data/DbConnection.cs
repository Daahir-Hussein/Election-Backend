using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Data
{
    public class DbConnection
    {
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(
                "Server=DESKTOP-DM86IDB;Database=ElectionDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
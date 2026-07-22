using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Data
{
    public static class DbConnection
    {
        private static string? _connectionString;

        public static void Configure(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is not configured."
                );
        }

        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection has not been initialized."
                );
            }

            return new SqlConnection(_connectionString);
        }
    }
}
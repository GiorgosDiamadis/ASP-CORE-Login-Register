using MySqlConnector;

namespace WebApplication.Database
{
    public class MySqlContext
    {
        private string ConnectionString { get; set; }

        public MySqlContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
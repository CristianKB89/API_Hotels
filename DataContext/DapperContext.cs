using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace API_Hotels.DataContext
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDb");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}

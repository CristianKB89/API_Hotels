using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System;

namespace API_Hotels.DataContext
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = Environment.GetEnvironmentVariable("MySqlConnectionString");
        }

        public IDbConnection CreateConnection()
            => new MySqlConnection(_connectionString);
    }
}

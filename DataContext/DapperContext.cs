﻿using Microsoft.Extensions.Configuration;
using System.Data;
using MySqlConnector;

namespace API_Hotels.DataContext
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnectionString");            
        }

        public IDbConnection CreateConnection()
            => new MySqlConnection(_connectionString);
    }
}

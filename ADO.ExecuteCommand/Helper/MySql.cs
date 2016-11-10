using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ADO.ExecuteCommand.Helper
{
    public sealed class MySql : CommandHelper
    {
        public MySql(string connectionString) : base(connectionString)
        {
        }

        public override IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new NullReferenceException("ConnectionString");
            return new MySqlConnection( ConnectionString );
        }

        protected override IDataParameter GetParameter()
        {
            return new MySqlParameter(); 
        }
    }
}
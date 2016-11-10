
using System;
using System.Data;
using System.Data.SqlClient;

namespace ADO.ExecuteCommand.Helper
{
    public class MsSql : CommandHelper
    {
        public MsSql(string connectionString) : base(connectionString)
        {
        }

        public override IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new NullReferenceException("ConnectionString");
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Return a database connection using connection string passed as parameter
        /// </summary>
        /// <param name="connectionString">connection string to use</param>
        /// <returns>Database connection</returns>
        public IDbConnection GetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("connectionString");
            return new SqlConnection(connectionString);
        }

        protected override IDataParameter GetParameter()
        {
            return new SqlParameter();
        }
    }
}

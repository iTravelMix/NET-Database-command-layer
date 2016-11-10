using System;
using System.Data;
using Npgsql;

namespace ADO.ExecuteCommand.Helper
{
    public class PgSql : CommandHelper
    {
        public PgSql(string connectionString) : base(connectionString)
        {
        }

        public override IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new NullReferenceException("ConnectionString");
            return new NpgsqlConnection(ConnectionString);
        }

        public IDbConnection GetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("connectionString");
            return new NpgsqlConnection(connectionString);
        }

        protected override IDataParameter GetParameter()
        {
            return new NpgsqlParameter();
        }
    }
}

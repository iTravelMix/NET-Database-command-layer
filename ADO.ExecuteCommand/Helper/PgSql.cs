using System;
using System.Data;
using Npgsql;

namespace ADO.ExecuteCommand.Helper
{
    public class PgSql : CommandHelper
    {
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

        protected override IDbDataAdapter GetDataAdapter()
        {
            return new NpgsqlDataAdapter();
        }

        protected override void DeriveParameters(IDbCommand cmd)
        {
            if (!(cmd is NpgsqlCommand)) throw new ArgumentException("The command provided is not a NpgSqlCommand instance.", nameof(cmd));
            NpgsqlCommandBuilder.DeriveParameters((NpgsqlCommand)cmd);
        }

        protected override DataTable FillTable(IDbDataAdapter da)
        {
            var dt = new DataTable();
            ((NpgsqlDataAdapter)da).Fill(dt);

            return dt;
        }
    }
}

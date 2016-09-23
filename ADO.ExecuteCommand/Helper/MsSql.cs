
using System;
using System.Data;
using System.Data.SqlClient;

namespace ADO.ExecuteCommand.Helper
{
    public class MsSql : CommandHelper
    {
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

        protected override IDbDataAdapter GetDataAdapter()
        {
            return new SqlDataAdapter();
        }

        protected override void DeriveParameters(IDbCommand cmd)
        {
            if (!(cmd is SqlCommand)) throw new ArgumentException("The command provided is not a SqlCommand instance.", nameof(cmd));
            SqlCommandBuilder.DeriveParameters((SqlCommand)cmd);
        }

        protected override DataTable FillTable(IDbDataAdapter da)
        {
            var dt = new DataTable();
            ((SqlDataAdapter)da).Fill(dt);

            return dt;
        }
    }
}

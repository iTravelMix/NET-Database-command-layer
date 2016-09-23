using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ADO.ExecuteCommand.Helper
{
    public sealed class MySql : CommandHelper
    {
        public override IDbConnection GetConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new NullReferenceException("ConnectionString");
            return new MySqlConnection( ConnectionString );
        }

        protected override IDbDataAdapter GetDataAdapter()
        {
            return new MySqlDataAdapter();
        }

        protected override void DeriveParameters(IDbCommand cmd)
        {
            if (!(cmd is MySqlCommand)) throw new ArgumentException("The command provided is not a MySqlCommand instance.", nameof(cmd));
            MySqlCommandBuilder.DeriveParameters((MySqlCommand)cmd);
        }

        protected override IDataParameter GetParameter()
        {
            return new MySqlParameter(); 
        }

        protected override void ClearCommand(IDbCommand command)
        {
            var canClear = true;
            foreach(IDataParameter commandParameter in command.Parameters)
            {
                if (commandParameter.Direction != ParameterDirection.Input) canClear = false;
            }
            
            if (canClear)
            {
                command.Parameters.Clear();
            }

            command.Parameters.Clear();
        }

        protected override DataTable FillTable(IDbDataAdapter da)
        {
            var dt = new DataTable();
            ((MySqlDataAdapter) da).Fill(dt);

            return dt;
        }
    }
}
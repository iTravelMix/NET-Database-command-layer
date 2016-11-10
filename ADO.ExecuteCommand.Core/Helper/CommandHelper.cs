using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Helper
{
    public abstract partial class CommandHelper
    {
        #region Declare members

        protected string ConnectionString;

        #endregion

        protected CommandHelper(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        #region Provider specific abstract methods

        public abstract IDbConnection GetConnection();
        protected abstract IDataParameter GetParameter();

        #endregion

        protected virtual IDataParameter GetParameter(string name, object value)
        {
            var parameter = this.GetParameter();
            parameter.ParameterName = name;
            parameter.Value = value;

            return parameter;
        }

        #region private utility methods

        protected virtual void AttachParameters(IDbCommand command, IDataParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (commandParameters == null) return;

            foreach (var p in commandParameters.Where(p => p != null))
            {
                if ((p.Direction == ParameterDirection.InputOutput ||
                       p.Direction == ParameterDirection.Input) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        protected virtual bool PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters)
        {
            var mustCloseConnection = false;

            if (command == null) throw new ArgumentNullException(nameof(command));
            if (string.IsNullOrEmpty(commandText)) throw new ArgumentNullException(nameof(commandText));

            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }

            command.Connection = connection;
            command.CommandText = commandText;

            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was roll-backed or committed, please provide an open transaction.", nameof(transaction));
                command.Transaction = transaction;
            }

            command.CommandType = commandType;

            if (commandParameters != null)
            {
                this.AttachParameters(command, commandParameters);
            }

            return mustCloseConnection;
        }

        #endregion private utility methods

        #region ExecuteNonCommand

        public int ExecuteCommand(Command command)
        {
            var dataParameters = this.GetCriterialParameters(command.Parameters);
            return this.ExecuteNonQuery(CommandType.Text, command.Expression, dataParameters);
        }

        public void ExecuteBatchCommand(CommandBatch commandBatch)
        {
            using (var connection = this.GetConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                foreach (var command in commandBatch.Commands)
                {
                    try
                    {
                        var dataParameters = this.GetCriterialParameters(command.Parameters);
                        this.ExecuteNonQuery(connection, transaction, CommandType.Text, command.Expression, dataParameters);
                    }
                    catch (Exception)
                    {
                        if (!commandBatch.ThrowOnError) continue;

                        transaction.Rollback();
                        throw;
                    }
                }

                transaction.Commit();
            }
        }

        #endregion

        #region ExecuteNonQuery

        protected int ExecuteNonQuery(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            int rowAffected;

            using (var connection = this.GetConnection())
            {
                IDbTransaction transaction = null;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    rowAffected = this.ExecuteNonQuery(connection, transaction, commandType, commandText, commandParameters);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction?.Rollback();
                    throw;
                }
            }

            return rowAffected;
        }

        protected virtual int ExecuteNonQuery(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var cmd = connection.CreateCommand();
            var mustCloseConnection = this.PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

            var retval = cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
            if (mustCloseConnection) connection.Close();

            return retval;
        }
        #endregion ExecuteNonQuery

        #region Parameter Discovery Functions

        protected IDataParameter[] GetCriterialParameters(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return parameters?.Select(p => this.GetParameter(p.Key, p.Value)).ToArray();
        }

        #endregion Parameter Discovery Functions

    }
}
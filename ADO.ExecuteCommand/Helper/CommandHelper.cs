using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Helper
{
    public abstract class CommandHelper
    {
        #region Declare members

        protected static Hashtable ParamCache = Hashtable.Synchronized(new Hashtable());
        protected static string ConnectionString;

        #endregion

        #region Provider specific abstract methods

        public abstract IDbConnection GetConnection();
        protected abstract IDataParameter GetParameter();
        protected abstract IDbDataAdapter GetDataAdapter();
        protected abstract void DeriveParameters(IDbCommand cmd);
        protected abstract DataTable FillTable(IDbDataAdapter da);

        #endregion

        #region Factory

        public static CommandHelper CreateHelper(string providerAlias)
        {
            try
            {
                var dict = ConfigurationManager.GetSection("daProviders") as IDictionary;
                if (dict == null) throw new Exception("Null Reference in DataAccess Provider configuration Session.");

                var providerConfig = dict[providerAlias] as ProviderAlias;
                if (providerConfig == null) throw new Exception("Null Reference in Provider Alias configuration Session.");

                var providerType = providerConfig.TypeName;
                ConnectionString = providerConfig.ConnectionString;

                var daType = Type.GetType(providerType);
                if (daType == null) throw new Exception("Null Reference in Provider type configuration Session.");

                var provider = daType.Assembly.CreateInstance(daType.FullName);
                if (provider is CommandHelper) return provider as CommandHelper;

                throw new Exception("The provider specified does not extends the AdoHelper abstract class.");
            }
            catch (Exception e)
            {
                throw new Exception("If the section is not defined on the configuration file this method can't be used to create an AdoHelper instance.", e);
            }
        }

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

        protected void AssignParameterValues(IDataParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                return;
            }

            var columns = dataRow.Table.Columns;

            var i = 0;
            foreach (var commandParameter in commandParameters)
            {
                if (commandParameter.ParameterName == null || commandParameter.ParameterName.Length <= 1)
                    throw new Exception($"Please provide a valid parameter name on the parameter #{i}, the ParameterName property has the following value: '{commandParameter.ParameterName}'.");

                if (columns.Contains(commandParameter.ParameterName)) commandParameter.Value = dataRow[commandParameter.ParameterName];
                else if (columns.Contains(commandParameter.ParameterName.Substring(1))) commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];

                i++;
            }
        }

        protected void AssignParameterValues(IDataParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }

            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            for (var i = 0; i < commandParameters.Length; i++)
            {
                var param = parameterValues[i] as IDataParameter;
                if (param != null)
                {
                    var paramInstance = param;
                    commandParameters[i].Value = paramInstance.Value ?? DBNull.Value;

                    continue;
                }

                commandParameters[i].Value = parameterValues[i] ?? DBNull.Value;
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

        protected virtual void ClearCommand(IDbCommand command)
        {
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
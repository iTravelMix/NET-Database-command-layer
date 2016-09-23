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
                // Check for derived output value with no value assigned
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
                // Do nothing if we get no data
                return;
            }

            var columns = dataRow.Table.Columns;

            var i = 0;
            // Set the parameters values
            foreach (var commandParameter in commandParameters)
            {
                // Check the parameter name
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
                // Do nothing if we get no data
                return;
            }

            // We must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            // Iterate through the IDataParameters, assigning the values from the corresponding position in the 
            // value array
            for (var i = 0; i < commandParameters.Length; i++)
            {
                // If the current array value derives from IDataParameter, then assign its Value property
                if (parameterValues[i] is IDataParameter)
                {
                    var paramInstance = (IDataParameter)parameterValues[i];
                    commandParameters[i].Value = paramInstance.Value ?? DBNull.Value;
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        protected virtual void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (string.IsNullOrEmpty(commandText)) throw new ArgumentNullException(nameof(commandText));

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", nameof(transaction));
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                this.AttachParameters(command, commandParameters);
            }
        }

        protected virtual void ClearCommand(IDbCommand command)
        {
        }

        #endregion private utility methods

        #region ExecuteNonCommand

        public int ExecuteCommand(ICommand command)
        {
            var dataParameters = this.GetCriterialParameters(command.Parameters);
            return this.ExecuteNonQuery(CommandType.Text, command.Expression, dataParameters);
        }

        //public void ExecuteBatchCommand(ICommandBatch command)
        //{
        //    IDbTransaction transaction = null;
        //    using (var connection = this.GetConnection())
        //    {
        //        connection.Open();
        //        transaction = connection.BeginTransaction();

        //        foreach (var VARIABLE in com)
        //        {
                    
        //        }
        //        var dataParameters = this.GetCriterialParameters(command.Parameters);
        //    }
        //    return this.ExecuteNonQuery(CommandType.Text, command.Expression, dataParameters);
        //}

        #endregion

        #region ExecuteNonQuery

        internal int ExecuteNonQuery(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            int rowAffected;

            using (var connection = this.GetConnection())
            {
                IDbTransaction transaction = null;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    rowAffected = this.ExecuteNonQuery(connection, commandType, commandText, commandParameters);
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

        protected virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            // Create a command and prepare it for execution
            var cmd = connection.CreateCommand();
            bool mustCloseConnection;
            this.PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            var retval = cmd.ExecuteNonQuery();

            // Detach the IDataParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            if (mustCloseConnection) connection.Close();

            return retval;
        }

        protected int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(spName)) throw new ArgumentNullException(nameof(spName));

            if ((parameterValues == null) || (parameterValues.Length <= 0))
            {
                // Otherwise we can just call the SP without params
                return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }

            // If we receive parameter values, we need to figure out where they go
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            var commandParameters = this.GetSpParameterSet(connection, spName);

            // Assign the provided values to these parameters based on parameter order
            this.AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of IDataParameters
            return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
        }

        #endregion ExecuteNonQuery


        #region CreateCommand

        protected virtual IDbCommand CreateCommand(IDbConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(spName)) throw new ArgumentNullException(nameof(spName));

            // Create a IDbCommand
            var cmd = connection.CreateCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            // If we receive parameter values, we need to figure out where they go
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                var commandParameters = this.GetSpParameterSet(connection, spName);

                // Assign the provided source columns to these parameters based on parameter order
                for (var index = 0; index < sourceColumns.Length; index++)
                    commandParameters[index].SourceColumn = sourceColumns[index];

                // Attach the discovered parameters to the IDbCommand object
                this.AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }
        #endregion

        #region ExecuteNonQueryTypedParams
        ///// <summary>
        ///// Ejecuta un procedimiento almacenado mediante un <see cref="IDbCommand"/> (que no retorna datos)
        ///// sobre la base de datos asociada a la cadena de conexión usando el valor de las columnas del DataRow como 
        ///// parámetros del procedimiento almacenado. Este método puede realizar una consulta sobre la base de datos
        ///// para obtener los parámetros asociados al procedimiento y relacionar los valores del DataRow a los parámetros según su orden. 
        ///// 
        ///// Esta consulta se realiza solo la primera vez que el procedimiento es invocado.
        ///// </summary>
        ///// <param name="spName">El nombre del procedimiento almacenado</param>
        ///// <param name="dataRow">El <see cref="DataRow"/> usado para pasar los valores de los parámetros al procedimiento almacenado.</param>
        ///// <returns>Un int que representa el número de filas afectadas por el comando</returns>
        //public virtual int ExecuteNonQueryTypedParams(String spName, DataRow dataRow)
        //{
        //    if( string.IsNullOrEmpty(spName) ) throw new ArgumentNullException( "spName" );

        //    if (dataRow == null || dataRow.ItemArray.Length <= 0)
        //    {
        //        return ExecuteNonQuery(CommandType.StoredProcedure, spName);
        //    }

        //    // If the row has values, the store procedure parameters must be initialized
        //    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //    var commandParameters = GetSpParameterSet(spName);

        //    // Set the parameters values
        //    AssignParameterValues(commandParameters, dataRow);

        //    return ExecuteNonQuery(CommandType.StoredProcedure, spName, commandParameters);
        //}

        ///// <summary>
        ///// Ejecuta un procedimiento almacenado mediante un <see cref="IDbCommand"/> (que no retorna datos)
        ///// sobre la base de datos asociada a la conexión y usando el valor de las columnas del DataRow como 
        ///// parámetros del procedimiento almacenado. Este método puede realizar una consulta sobre la base de datos
        ///// para obtener los parámetros asociados al procedimiento y relacionar los valores del DataRow a los parámetros según su orden. 
        ///// 
        ///// Esta consulta se realiza solo la primera vez que el procedimiento es invocado.
        ///// </summary>
        ///// <param name="connection">Una conexión válida a la base de datos</param>
        ///// <param name="spName">El nombre del procedimiento almacenado</param>
        ///// <param name="dataRow">El <see cref="DataRow"/> usado para pasar los valores de los parámetros al procedimiento almacenado.</param>
        ///// <returns>Un int que representa el número de filas afectadas por el comando</returns>
        //public virtual int ExecuteNonQueryTypedParams(IDbConnection connection, String spName, DataRow dataRow)
        //{
        //    if( connection == null ) throw new ArgumentNullException( "connection" );
        //    if( string.IsNullOrEmpty(spName) ) throw new ArgumentNullException( "spName" );

        //    if (dataRow == null || dataRow.ItemArray.Length <= 0)
        //    {
        //        return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        //    }

        //    // If the row has values, the store procedure parameters must be initialized
        //    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //    var commandParameters = GetSpParameterSet(connection, spName);

        //    // Set the parameters values
        //    AssignParameterValues(commandParameters, dataRow);

        //    return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
        //}

        #endregion

        #region Parameter Discovery Functions


        protected IDataParameter[] GetCriterialParameters(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return parameters?.Select(p => this.GetParameter(p.Key, p.Value)).ToArray();
        }

        protected virtual IDataParameter[] GetSpParameterSet(string spName)
        {
            return this.GetSpParameterSet(spName, false);
        }

        protected virtual IDataParameter[] GetSpParameterSet(string spName, bool includeReturnValueParameter)
        {
            if (string.IsNullOrEmpty(spName)) throw new ArgumentNullException(nameof(spName));

            using (var connection = this.GetConnection())
            {
                return this.GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        protected virtual IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName)
        {
            return this.GetSpParameterSet(connection, spName, false);
        }

        protected virtual IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (connection as ICloneable == null) throw new ArgumentException("can´t discover parameters if the connection doesn´t implement the ICloneable interface", nameof(connection));

            var clonedConnection = (IDbConnection)((ICloneable)connection).Clone();
            return this.GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
        }

        private IDataParameter[] GetSpParameterSetInternal(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(spName)) throw new ArgumentNullException(nameof(spName));

            //string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter":"");

            var cachedParameters = AdoHelperParameterCache.GetCachedParameterSet(connection.ConnectionString,
                                                                                 spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : ""));

            if (cachedParameters == null)
            {
                var spParameters = this.DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                AdoHelperParameterCache.CacheParameterSet(ConnectionString, spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : ""), spParameters);

                cachedParameters = AdoHelperParameterCache.CloneParameters(spParameters);
            }

            return cachedParameters;
        }

        private IDataParameter[] DiscoverSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(spName)) throw new ArgumentNullException(nameof(spName));

            var cmd = connection.CreateCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            this.DeriveParameters(cmd);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            var discoveredParameters = new IDataParameter[cmd.Parameters.Count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (var discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        #endregion Parameter Discovery Functions

    }

    internal sealed class AdoHelperParameterCache
    {
        private static readonly Hashtable ParamCache = Hashtable.Synchronized(new Hashtable());

        internal static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
        {
            var clonedParameters = new IDataParameter[originalParameters.Length];

            for (var i = 0; i < originalParameters.Length; i++)
            {
                clonedParameters[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #region caching functions

        internal static void CacheParameterSet(string connectionsString, string commandText, params IDataParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(commandText)) throw new ArgumentNullException(nameof(commandText));

            var hashKey = connectionsString + ":" + commandText;

            ParamCache[hashKey] = commandParameters;
        }

        internal static IDataParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(commandText)) throw new ArgumentNullException(nameof(commandText));

            var hashKey = connectionString + ":" + commandText;

            var cachedParameters = ParamCache[hashKey] as IDataParameter[];
            return cachedParameters == null ? null : CloneParameters(cachedParameters);
        }

        #endregion caching functions
    }
}
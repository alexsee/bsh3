// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// class for db access
/// </summary>
public class DbClient : IDisposable
{
    #region Fields

    DbConnection _connection;
    DbTransaction _transaction;
    DbProviderFactory _factory;
    readonly Dictionary<string, DbCommand> _commands = new();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the connection string
    /// </summary>
    public string ConnectionString
    {
        get
        {
            return _connection.ConnectionString;
        }
    }

    /// <summary>
    /// Gets or sets the command timeout
    /// </summary>
    public int CommandTimeout
    {
        get; set;
    }
    public object ConfigurationManager
    {
        get;
    }

    #endregion

    #region Construction / Destruction

    /// <summary>
    ///
    /// </summary>
    /// <param name="connectionStringName">the name of the connection string defined in application configuration</param>
    public DbClient(string connectionString)
    {
        InitializeConnection("System.Data.SQLite", connectionString);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="dbProvider">the database provider name</param>
    /// <param name="connectionString">the connection string</param>
    public DbClient(string dbProvider, string connectionString)
    {
        InitializeConnection(dbProvider, connectionString);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Method to initialize the db connection
    /// </summary>
    /// <param name="providerName">the db provider name</param>
    /// <param name="connectionString">the connection string</param>
    /// <param name="connectTimeout">the connect timeout</param>
    private void InitializeConnection(string providerName, string connectionString)
    {
        _factory = DbProviderFactories.GetFactory(providerName);

        if (_factory == null)
        {
            throw new InvalidOperationException(string.Format("The factory for data provider {0} could not be found!", providerName));
        }

        _connection = _factory.CreateConnection();
        _connection.ConnectionString = connectionString;
    }

    /// <summary>
    /// Method to open the connection
    /// </summary>
    private void OpenConnection()
    {
        if (_connection != null && _connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    /// <summary>
    /// Method to open the connection
    /// </summary>
    private async Task OpenConnectionAsync()
    {
        if (_connection != null && _connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }
    }

    /// <summary>
    /// Method to close the connection
    /// </summary>
    private void CloseConnection()
    {
        if (_connection != null && _connection.State != ConnectionState.Closed && _transaction == null)
        {
            _connection.Close();
        }
    }

    /// <summary>
    /// Starts a new database transaction and adds all commands to this transaction.
    /// </summary>
    public void BeginTransaction()
    {
        OpenConnection();
        _transaction = _connection.BeginTransaction();
    }

    /// <summary>
    /// Commits an open transaction to the database.
    /// </summary>
    public void CommitTransaction()
    {
        if (_transaction != null)
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        CloseConnection();
    }

    /// <summary>
    /// Rolls back all changes to the database from this transaction.
    /// </summary>
    public void RollbackTransaction()
    {
        if (_transaction != null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        CloseConnection();
    }

    /// <summary>
    /// Method to dispose the connection
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_connection != null)
        {
            _commands.Clear();

            CloseConnection();
            _connection.Dispose();
        }
    }

    /// <summary>
    /// Method to call a stored procedure and retrieve the result
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the command to execute</param>
    /// <param name="parameters">parameters for calling the stored procedure</param>
    /// <returns>the dataset with the execution results</returns>
    public DataSet ExecuteDataSet(CommandType commandType, string commandText, IDataParameter[] parameters)
    {
        return ExecuteDataSet(commandType, commandText, parameters, 60000);
    }

    /// <summary>
    /// Method to call a stored procedure and retrieve the result
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the command to execute</param>
    /// <param name="parameters">parameters for calling the stored procedure</param>
    /// <returns>the dataset with the execution results</returns>
    public DataSet ExecuteDataSet(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        var dsResult = new DataSet();
        // open the connection
        OpenConnection();

        IDbCommand command = CreateCommand(commandType, commandText, parameters, commandTimeout);

        IDbDataAdapter adapter = _factory.CreateDataAdapter();
        adapter.SelectCommand = command;
        adapter.Fill(dsResult);

        // close the connection
        CloseConnection();
        return dsResult;
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <returns>the data reader</returns>
    public IDataReader ExecuteDataReader(CommandType commandType, string commandText, IDataParameter[] parameters)
    {
        return ExecuteDataReader(commandType, commandText, parameters, 60000);
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <param name="commandTimeout">the command timeout</param>
    /// <returns>the data reader</returns>
    public IDataReader ExecuteDataReader(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        OpenConnection();

        IDbCommand command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        return command.ExecuteReader();
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <returns>the data reader</returns>
    public async Task<DbDataReader> ExecuteDataReaderAsync(CommandType commandType, string commandText, IDataParameter[] parameters)
    {
        return await ExecuteDataReaderAsync(commandType, commandText, parameters, 60000);
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <param name="commandTimeout">the command timeout</param>
    /// <returns>the data reader</returns>
    public async Task<DbDataReader> ExecuteDataReaderAsync(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        await OpenConnectionAsync();

        var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        return await command.ExecuteReaderAsync();
    }

    public async Task<object> ExecuteScalarAsync(string commandText)
    {
        return await ExecuteScalarAsync(CommandType.Text, commandText, null);
    }

    public async Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, IDataParameter[] parameters)
    {
        return await ExecuteScalarAsync(commandType, commandText, parameters, 60000);
    }

    public async Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        OpenConnection();

        var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        var result = await command.ExecuteScalarAsync();

        CloseConnection();

        return result;
    }

    public int ExecuteNonQuery(CommandType commandType, string commandText, IDataParameter[] parameters)
    {
        return ExecuteNonQuery(commandType, commandText, parameters, 60000);
    }

    public int ExecuteNonQuery(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        OpenConnection();

        var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        var result = command.ExecuteNonQuery();

        CloseConnection();

        return result;
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText)
    {
        return await ExecuteNonQueryAsync(CommandType.Text, commandText, null, 60000);
    }

    public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, IDataParameter[] parameters)
    {
        return await ExecuteNonQueryAsync(commandType, commandText, parameters, 60000);
    }

    public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        await OpenConnectionAsync();

        var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        var result = await command.ExecuteNonQueryAsync();

        CloseConnection();

        return result;
    }

    /// <summary>
    /// Method to create a command
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="commandText">the command text</param>
    /// <param name="parameters">the parameters</param>
    /// <param name="commandTimeout">the command timeout</param>
    /// <returns>the command</returns>
    private DbCommand CreateCommand(CommandType commandType, string commandText, IDataParameter[] parameters, int commandTimeout)
    {
        DbCommand command;

        if (_commands.ContainsKey(commandText))
        {
            command = _commands[commandText];

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    (command.Parameters[parameter.ParameterName] as IDataParameter).Value = parameter.Value;
                }
            }

            return command;
        }

        command = _connection.CreateCommand();
        command.CommandTimeout = commandTimeout;
        command.CommandText = commandText;
        command.CommandType = commandType;

        if (_transaction != null)
        {
            command.Transaction = _transaction;
        }

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
        }

        _commands.Add(commandText, command);
        return command;
    }

    /// <summary>
    /// Method to create a parameter
    /// </summary>
    /// <param name="parameterName">the parameter name</param>
    /// <param name="dbType">the database type</param>
    /// <param name="size">size of the parameter</param>
    /// <param name="value">the value of the parameter</param>
    /// <returns>the new parameter</returns>
    public IDataParameter CreateParameter(string parameterName, DbType dbType, int size, object value)
    {
        return CreateParameter(parameterName, dbType, size, ParameterDirection.Input, value);
    }

    /// <summary>
    /// Method to create a parameter
    /// </summary>
    /// <param name="parameterName">the parameter name</param>
    /// <param name="dbType">the database type</param>
    /// <param name="size">size of the parameter</param>
    /// <param name="parameterDirection">the parameter direction</param>
    /// <returns></returns>
    public IDataParameter CreateParameter(string parameterName, DbType dbType, int size, ParameterDirection parameterDirection)
    {
        return CreateParameter(parameterName, dbType, size, parameterDirection, null);
    }

    /// <summary>
    /// Method to create a parameter
    /// </summary>
    /// <param name="parameterName">the parameter name</param>
    /// <param name="dbType">the database type</param>
    /// <param name="parameterDirection">the parameter direction</param>
    /// <param name="size">size of the parameter</param>
    /// <param name="value">the value of the parameter</param>
    /// <returns>the new parameter</returns>
    public IDataParameter CreateParameter(string parameterName, DbType dbType, int size, ParameterDirection parameterDirection, object value)
    {
        var parameter = _factory.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Direction = parameterDirection;
        parameter.DbType = dbType;
        parameter.Value = value;
        parameter.Size = size;

        return parameter;
    }

    #endregion
}

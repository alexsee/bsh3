// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// class for db access
/// </summary>
public class DbClient : IDisposable
{
    private const int DefaultCommandTimeout = 60000;

    #region Fields

    SQLiteConnection _connection;
    SQLiteTransaction _transaction;
    readonly Dictionary<string, SQLiteCommand> _commands = new();
    bool _disposed;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the connection string
    /// </summary>
    public string ConnectionString => _connection.ConnectionString;

    #endregion

    #region Construction / Destruction

    /// <summary>
    ///
    /// </summary>
    /// <param name="connectionString">the SQLite connection string</param>
    public DbClient(string connectionString)
    {
        _connection = new SQLiteConnection(connectionString);
    }

    #endregion

    #region Methods

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

    private async Task CloseConnectionAsync()
    {
        if (_connection != null && _connection.State != ConnectionState.Closed && _transaction == null)
        {
            await _connection.CloseAsync();
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
        if (!disposing || _disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var command in _commands.Values)
        {
            command.Dispose();
        }

        _commands.Clear();

        _transaction?.Dispose();
        _transaction = null;

        _connection?.Dispose();
    }

    /// <summary>
    /// Method to call a stored procedure and retrieve the result
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the command to execute</param>
    /// <param name="parameters">parameters for calling the stored procedure</param>
    /// <returns>the dataset with the execution results</returns>
    public DataSet ExecuteDataSet(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        return ExecuteDataSet(commandType, commandText, parameters, DefaultCommandTimeout);
    }

    /// <summary>
    /// Method to call a stored procedure and retrieve the result
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the command to execute</param>
    /// <param name="parameters">parameters for calling the stored procedure</param>
    /// <returns>the dataset with the execution results</returns>
    public DataSet ExecuteDataSet(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
    {
        OpenConnection();

        var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        using var adapter = new SQLiteDataAdapter(command);

        try
        {
            var dsResult = new DataSet();
            adapter.Fill(dsResult);
            return dsResult;
        }
        finally
        {
            CloseConnection();
        }
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <returns>the data reader</returns>
    public IDataReader ExecuteDataReader(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        return ExecuteDataReader(commandType, commandText, parameters, DefaultCommandTimeout);
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <param name="commandTimeout">the command timeout</param>
    /// <returns>the data reader</returns>
    public IDataReader ExecuteDataReader(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
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
    public async Task<DbDataReader> ExecuteDataReaderAsync(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        return await ExecuteDataReaderAsync(commandType, commandText, parameters, DefaultCommandTimeout);
    }

    /// <summary>
    /// Method to execute a datareader
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="procedureName">the procedurename</param>
    /// <param name="parameters">the parameters</param>
    /// <param name="commandTimeout">the command timeout</param>
    /// <returns>the data reader</returns>
    public async Task<DbDataReader> ExecuteDataReaderAsync(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
    {
        await OpenConnectionAsync();

        var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
        return await command.ExecuteReaderAsync();
    }

    public async Task<object> ExecuteScalarAsync(string commandText)
    {
        return await ExecuteScalarAsync(CommandType.Text, commandText, null);
    }

    public async Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        return await ExecuteScalarAsync(commandType, commandText, parameters, DefaultCommandTimeout);
    }

    public async Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
    {
        await OpenConnectionAsync();

        try
        {
            var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
            return await command.ExecuteScalarAsync();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public int ExecuteNonQuery(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        return ExecuteNonQuery(commandType, commandText, parameters, DefaultCommandTimeout);
    }

    public int ExecuteNonQuery(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
    {
        OpenConnection();

        try
        {
            var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
            return command.ExecuteNonQuery();
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText)
    {
        return await ExecuteNonQueryAsync(CommandType.Text, commandText, null, DefaultCommandTimeout);
    }

    public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        return await ExecuteNonQueryAsync(commandType, commandText, parameters, DefaultCommandTimeout);
    }

    public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
    {
        await OpenConnectionAsync();

        try
        {
            var command = CreateCommand(commandType, commandText, parameters, commandTimeout);
            return await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    /// <summary>
    /// Method to create a command
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="commandText">the command text</param>
    /// <param name="parameters">the parameters</param>
    /// <param name="commandTimeout">the command timeout</param>
    /// <returns>the command</returns>
    private SQLiteCommand CreateCommand(CommandType commandType, string commandText, (string, object)[] parameters, int commandTimeout)
    {
        if (!_commands.TryGetValue(commandText, out var command))
        {
            command = _connection.CreateCommand();
            command.CommandText = commandText;
            _commands.Add(commandText, command);
        }

        command.CommandType = commandType;
        command.CommandTimeout = commandTimeout;
        command.Transaction = _transaction;
        command.Parameters.Clear();

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
            }
        }

        return command;
    }
    #endregion
}

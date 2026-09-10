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
/// SQLite database client used by the backup engine.
/// </summary>
public class DbClient : IDisposable
{
    private const int DefaultCommandTimeout = 60000;

    #region Fields

    SQLiteConnection _connection;
    SQLiteTransaction _transaction;
    readonly List<SQLiteCommand> _readerCommands = new();
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
    /// Creates a SQLite client for the given connection string.
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
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    /// <summary>
    /// Method to open the connection
    /// </summary>
    private async Task OpenConnectionAsync()
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }
    }

    /// <summary>
    /// Method to close the connection
    /// </summary>
    private void CloseConnection()
    {
        if (_connection.State != ConnectionState.Closed && _transaction == null)
        {
            _connection.Close();
        }
    }

    private async Task CloseConnectionAsync()
    {
        if (_connection.State != ConnectionState.Closed && _transaction == null)
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var command in _readerCommands)
        {
            command.Dispose();
        }

        _readerCommands.Clear();

        if (_transaction != null)
        {
            try
            {
                _transaction.Rollback();
            }
            catch
            {
                // rollback best-effort during dispose; connection teardown rolls back anyway
            }

            _transaction.Dispose();
            _transaction = null;
        }

        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Executes a query and returns the result as a <see cref="DataSet"/>.
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="commandText">the command to execute</param>
    /// <param name="parameters">parameters for the command</param>
    /// <returns>the dataset with the execution results</returns>
    public DataSet ExecuteDataSet(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        OpenConnection();

        try
        {
            using var command = CreateCommand(commandType, commandText, parameters);
            using var adapter = new SQLiteDataAdapter(command);
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
    /// Executes a query and returns a data reader. The reader must be disposed
    /// before this client is disposed.
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="commandText">the command text</param>
    /// <param name="parameters">the parameters</param>
    /// <returns>the data reader</returns>
    public IDataReader ExecuteDataReader(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        OpenConnection();

        var command = CreateCommand(commandType, commandText, parameters);
        try
        {
            var reader = command.ExecuteReader();
            _readerCommands.Add(command);
            return reader;
        }
        catch
        {
            command.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Executes a query and returns a data reader. The reader must be disposed
    /// before this client is disposed.
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="commandText">the command text</param>
    /// <param name="parameters">the parameters</param>
    /// <returns>the data reader</returns>
    public async Task<DbDataReader> ExecuteDataReaderAsync(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        await OpenConnectionAsync();

        var command = CreateCommand(commandType, commandText, parameters);
        try
        {
            var reader = await command.ExecuteReaderAsync();
            _readerCommands.Add(command);
            return reader;
        }
        catch
        {
            command.Dispose();
            throw;
        }
    }

    public async Task<object> ExecuteScalarAsync(string commandText)
    {
        return await ExecuteScalarAsync(CommandType.Text, commandText, null);
    }

    public async Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        await OpenConnectionAsync();

        try
        {
            using var command = CreateCommand(commandType, commandText, parameters);
            return await command.ExecuteScalarAsync();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public int ExecuteNonQuery(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        OpenConnection();

        try
        {
            using var command = CreateCommand(commandType, commandText, parameters);
            return command.ExecuteNonQuery();
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText)
    {
        return await ExecuteNonQueryAsync(CommandType.Text, commandText, null);
    }

    public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        await OpenConnectionAsync();

        try
        {
            using var command = CreateCommand(commandType, commandText, parameters);
            return await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    /// <summary>
    /// Creates a new command bound to the current connection and transaction.
    /// </summary>
    /// <param name="commandType">the command type</param>
    /// <param name="commandText">the command text</param>
    /// <param name="parameters">the parameters</param>
    /// <returns>the command</returns>
    private SQLiteCommand CreateCommand(CommandType commandType, string commandText, (string, object)[] parameters)
    {
        var command = _connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandType = commandType;
        command.CommandTimeout = DefaultCommandTimeout;
        command.Transaction = _transaction;

        foreach (var parameter in parameters ?? [])
        {
            command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
        }

        return command;
    }
    #endregion
}

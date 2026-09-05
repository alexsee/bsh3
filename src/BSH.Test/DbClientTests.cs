// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;
using NUnit.Framework;

namespace BSH.Test;

public class DbClientTests
{
    private string databaseFile;

    [SetUp]
    public void SetUp()
    {
        databaseFile = Path.Combine(Path.GetTempPath(), $"bsh-db-client-{Guid.NewGuid():N}.db");
    }

    [TearDown]
    public void TearDown()
    {
        SQLiteConnection.ClearAllPools();
        File.Delete(databaseFile);
    }

    [Test]
    public async Task ExecuteNonQueryRebindsParametersOnEachCall()
    {
        using var dbClient = CreateDbClient();
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE entries (value TEXT)");

        const string insert = "INSERT INTO entries (value) VALUES (@value)";
        await dbClient.ExecuteNonQueryAsync(CommandType.Text, insert, new (string, object)[] { ("value", "first") });
        await dbClient.ExecuteNonQueryAsync(CommandType.Text, insert, new (string, object)[] { ("value", "second") });

        using var values = dbClient.ExecuteDataSet(CommandType.Text, "SELECT value FROM entries ORDER BY rowid", null);

        Assert.That(values.Tables[0].Rows[0]["value"], Is.EqualTo("first"));
        Assert.That(values.Tables[0].Rows[1]["value"], Is.EqualTo("second"));
    }

    [Test]
    public async Task ExecuteNonQueryAfterRollbackCommitsNewWork()
    {
        using var dbClient = CreateDbClient();
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE entries (value TEXT)");

        const string insert = "INSERT INTO entries (value) VALUES (@value)";
        dbClient.BeginTransaction();
        await dbClient.ExecuteNonQueryAsync(CommandType.Text, insert, new (string, object)[] { ("value", "rolled back") });
        dbClient.RollbackTransaction();

        await dbClient.ExecuteNonQueryAsync(CommandType.Text, insert, new (string, object)[] { ("value", "committed") });

        Assert.That(await dbClient.ExecuteScalarAsync("SELECT value FROM entries"), Is.EqualTo("committed"));
    }

    [Test]
    public async Task ExecuteDataReaderThenSameSqlNonQuerySucceeds()
    {
        using var dbClient = CreateDbClient();
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE entries (value TEXT)");
        await dbClient.ExecuteNonQueryAsync(CommandType.Text, "INSERT INTO entries (value) VALUES (@value)", new (string, object)[] { ("value", "first") });

        const string select = "SELECT value FROM entries";
        using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, select, null))
        {
            Assert.That(await reader.ReadAsync(), Is.True);
            Assert.That(reader.GetString(0), Is.EqualTo("first"));
        }

        using var secondReader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, select, null);
        Assert.That(await secondReader.ReadAsync(), Is.True);
        Assert.That(secondReader.GetString(0), Is.EqualTo("first"));
    }

    private DbClient CreateDbClient()
    {
        return new DbClient($"Data Source={databaseFile};Mode=ReadWriteCreate;Pooling=False;");
    }
}

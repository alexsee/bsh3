// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

public class ImportVersionSourcesTests
{
    private string databaseFile;

    [SetUp]
    public void SetUp()
    {
        databaseFile = Path.Combine(Path.GetTempPath(), $"bsh-import-sources-{Guid.NewGuid():N}.db");
    }

    [TearDown]
    public void TearDown()
    {
        SQLiteConnection.ClearAllPools();
        File.Delete(databaseFile);
    }

    [Test]
    public async Task QuotedSourcePathIsStoredWithoutRewritingTheRow()
    {
        using var dbClient = new DbClient($"Data Source={databaseFile};Mode=ReadWriteCreate;Pooling=False;");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE versiontable (versionID INTEGER PRIMARY KEY, versionTitle TEXT, versionSources TEXT)");
        await dbClient.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionTitle, versionSources) VALUES (1, 'keep', 'C:\\Old')");
        await dbClient.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionTitle, versionSources) VALUES (2, 'other', 'C:\\Other')");

        var quoted = "C:\\Docs\" , versionTitle = \"pwned\", versionSources = \"x";
        await ucDoConfigure.UpdateVersionSourcesAsync(dbClient, "1", quoted);
        await ucDoConfigure.UpdateVersionSourcesAsync(dbClient, "2", "D:\\Photos");

        var title = await dbClient.ExecuteScalarAsync("SELECT versionTitle FROM versiontable WHERE versionID = 1");
        var sources = await dbClient.ExecuteScalarAsync("SELECT versionSources FROM versiontable WHERE versionID = 1");
        var otherTitle = await dbClient.ExecuteScalarAsync("SELECT versionTitle FROM versiontable WHERE versionID = 2");
        var otherSources = await dbClient.ExecuteScalarAsync("SELECT versionSources FROM versiontable WHERE versionID = 2");

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("keep"));
            Assert.That(sources, Is.EqualTo(quoted));
            Assert.That(otherTitle, Is.EqualTo("other"));
            Assert.That(otherSources, Is.EqualTo("D:\\Photos"));
        });
    }
}

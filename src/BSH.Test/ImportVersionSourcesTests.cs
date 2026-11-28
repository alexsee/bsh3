// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
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
        foreach (var path in new[] { databaseFile, databaseFile + "-wal", databaseFile + "-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Test]
    public async Task QuotedSourcePathIsStoredWithoutRewritingTheRow()
    {
        var factory = new DbClientFactory();
        await factory.InitializeAsync(databaseFile);
        await factory.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionTitle, versionSources) VALUES (1, 'keep', 'C:\\Old')");
        await factory.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionTitle, versionSources) VALUES (2, 'other', 'C:\\Other')");

        const string quoted = "C:\\Docs\"Archive's";
        await BackupDatabaseImport.RemapVersionSourcesAsync(factory,
            [new VersionDetails { Id = "1", Sources = @"C:\Old" }, new VersionDetails { Id = "2", Sources = @"C:\Other" }],
            [(@"C:\Old", quoted)]);

        using var dbClient = factory.CreateDbClient();
        var title = await dbClient.ExecuteScalarAsync("SELECT versionTitle FROM versiontable WHERE versionID = 1");
        var sources = await dbClient.ExecuteScalarAsync("SELECT versionSources FROM versiontable WHERE versionID = 1");
        var otherTitle = await dbClient.ExecuteScalarAsync("SELECT versionTitle FROM versiontable WHERE versionID = 2");
        var otherSources = await dbClient.ExecuteScalarAsync("SELECT versionSources FROM versiontable WHERE versionID = 2");

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("keep"));
            Assert.That(sources, Is.EqualTo(quoted));
            Assert.That(otherTitle, Is.EqualTo("other"));
            Assert.That(otherSources, Is.EqualTo(@"C:\Other"));
        });
    }
}

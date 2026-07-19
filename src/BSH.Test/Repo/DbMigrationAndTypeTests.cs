// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Types.Exceptions;
using NUnit.Framework;
using System;
using System.IO;

namespace BSH.Test.Repo;

public class DbMigrationServiceTests
{
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;

    [SetUp]
    public async Task SetUp()
    {
        if (File.Exists("testdb_migration.db"))
        {
            DbClientFactory.ClosePool();
            File.Delete("testdb_migration.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_migration.db"));
        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
    }

    [TearDown]
    public void TearDown()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_migration.db"))
        {
            File.Delete("testdb_migration.db");
        }
    }

    [Test]
    public async Task InitializeAsync_Succeeds_ForCurrentDatabaseVersion()
    {
        var migration = new DbMigrationService(dbClientFactory, configurationManager);
        await migration.InitializeAsync();
        Assert.That(int.Parse(configurationManager.DBVersion), Is.LessThanOrEqualTo(9));
    }

    [Test]
    public void InitializeAsync_Throws_WhenDatabaseVersionTooNew()
    {
        configurationManager.DBVersion = "99";
        var migration = new DbMigrationService(dbClientFactory, configurationManager);
        Assert.ThrowsAsync<DatabaseIncompatibleException>(async () => await migration.InitializeAsync());
    }
}

public class TypeSmokeTests
{
    [Test]
    public void FolderTableRow_StoresValues()
    {
        var row = new FolderTableRow(@"C:\folder", @"C:\");
        Assert.That(row.Folder, Is.EqualTo(@"C:\folder"));
        Assert.That(row.RootPath, Is.EqualTo(@"C:\"));
    }

    [Test]
    public void ExplorerWindow_StoresValues()
    {
        var window = new ExplorerWindow(@"C:\Docs", "Docs");
        Assert.That(window.Path, Is.EqualTo(@"C:\Docs"));
        Assert.That(window.WindowTitle, Is.EqualTo("Docs"));
    }

    [Test]
    public void DomainExceptions_CanBeConstructed()
    {
        Assert.That(new DatabaseFileNotUpdatedException(), Is.Not.Null);
        Assert.That(new DatabaseIncompatibleException(), Is.Not.Null);
        Assert.That(new DeviceContainsWrongStateException(), Is.Not.Null);
        Assert.That(new DeviceNotReadyException(), Is.Not.Null);
    }
}

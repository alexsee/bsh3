// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using BSH.Test.Helpers;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

public class DatabaseSchemaUpgradeTests
{
    [TestCase("1")]
    [TestCase("8")]
    public async Task InitializeAsync_UpgradesPriorSchemaAndKeepsEngineReadable(string schemaVersion)
    {
        await using var fixture = await SchemaUpgradeFixture.CreateAsync(schemaVersion);

        var migration = new DbMigrationService(fixture.DbFactory, fixture.ConfigurationManager);
        await migration.InitializeAsync();

        Assert.That(fixture.ConfigurationManager.DBVersion, Is.EqualTo(LegacyBackupDatabaseBuilder.CurrentSchemaVersion));

        var reloadedConfiguration = new ConfigurationManager(fixture.DbFactory);
        await reloadedConfiguration.InitializeAsync();

        Assert.That(reloadedConfiguration.DBVersion, Is.EqualTo(LegacyBackupDatabaseBuilder.CurrentSchemaVersion));
        Assert.That(reloadedConfiguration.SourceFolder, Is.EqualTo(LegacyBackupDatabaseBuilder.SourceFolder));
        Assert.That(reloadedConfiguration.BackupFolder, Is.EqualTo(LegacyBackupDatabaseBuilder.BackupFolder));

        var queryManager = new QueryManager(fixture.DbFactory, reloadedConfiguration, new StorageFactoryMock());
        var versions = queryManager.GetVersions();

        Assert.That(versions, Has.Count.EqualTo(1));
        Assert.That(versions[0].Id, Is.EqualTo("1"));
        Assert.That(versions[0].Title, Is.EqualTo(LegacyBackupDatabaseBuilder.VersionTitle));
        Assert.That(versions[0].Description, Is.EqualTo(LegacyBackupDatabaseBuilder.VersionDescription));
        Assert.That(versions[0].CreationDate, Is.EqualTo(new DateTime(2021, 1, 1)));
    }

    [Test]
    public async Task InitializeAsync_ThrowsWhenDatabaseVersionIsNewerThanSupported()
    {
        await using var fixture = await SchemaUpgradeFixture.CreateAsync("99");

        var migration = new DbMigrationService(fixture.DbFactory, fixture.ConfigurationManager);

        Assert.ThrowsAsync<DatabaseIncompatibleException>(async () => await migration.InitializeAsync());
    }

    private sealed class SchemaUpgradeFixture : IAsyncDisposable
    {
        private SchemaUpgradeFixture(string directoryPath, IDbClientFactory dbFactory, ConfigurationManager configurationManager)
        {
            DirectoryPath = directoryPath;
            DbFactory = dbFactory;
            ConfigurationManager = configurationManager;
        }

        public string DirectoryPath { get; }

        public IDbClientFactory DbFactory { get; }

        public ConfigurationManager ConfigurationManager { get; }

        public static async Task<SchemaUpgradeFixture> CreateAsync(string schemaVersion)
        {
            var directoryPath = Path.Combine(Path.GetTempPath(), "BSH.Test", "schema-upgrade-" + Guid.NewGuid().ToString("N"));
            var databasePath = Path.Combine(directoryPath, "backupservicehome.bshdb");

            DbClientFactory.ClosePool();
            await LegacyBackupDatabaseBuilder.CreateAsync(databasePath, schemaVersion);

            var dbClientFactory = new DbClientFactory();
            await dbClientFactory.InitializeAsync(databasePath);

            var configurationManager = new ConfigurationManager(dbClientFactory);
            await configurationManager.InitializeAsync();

            return new SchemaUpgradeFixture(directoryPath, dbClientFactory, configurationManager);
        }

        public ValueTask DisposeAsync()
        {
            DbClientFactory.ClosePool();
            TempDirectory.DeleteBestEffort(DirectoryPath);
            return ValueTask.CompletedTask;
        }
    }
}

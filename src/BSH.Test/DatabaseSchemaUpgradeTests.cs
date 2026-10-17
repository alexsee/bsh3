// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using BSH.Test.Fakes;
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
        var before = await File.ReadAllBytesAsync(fixture.DatabasePath);

        var migration = new DbMigrationService(fixture.DbFactory, fixture.ConfigurationManager);

        Assert.ThrowsAsync<DatabaseIncompatibleException>(async () => await migration.InitializeAsync());
        Assert.That(Directory.GetFiles(fixture.DirectoryPath, "*.bak"), Is.Empty);
        Assert.That(await File.ReadAllBytesAsync(fixture.DatabasePath), Is.EqualTo(before));
        Assert.That(fixture.ConfigurationManager.DBVersion, Is.EqualTo("99"));
    }

    [Test]
    public async Task InitializeAsync_BacksUpDatabaseBeforeChangingSchema()
    {
        await using var fixture = await SchemaUpgradeFixture.CreateAsync("8");

        var migration = new DbMigrationService(fixture.DbFactory, fixture.ConfigurationManager);
        await migration.InitializeAsync();

        var backups = Directory.GetFiles(fixture.DirectoryPath, "*.bshdb.*.bak");
        Assert.That(backups, Has.Length.EqualTo(1));

        var backupFactory = new DbClientFactory();
        await backupFactory.InitializeAsync(backups[0]);
        var backupConfiguration = new ConfigurationManager(backupFactory);
        await backupConfiguration.InitializeAsync();

        Assert.That(backupConfiguration.DBVersion, Is.EqualTo("8"));
        Assert.That(fixture.ConfigurationManager.DBVersion, Is.EqualTo(LegacyBackupDatabaseBuilder.CurrentSchemaVersion));
    }

    [Test]
    public async Task InitializeAsync_DoesNotBackUpCurrentSchema()
    {
        await using var fixture = await SchemaUpgradeFixture.CreateAsync(LegacyBackupDatabaseBuilder.CurrentSchemaVersion);

        var migration = new DbMigrationService(fixture.DbFactory, fixture.ConfigurationManager);
        await migration.InitializeAsync();

        Assert.That(Directory.GetFiles(fixture.DirectoryPath, "*.bak"), Is.Empty);
        Assert.That(fixture.ConfigurationManager.DBVersion, Is.EqualTo(LegacyBackupDatabaseBuilder.CurrentSchemaVersion));
    }

    [Test]
    public async Task InitializeAsync_ThrowsWhenDatabaseVersionIsNotNumeric()
    {
        await using var fixture = await SchemaUpgradeFixture.CreateAsync("1");
        fixture.ConfigurationManager.DBVersion = "not-a-version";

        var migration = new DbMigrationService(fixture.DbFactory, fixture.ConfigurationManager);

        Assert.ThrowsAsync<DatabaseIncompatibleException>(async () => await migration.InitializeAsync());
    }

    [Test]
    public async Task InitializeAsync_StillBacksUpWhenTheCheckpointFails()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bsh-checkpoint-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = Path.Combine(directory, "backupservicehome.bshdb");
        await File.WriteAllTextAsync(database, "database");

        try
        {
            var configuration = new FakeConfigurationManager { DBVersion = "8" };
            var migration = new DbMigrationService(new ThrowingDbClientFactory(database), configuration);

            Assert.ThrowsAsync<IOException>(async () => await migration.InitializeAsync());

            Assert.That(Directory.GetFiles(directory, "*.bak"), Has.Length.EqualTo(1));
            Assert.That(configuration.DBVersion, Is.EqualTo("8"));
            Assert.That(await File.ReadAllTextAsync(database), Is.EqualTo("database"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class ThrowingDbClientFactory : IDbClientFactory
    {
        public ThrowingDbClientFactory(string databaseFile)
        {
            DatabaseFile = databaseFile;
        }

        public string DatabaseFile { get; }

        public DbClient CreateDbClient()
        {
            throw new IOException("database is busy");
        }

        public Task InitializeAsync(string databaseFile)
        {
            return Task.CompletedTask;
        }

        public Task ExecuteNonQueryAsync(string query)
        {
            return Task.CompletedTask;
        }
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

        public string DatabasePath => Path.Combine(DirectoryPath, "backupservicehome.bshdb");

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

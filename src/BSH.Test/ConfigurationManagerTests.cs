// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Security;
using NUnit.Framework;

namespace BSH.Test;
public class ConfigurationManagerTests
{
    private IDbClientFactory dbClientFactory;

    [SetUp]
    public async Task Setup()
    {
        DbClientFactory.ClosePool();

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_configurationmanager.db"));
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_configurationmanager.db"))
        {
            File.Delete("testdb_configurationmanager.db");
        }
    }

    [Test]
    public async Task LoadConfigurationTest()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        Assert.That(configurationManager.TaskType, Is.EqualTo(TaskType.Auto));
        Assert.That(configurationManager.MediumType, Is.EqualTo(MediaType.LocalDevice));
        Assert.That(configurationManager.SourceFolder, Is.Empty);
        Assert.That(configurationManager.BackupFolder, Is.Empty);
        Assert.That(configurationManager.FtpPort, Is.EqualTo("21"));
        Assert.That(configurationManager.Compression, Is.EqualTo(0));
        Assert.That(configurationManager.Encrypt, Is.EqualTo(0));
        Assert.That(configurationManager.IsConfigured, Is.EqualTo("0"));
    }

    [Test]
    public async Task SaveConfigurationTest()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.MediumType = MediaType.FileTransferServer;
        configurationManager.TaskType = TaskType.Manual;

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        Assert.That(configurationManager.MediumType, Is.EqualTo(MediaType.FileTransferServer));
        Assert.That(configurationManager.TaskType, Is.EqualTo(TaskType.Manual));
    }

    [Test]
    public async Task FtpPasswordIsProtectedAtRestAndReloadedAsPlaintext()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        configurationManager.FtpPass = "ftp-secret";

        var persistedPassword = await GetPersistedFtpPasswordAsync();
        Assert.That(persistedPassword, Is.Not.EqualTo("ftp-secret"));
        Assert.That(
            Crypto.DecryptString(persistedPassword, DataProtectionScope.LocalMachine),
            Is.EqualTo("ftp-secret"));

        var reloadedConfiguration = new ConfigurationManager(dbClientFactory);
        await reloadedConfiguration.InitializeAsync();

        Assert.That(reloadedConfiguration.FtpPass, Is.EqualTo("ftp-secret"));
    }

    [Test]
    public async Task LegacyPlaintextFtpPasswordIsMigratedAndRemainsUsable()
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO configuration (confValue, confProperty) VALUES ('legacy-secret', 'ftppass');");

        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        Assert.That(configurationManager.FtpPass, Is.EqualTo("legacy-secret"));

        var persistedPassword = await GetPersistedFtpPasswordAsync();
        Assert.That(persistedPassword, Is.Not.EqualTo("legacy-secret"));
        Assert.That(
            Crypto.DecryptString(persistedPassword, DataProtectionScope.LocalMachine),
            Is.EqualTo("legacy-secret"));
    }

    private async Task<string> GetPersistedFtpPasswordAsync()
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var persistedValue = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT confValue FROM configuration WHERE confProperty = @property",
            [("property", "ftppass")]);
        return persistedValue.ToString();
    }

    [Test]
    public async Task LoadConfigurationFromDirectDatabaseInsertTest()
    {
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO configuration (confValue, confProperty) VALUES ('FileTransferServer', 'mediumtype');");

        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        Assert.That(configurationManager.MediumType, Is.EqualTo(MediaType.FileTransferServer));
    }

    [Test]
    public async Task LoadConfigurationInvalidIntegerBackedValuesKeepDefaults()
    {
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO configuration (confValue, confProperty) VALUES ('invalid-task', 'tasktype');");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO configuration (confValue, confProperty) VALUES ('invalid-compression', 'compression');");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO configuration (confValue, confProperty) VALUES ('invalid-encrypt', 'encrypt');");

        var configurationManager = new ConfigurationManager(dbClientFactory);
        Assert.DoesNotThrowAsync(async () => await configurationManager.InitializeAsync());

        Assert.That(configurationManager.TaskType, Is.EqualTo(TaskType.Auto));
        Assert.That(configurationManager.Compression, Is.EqualTo(0));
        Assert.That(configurationManager.Encrypt, Is.EqualTo(0));
    }

    [Test]
    public async Task ReinitializeClearsStaleIsConfiguredWhenDatabaseIsRecreated()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.IsConfigured = "1";
        configurationManager.SourceFolder = @"C:\Users\alex\Documents";

        DbClientFactory.ClosePool();
        if (File.Exists("testdb_configurationmanager.db"))
        {
            File.Delete("testdb_configurationmanager.db");
        }

        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_configurationmanager.db"));
        await configurationManager.InitializeAsync();

        Assert.That(configurationManager.IsConfigured, Is.EqualTo("0"));
        Assert.That(configurationManager.SourceFolder, Is.Empty);
    }

    [Test]
    public async Task NullableBackedPropertiesPersistNullAsEmptyString()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        configurationManager.RemindSpace = "512";
        configurationManager.DbStatus = "1";
        configurationManager.DeativateAutoBackupsWhenAkku = "0";
        configurationManager.InfoBackupDone = "1";
        configurationManager.ShowWaitOnMediaAutoBackups = "1";

        // null assignments must not throw and persist as empty strings
        configurationManager.RemindSpace = null!;
        configurationManager.DbStatus = null!;
        configurationManager.DeativateAutoBackupsWhenAkku = null!;
        configurationManager.InfoBackupDone = null!;
        configurationManager.ShowWaitOnMediaAutoBackups = null!;

        var reloadedConfiguration = new ConfigurationManager(dbClientFactory);
        await reloadedConfiguration.InitializeAsync();

        Assert.That(reloadedConfiguration.RemindSpace, Is.Empty);
        Assert.That(reloadedConfiguration.DbStatus, Is.Empty);
        Assert.That(reloadedConfiguration.DeativateAutoBackupsWhenAkku, Is.Empty);
        Assert.That(reloadedConfiguration.InfoBackupDone, Is.Empty);
        Assert.That(reloadedConfiguration.ShowWaitOnMediaAutoBackups, Is.Empty);
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using NUnit.Framework;
using System.IO;
using Brightbits.BSH.Engine;

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

        Assert.AreEqual("", configurationManager.AutoBackup);
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

        Assert.AreEqual(MediaType.FileTransferServer, configurationManager.MediumType);
        Assert.AreEqual(TaskType.Manual, configurationManager.TaskType);
    }

    [Test]
    public async Task LoadConfigurationFromDirectDatabaseInsertTest()
    {
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO configuration (confValue, confProperty) VALUES ('FileTransferServer', 'mediumtype');");

        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        Assert.AreEqual(MediaType.FileTransferServer, configurationManager.MediumType);
    }
}

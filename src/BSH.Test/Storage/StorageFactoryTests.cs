// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;
using NUnit.Framework;

namespace BSH.Test.Storage;

public class StorageFactoryTests
{
    private IDbClientFactory dbClientFactory;
    private ConfigurationManager configurationManager;

    [SetUp]
    public async Task Setup()
    {
        DbClientFactory.ClosePool();

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_storagefactory.db"));

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_storagefactory.db"))
        {
            File.Delete("testdb_storagefactory.db");
        }
    }

    [TestCase(MediaType.LocalDevice, StorageProviderKind.LocalFileSystem)]
    [TestCase(MediaType.FileTransferServer, StorageProviderKind.Ftp)]
    [TestCase(MediaType.WebDav, StorageProviderKind.WebDav)]
    [TestCase(MediaType.Unset, StorageProviderKind.LocalFileSystem)]
    public void GetCurrentStorageProvider_SelectsProviderByMediumType(MediaType mediumType, StorageProviderKind expectedKind)
    {
        configurationManager.MediumType = mediumType;
        var factory = new StorageFactory(configurationManager);

        using var provider = factory.GetCurrentStorageProvider();

        Assert.That(provider.Kind, Is.EqualTo(expectedKind));
    }

    [Test]
    public async Task MediumType_WebDav_RoundTripsThroughConfiguration()
    {
        configurationManager.MediumType = MediaType.WebDav;

        var reloaded = new ConfigurationManager(dbClientFactory);
        await reloaded.InitializeAsync();

        Assert.That(reloaded.MediumType, Is.EqualTo(MediaType.WebDav));
    }
}

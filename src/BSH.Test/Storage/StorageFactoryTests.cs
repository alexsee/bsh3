// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test.Storage;

/// <summary>
/// Covers the typed <see cref="StorageFactory"/> provider selection.
/// </summary>
public class StorageFactoryTests
{
    [TestCase(MediaType.FileTransferServer, typeof(FtpStorage))]
    [TestCase(MediaType.LocalDevice, typeof(FileSystemStorage))]
    public void MediumSelectsExpectedStorageProvider(MediaType medium, Type expected)
    {
        var factory = new StorageFactory(new FakeConfigurationManager
        {
            MediumType = medium,
            BackupFolder = Path.GetTempPath(),
            FtpHost = "example.org",
            FtpPort = "21",
            FtpUser = "user",
            FtpPass = "pass",
            FtpFolder = "/backups",
            FtpCoding = "UTF-8",
            FtpEncryptionMode = "3",
            OldBackupPrevent = "7"
        });

        using var provider = factory.GetCurrentStorageProvider();

        Assert.That(provider, Is.InstanceOf(expected));
    }
}

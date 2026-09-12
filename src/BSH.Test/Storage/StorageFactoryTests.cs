// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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
    [Test]
    public void FtpMediumSelectsFtpStorage()
    {
        var factory = new StorageFactory(new FakeConfigurationManager
        {
            MediumType = MediaType.FileTransferServer,
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

        Assert.That(provider, Is.InstanceOf<FtpStorage>());
    }

    [Test]
    public void OtherMediaSelectLocalFileSystemStorage()
    {
        var factory = new StorageFactory(new FakeConfigurationManager
        {
            MediumType = MediaType.LocalDevice,
            BackupFolder = System.IO.Path.GetTempPath()
        });

        using var provider = factory.GetCurrentStorageProvider();

        Assert.That(provider, Is.InstanceOf<FileSystemStorage>());
    }
}

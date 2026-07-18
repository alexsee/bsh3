// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Utils;
using NUnit.Framework;

namespace BSH.Test.Utils;

public class BackupFileTypeTests
{
    [TestCase(StorageProviderKind.LocalFileSystem, false, false, BackupFileType.Local)]
    [TestCase(StorageProviderKind.LocalFileSystem, true, false, BackupFileType.LocalCompressed)]
    [TestCase(StorageProviderKind.LocalFileSystem, false, true, BackupFileType.LocalEncrypted)]
    [TestCase(StorageProviderKind.LocalFileSystem, true, true, BackupFileType.LocalEncrypted)]
    [TestCase(StorageProviderKind.Ftp, false, false, BackupFileType.Ftp)]
    [TestCase(StorageProviderKind.Ftp, true, false, BackupFileType.FtpCompressed)]
    [TestCase(StorageProviderKind.Ftp, false, true, BackupFileType.FtpEncrypted)]
    [TestCase(StorageProviderKind.Ftp, true, true, BackupFileType.FtpEncrypted)]
    [TestCase(StorageProviderKind.WebDav, false, false, BackupFileType.WebDav)]
    [TestCase(StorageProviderKind.WebDav, true, false, BackupFileType.WebDavCompressed)]
    [TestCase(StorageProviderKind.WebDav, false, true, BackupFileType.WebDavEncrypted)]
    [TestCase(StorageProviderKind.WebDav, true, true, BackupFileType.WebDavEncrypted)]
    public void GetFileType_ReturnsExpectedType(StorageProviderKind kind, bool compress, bool encrypt, int expected)
    {
        Assert.That(BackupFileType.GetFileType(kind, compress, encrypt), Is.EqualTo(expected));
    }

    [TestCase(BackupFileType.Local)]
    [TestCase(BackupFileType.Ftp)]
    [TestCase(BackupFileType.WebDav)]
    public void IsRegular_ReturnsTrueForRegularTypes(int fileType)
    {
        Assert.That(BackupFileType.IsRegular(fileType), Is.True);
        Assert.That(BackupFileType.IsCompressed(fileType), Is.False);
        Assert.That(BackupFileType.IsEncrypted(fileType), Is.False);
        Assert.That(BackupFileType.IsKnown(fileType), Is.True);
    }

    [TestCase(BackupFileType.LocalCompressed)]
    [TestCase(BackupFileType.FtpCompressed)]
    [TestCase(BackupFileType.WebDavCompressed)]
    public void IsCompressed_ReturnsTrueForCompressedTypes(int fileType)
    {
        Assert.That(BackupFileType.IsCompressed(fileType), Is.True);
        Assert.That(BackupFileType.IsRegular(fileType), Is.False);
        Assert.That(BackupFileType.IsEncrypted(fileType), Is.False);
        Assert.That(BackupFileType.IsKnown(fileType), Is.True);
    }

    [TestCase(BackupFileType.LocalEncrypted)]
    [TestCase(BackupFileType.FtpEncrypted)]
    [TestCase(BackupFileType.WebDavEncrypted)]
    public void IsEncrypted_ReturnsTrueForEncryptedTypes(int fileType)
    {
        Assert.That(BackupFileType.IsEncrypted(fileType), Is.True);
        Assert.That(BackupFileType.IsRegular(fileType), Is.False);
        Assert.That(BackupFileType.IsCompressed(fileType), Is.False);
        Assert.That(BackupFileType.IsKnown(fileType), Is.True);
    }

    [Test]
    public void IsKnown_ReturnsFalseForUnknownType()
    {
        Assert.That(BackupFileType.IsKnown(0), Is.False);
        Assert.That(BackupFileType.IsKnown(99), Is.False);
    }
}

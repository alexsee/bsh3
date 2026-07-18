// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Security.Cryptography;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class MediaTargetApplierTests
{
    [TestCase(@"\\server\share", true)]
    [TestCase(@"\\server\share\backups", true)]
    [TestCase(@"//server/share", true)]
    [TestCase(@"C:\Backups", false)]
    [TestCase(@"D:\", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase(@"\", false)]
    public void IsUncPath_DetectsUncPaths(string? path, bool expected)
    {
        Assert.That(MediaTargetApplier.IsUncPath(path), Is.EqualTo(expected));
    }

    [Test]
    public void ApplyUncTarget_SetsFolderCredentialsAndClearsVolumeSerial()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.FileTransferServer,
            BackupFolder = @"E:\OldBackups",
            MediaVolumeSerial = "1234567890",
            UNCUsername = "old-user",
            UNCPassword = "old-password",
        };

        MediaTargetApplier.ApplyUncTarget(
            configuration,
            @"\\nas\backups\machine",
            "domain\\backup",
            "secret");

        Assert.That(configuration.MediumType, Is.EqualTo(MediaType.LocalDevice));
        Assert.That(configuration.BackupFolder, Is.EqualTo(@"\\nas\backups\machine"));
        Assert.That(configuration.MediaVolumeSerial, Is.EqualTo(""));
        Assert.That(configuration.UNCUsername, Is.EqualTo(@"domain\backup"));
        Assert.That(
            Crypto.DecryptString(configuration.UNCPassword, DataProtectionScope.LocalMachine),
            Is.EqualTo("secret"));
    }

    [Test]
    public void ApplyUncTarget_NormalizesForwardSlashesAndAllowsEmptyCredentials()
    {
        var configuration = new FakeConfigurationManager
        {
            MediaVolumeSerial = "999",
            UNCUsername = "keep-me",
            UNCPassword = "keep-me",
        };

        MediaTargetApplier.ApplyUncTarget(configuration, "//fileserver/share", null, null);

        Assert.That(configuration.BackupFolder, Is.EqualTo(@"\\fileserver\share"));
        Assert.That(configuration.MediaVolumeSerial, Is.EqualTo(""));
        Assert.That(configuration.UNCUsername, Is.EqualTo(""));
        Assert.That(configuration.UNCPassword, Is.EqualTo(""));
    }

    [Test]
    public void ApplyUncTarget_ThrowsWhenPathMissing()
    {
        var configuration = new FakeConfigurationManager();

        Assert.Throws<ArgumentException>(() =>
            MediaTargetApplier.ApplyUncTarget(configuration, "   ", "user", "pass"));
    }

    [Test]
    public void ApplyUncTarget_ThrowsWhenPathIsNotUnc()
    {
        var configuration = new FakeConfigurationManager();

        Assert.Throws<ArgumentException>(() =>
            MediaTargetApplier.ApplyUncTarget(configuration, @"C:\Backups", "user", "pass"));
    }

    [Test]
    public void ApplyLocalTarget_SetsFolderSerialAndClearsUncCredentials()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.FileTransferServer,
            UNCUsername = "old-user",
            UNCPassword = "old-password",
            MediaVolumeSerial = "keep-me",
        };

        MediaTargetApplier.ApplyLocalTarget(configuration, @"D:\Backups\PC\User", "ABCDEF12");

        Assert.That(configuration.MediumType, Is.EqualTo(MediaType.LocalDevice));
        Assert.That(configuration.BackupFolder, Is.EqualTo(@"D:\Backups\PC\User"));
        Assert.That(configuration.MediaVolumeSerial, Is.EqualTo("ABCDEF12"));
        Assert.That(configuration.UNCUsername, Is.EqualTo(""));
        Assert.That(configuration.UNCPassword, Is.EqualTo(""));
    }

    [Test]
    public void ApplyLocalTarget_NormalizesZeroSerial()
    {
        var configuration = new FakeConfigurationManager();

        MediaTargetApplier.ApplyLocalTarget(configuration, @"E:\Backups", "0");

        Assert.That(configuration.MediaVolumeSerial, Is.EqualTo(""));
    }

    [Test]
    public void ApplyFtpTarget_SetsFtpFieldsAndClearsLocalUncState()
    {
        var configuration = new FakeConfigurationManager
        {
            BackupFolder = @"C:\Backups",
            MediaVolumeSerial = "123",
            UNCUsername = "user",
            UNCPassword = "pass",
        };

        MediaTargetApplier.ApplyFtpTarget(
            configuration,
            "ftp.example.com",
            "2121",
            "ftp-user",
            "ftp-pass",
            "/backups",
            "UTF8",
            encryptionMode: "0",
            sslProtocols: "0");

        Assert.That(configuration.MediumType, Is.EqualTo(MediaType.FileTransferServer));
        Assert.That(configuration.BackupFolder, Is.EqualTo(""));
        Assert.That(configuration.MediaVolumeSerial, Is.EqualTo(""));
        Assert.That(configuration.UNCUsername, Is.EqualTo(""));
        Assert.That(configuration.UNCPassword, Is.EqualTo(""));
        Assert.That(configuration.FtpHost, Is.EqualTo("ftp.example.com"));
        Assert.That(configuration.FtpPort, Is.EqualTo("2121"));
        Assert.That(configuration.FtpUser, Is.EqualTo("ftp-user"));
        Assert.That(configuration.FtpPass, Is.EqualTo("ftp-pass"));
        Assert.That(configuration.FtpFolder, Is.EqualTo("/backups"));
        Assert.That(configuration.FtpCoding, Is.EqualTo("UTF8"));
        Assert.That(configuration.FtpEncryptionMode, Is.EqualTo("0"));
        Assert.That(configuration.FtpSslProtocols, Is.EqualTo("0"));
    }

    [TestCase(@"\\server\share\path", null)]
    [TestCase(@"C:\Backups\User", @"C:\")]
    [TestCase(@"D:\", @"D:\")]
    [TestCase("ab", null)]
    public void GetVolumeSerialRoot_SkipsUncAndShortPaths(string path, string? expected)
    {
        Assert.That(MediaTargetApplier.GetVolumeSerialRoot(path), Is.EqualTo(expected));
    }

    [Test]
    public void BuildLocalBackupFolder_CombinesDriveRootMachineAndUser()
    {
        var folder = MediaTargetApplier.BuildLocalBackupFolder(@"E:\");

        Assert.That(folder, Does.StartWith(@"E:\Backups\"));
        Assert.That(folder, Does.Contain(Environment.MachineName));
        Assert.That(folder, Does.EndWith(Environment.UserName).IgnoreCase);
    }
}

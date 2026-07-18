// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services;
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

    [TestCase(@"\\server\share\path", null)]
    [TestCase(@"C:\Backups\User", @"C:\")]
    [TestCase(@"D:\", @"D:\")]
    [TestCase("ab", null)]
    public void GetVolumeSerialRoot_SkipsUncAndShortPaths(string path, string? expected)
    {
        Assert.That(MediaTargetApplier.GetVolumeSerialRoot(path), Is.EqualTo(expected));
    }

    private sealed class FakeConfigurationManager : IConfigurationManager
    {
        public string AutoBackup { get; set; } = "";
        public string BackupFolder { get; set; } = "";
        public string BackupSize { get; set; } = "";
        public int Compression { get; set; }
        public string DbStatus { get; set; } = "";
        public string DBVersion { get; set; } = "";
        public string DeativateAutoBackupsWhenAkku { get; set; } = "";
        public string DoPastBackups { get; set; } = "";
        public int Encrypt { get; set; }
        public string EncryptPassMD5 { get; set; } = "";
        public string ExcludeCompression { get; set; } = "";
        public string ExcludeFile { get; set; } = "";
        public string ExcludeFileBigger { get; set; } = "";
        public string ExcludeFileTypes { get; set; } = "";
        public string ExcludeFolder { get; set; } = "";
        public string ExcludeMask { get; set; } = "";
        public string FreeSpace { get; set; } = "";
        public string FtpCoding { get; set; } = "";
        public string FtpEncryptionMode { get; set; } = "";
        public string FtpFolder { get; set; } = "";
        public string FtpHost { get; set; } = "";
        public string FtpPass { get; set; } = "";
        public string FtpPort { get; set; } = "";
        public string FtpSslProtocols { get; set; } = "";
        public string FtpUser { get; set; } = "";
        public string InfoBackupDone { get; set; } = "";
        public string IntervallAutoHourBackups { get; set; } = "";
        public string IntervallDelete { get; set; } = "";
        public string IsConfigured { get; set; } = "";
        public string LastBackupDone { get; set; } = "";
        public string LastVersionDate { get; set; } = "";
        public string MediaVolumeSerial { get; set; } = "";
        public string Medium { get; set; } = "";
        public MediaType MediumType { get; set; }
        public string OldBackupPrevent { get; set; } = "";
        public string RemindAfterDays { get; set; } = "";
        public string RemindSpace { get; set; } = "";
        public string ScheduleFullBackup { get; set; } = "";
        public string ShowLocalizedPath { get; set; } = "";
        public string ShowWaitOnMediaAutoBackups { get; set; } = "";
        public string SourceFolder { get; set; } = "";
        public TaskType TaskType { get; set; }
        public string UNCPassword { get; set; } = "";
        public string UNCUsername { get; set; } = "";

        public Task InitializeAsync() => Task.CompletedTask;
    }
}

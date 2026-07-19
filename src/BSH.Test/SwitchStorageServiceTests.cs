// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Security;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using NUnit.Framework;

namespace BSH.Test;

public class SwitchStorageServiceTests
{
    private const string DatabaseFileName = "testdb_switch_storage.db";

    private string databasePath;
    private string tempRoot;
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private RecordingBackupService backupService;
    private PathOnlySetupService setupService;
    private ISwitchStorageService switchStorageService;

    [SetUp]
    public async Task Setup()
    {
        DbClientFactory.ClosePool();

        tempRoot = Path.Combine(Path.GetTempPath(), "bsh-switch-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        databasePath = Path.Combine(tempRoot, DatabaseFileName);

        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(databasePath);

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = @"D:\OldBackups";
        configurationManager.MediaVolumeSerial = "OLDVOL";
        configurationManager.FtpHost = "old.example";
        configurationManager.FtpPort = "21";
        configurationManager.FtpUser = "olduser";
        configurationManager.FtpPass = "oldpass";
        configurationManager.FtpFolder = "/old";
        configurationManager.FtpCoding = "UTF8";

        backupService = new RecordingBackupService();
        setupService = new PathOnlySetupService();
        switchStorageService = new SwitchStorageService(configurationManager, dbClientFactory, backupService, setupService);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();

        if (Directory.Exists(tempRoot))
        {
            try
            {
                Directory.Delete(tempRoot, true);
            }
            catch
            {
                // Best-effort cleanup for locked temp files.
            }
        }
    }

    [Test]
    public void LocalTargetContainsBackupDataReturnsTrueWhenBackupDatabaseExists()
    {
        var driveRoot = Path.Combine(tempRoot, "F") + Path.DirectorySeparatorChar;
        var backupFolder = setupService.BuildLocalBackupFolder(driveRoot);
        Directory.CreateDirectory(backupFolder);
        File.WriteAllText(Path.Combine(backupFolder, "backup.bshdb"), "existing");

        Assert.That(switchStorageService.LocalTargetContainsBackupData(driveRoot), Is.True);
    }

    [Test]
    public void LocalTargetContainsBackupDataReturnsFalseWhenTargetIsEmpty()
    {
        var driveRoot = Path.Combine(tempRoot, "G") + Path.DirectorySeparatorChar;

        Assert.That(switchStorageService.LocalTargetContainsBackupData(driveRoot), Is.False);
    }

    [Test]
    public async Task SwitchToLocalAsyncUpdatesConfigurationClearsHistoryAndSyncsDatabase()
    {
        await SeedBackupHistoryAsync();

        var driveRoot = Path.Combine(tempRoot, "H") + Path.DirectorySeparatorChar;
        var expectedFolder = setupService.BuildLocalBackupFolder(driveRoot);

        await switchStorageService.SwitchToLocalAsync(driveRoot, "NEVol", databasePath);

        Assert.That(configurationManager.MediumType, Is.EqualTo(MediaType.LocalDevice));
        Assert.That(configurationManager.BackupFolder, Is.EqualTo(expectedFolder));
        Assert.That(configurationManager.MediaVolumeSerial, Is.EqualTo("NEVol"));
        Assert.That(configurationManager.UNCUsername, Is.EqualTo(""));
        Assert.That(configurationManager.UNCPassword, Is.EqualTo(""));

        Assert.That(await CountAsync("versiontable"), Is.EqualTo(0));
        Assert.That(await CountAsync("filelink"), Is.EqualTo(0));
        Assert.That(await CountAsync("fileversiontable"), Is.EqualTo(0));

        Assert.That(backupService.UpdatedDatabaseFiles, Is.EqualTo(new[] { databasePath }));
    }

    [Test]
    public async Task SwitchToLocalAsyncClearsZeroVolumeSerial()
    {
        var driveRoot = Path.Combine(tempRoot, "I") + Path.DirectorySeparatorChar;

        await switchStorageService.SwitchToLocalAsync(driveRoot, "0", databasePath);

        Assert.That(configurationManager.MediaVolumeSerial, Is.EqualTo(""));
    }

    [Test]
    public void UncTargetContainsBackupDataReturnsTrueWhenBackupDatabaseExists()
    {
        var uncPath = Path.Combine(tempRoot, "unc-share");
        Directory.CreateDirectory(uncPath);
        File.WriteAllText(Path.Combine(uncPath, "backup.bshdb"), "existing");

        Assert.That(switchStorageService.UncTargetContainsBackupData(uncPath), Is.True);
    }

    [Test]
    public void UncTargetContainsBackupDataReturnsFalseWhenTargetIsEmpty()
    {
        var uncPath = Path.Combine(tempRoot, "unc-empty");
        Directory.CreateDirectory(uncPath);

        Assert.That(switchStorageService.UncTargetContainsBackupData(uncPath), Is.False);
    }

    [Test]
    public async Task SwitchToUncAsyncUpdatesConfigurationClearsHistoryAndSyncsDatabase()
    {
        await SeedBackupHistoryAsync();

        configurationManager.MediaVolumeSerial = "OLDVOL";
        configurationManager.UNCUsername = "old-user";
        configurationManager.UNCPassword = "old-password";
        configurationManager.FtpHost = "ftp.old";

        var unc = new SwitchStorageUncTarget(
            Path: @"\\fileserver\backups",
            Username: @"domain\backup",
            Password: "secret");

        await switchStorageService.SwitchToUncAsync(unc, databasePath);

        Assert.That(configurationManager.MediumType, Is.EqualTo(MediaType.LocalDevice));
        Assert.That(configurationManager.BackupFolder, Is.EqualTo(@"\\fileserver\backups"));
        Assert.That(configurationManager.MediaVolumeSerial, Is.EqualTo(""));
        Assert.That(configurationManager.UNCUsername, Is.EqualTo(@"domain\backup"));
        Assert.That(
            Crypto.DecryptString(configurationManager.UNCPassword, DataProtectionScope.LocalMachine),
            Is.EqualTo("secret"));
        Assert.That(configurationManager.FtpHost, Is.EqualTo(""));

        Assert.That(await CountAsync("versiontable"), Is.EqualTo(0));
        Assert.That(await CountAsync("filelink"), Is.EqualTo(0));
        Assert.That(await CountAsync("fileversiontable"), Is.EqualTo(0));

        Assert.That(backupService.UpdatedDatabaseFiles, Is.EqualTo(new[] { databasePath }));
    }

    [Test]
    public async Task SwitchToFtpAsyncUpdatesConfigurationClearsHistoryAndSyncsDatabase()
    {
        await SeedBackupHistoryAsync();

        var ftp = new SwitchStorageFtpTarget(
            Host: "ftp.example",
            Port: "2121",
            User: "backup",
            Password: "secret",
            Folder: "/backups",
            Encoding: "ISO-8859-1",
            EnforceUnencrypted: true);

        await switchStorageService.SwitchToFtpAsync(ftp, databasePath);

        Assert.That(configurationManager.MediumType, Is.EqualTo(MediaType.FileTransferServer));
        Assert.That(configurationManager.BackupFolder, Is.EqualTo(""));
        Assert.That(configurationManager.FtpHost, Is.EqualTo("ftp.example"));
        Assert.That(configurationManager.FtpPort, Is.EqualTo("2121"));
        Assert.That(configurationManager.FtpUser, Is.EqualTo("backup"));
        Assert.That(configurationManager.FtpPass, Is.EqualTo("secret"));
        Assert.That(configurationManager.FtpFolder, Is.EqualTo("/backups"));
        Assert.That(configurationManager.FtpCoding, Is.EqualTo("ISO-8859-1"));
        Assert.That(configurationManager.FtpEncryptionMode, Is.EqualTo("0"));
        Assert.That(configurationManager.FtpSslProtocols, Is.EqualTo("0"));

        Assert.That(await CountAsync("versiontable"), Is.EqualTo(0));
        Assert.That(await CountAsync("filelink"), Is.EqualTo(0));
        Assert.That(await CountAsync("fileversiontable"), Is.EqualTo(0));

        Assert.That(backupService.UpdatedDatabaseFiles, Is.EqualTo(new[] { databasePath }));
    }

    [Test]
    public void SyncDatabaseToCurrentMediumDelegatesToBackupService()
    {
        switchStorageService.SyncDatabaseToCurrentMedium(databasePath);

        Assert.That(backupService.UpdatedDatabaseFiles, Is.EqualTo(new[] { databasePath }));
    }

    private async Task SeedBackupHistoryAsync()
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionStable, versionSources, versionStatus, versionType) " +
            "VALUES (1, '01-01-2021 00-00-00', '1.0.0', 'seed', 1, 'C:\\Source', 0, 2)");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO filetable (fileID, fileName, filePath) VALUES (1, 'a.txt', 'C:\\Source\\')");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID) " +
            "VALUES (1, 0, 1, 'hash1', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 100, 1, '1')");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO filelink (fileversionID, versionID) VALUES (1, 1)");
    }

    private async Task<int> CountAsync(string tableName)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        return Convert.ToInt32(await dbClient.ExecuteScalarAsync($"SELECT COUNT(*) FROM {tableName}"));
    }

    /// <summary>
    /// Mirrors SetupService.BuildLocalBackupFolder without pulling in unused setup dependencies.
    /// </summary>
    private sealed class PathOnlySetupService : ISetupService
    {
        public string BuildLocalBackupFolder(string driveRoot)
        {
            var root = driveRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Path.Combine(root, "Backups", Environment.MachineName, Environment.UserName);
        }

        public string? GetDefaultSourceFolder() => throw new NotSupportedException();
        public bool TryAddSourceFolder(IList<string> sources, string folderPath, out string? error) => throw new NotSupportedException();
        public bool IsLocalBackupFolderAvailable(string backupFolder) => throw new NotSupportedException();
        public void ApplyNewConfiguration(NewSetupConfiguration configuration) => throw new NotSupportedException();
        public IReadOnlyList<DiscoveredBackup> DiscoverBackupsOnDrive(string driveRoot) => throw new NotSupportedException();
        public bool BackupDatabaseExists(string folderPath) => throw new NotSupportedException();
        public bool CanRemapSourcePath(string originalPath, string newPath, out string? error) => throw new NotSupportedException();
        public void ReplaceDatabaseWithCopy(string sourceDatabasePath, string destinationDatabasePath) => throw new NotSupportedException();
        public void PrepareDatabaseReplacement(string destinationDatabasePath) => throw new NotSupportedException();
        public Task RemapSourcesAsync(IReadOnlyList<SourceRemap> remaps) => throw new NotSupportedException();
        public Task ConvertFileTypesForLocalImportAsync() => throw new NotSupportedException();
        public Task ConvertFileTypesForFtpImportAsync() => throw new NotSupportedException();
    }

    private sealed class RecordingBackupService : IBackupService
    {
        public List<string> UpdatedDatabaseFiles { get; } = [];

        public Task<bool> CheckMedia(bool quickCheck = false) => Task.FromResult(true);
        public string GetPassword() => string.Empty;
        public bool HasPassword() => false;
        public void SetPassword(string password) { }
        public Task SetStableAsync(string version, bool stable) => Task.CompletedTask;
        public Task UpdateVersionAsync(string version, VersionDetails versionDetails) => Task.CompletedTask;
        public Task StartBackup(string title, string description, IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false) => Task.CompletedTask;
        public Task StartDelete(string version, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartDeleteSingle(string fileFilter, string pathFilter, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartEdit(IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartRestore(string version, string file, string destination, IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => Task.CompletedTask;

        public void UpdateDatabaseFile(string databaseFile)
        {
            UpdatedDatabaseFiles.Add(databaseFile);
        }
    }
}

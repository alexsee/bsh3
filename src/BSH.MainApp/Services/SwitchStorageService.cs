// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Services;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;

namespace BSH.MainApp.Services;

public sealed class SwitchStorageService : ISwitchStorageService
{
    private readonly IConfigurationManager configurationManager;
    private readonly IDbClientFactory dbClientFactory;
    private readonly IBackupService backupService;
    private readonly ISetupService setupService;

    public SwitchStorageService(
        IConfigurationManager configurationManager,
        IDbClientFactory dbClientFactory,
        IBackupService backupService,
        ISetupService setupService)
    {
        this.configurationManager = configurationManager;
        this.dbClientFactory = dbClientFactory;
        this.backupService = backupService;
        this.setupService = setupService;
    }

    public bool LocalTargetContainsBackupData(string driveRoot)
    {
        var backupFolder = setupService.BuildLocalBackupFolder(driveRoot);
        return File.Exists(Path.Combine(backupFolder, "backup.bshdb"));
    }

    public bool UncTargetContainsBackupData(string uncPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uncPath);

        var normalized = uncPath.Replace('/', '\\');
        return File.Exists(Path.Combine(normalized, "backup.bshdb"));
    }

    public void SyncDatabaseToCurrentMedium(string databaseFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);
        backupService.UpdateDatabaseFile(databaseFile);
    }

    public async Task SwitchToLocalAsync(string driveRoot, string? mediaVolumeSerial, string databaseFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driveRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);

        var backupFolder = setupService.BuildLocalBackupFolder(driveRoot);
        MediaTargetApplier.ApplyLocalTarget(configurationManager, backupFolder, mediaVolumeSerial);

        await ClearBackupHistoryAsync();
        SyncDatabaseToCurrentMedium(databaseFile);
    }

    public async Task SwitchToUncAsync(SwitchStorageUncTarget unc, string databaseFile)
    {
        ArgumentNullException.ThrowIfNull(unc);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);

        MediaTargetApplier.ApplyUncTarget(configurationManager, unc.Path, unc.Username, unc.Password);

        await ClearBackupHistoryAsync();
        SyncDatabaseToCurrentMedium(databaseFile);
    }

    public async Task SwitchToFtpAsync(SwitchStorageFtpTarget ftp, string databaseFile)
    {
        ArgumentNullException.ThrowIfNull(ftp);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);

        MediaTargetApplier.ApplyFtpTarget(
            configurationManager,
            ftp.Host,
            ftp.Port,
            ftp.User,
            ftp.Password,
            ftp.Folder,
            string.IsNullOrWhiteSpace(ftp.Encoding) ? "UTF8" : ftp.Encoding,
            encryptionMode: ftp.EnforceUnencrypted ? "0" : "3",
            sslProtocols: "0");

        await ClearBackupHistoryAsync();
        SyncDatabaseToCurrentMedium(databaseFile);
    }

    private async Task ClearBackupHistoryAsync()
    {
        await dbClientFactory.ExecuteNonQueryAsync("DELETE FROM versiontable");
        await dbClientFactory.ExecuteNonQueryAsync("DELETE FROM filelink");
        await dbClientFactory.ExecuteNonQueryAsync("DELETE FROM fileversiontable");
    }
}

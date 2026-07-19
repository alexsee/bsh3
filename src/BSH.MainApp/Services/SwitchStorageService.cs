// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
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

        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = backupFolder;
        configurationManager.MediaVolumeSerial = string.IsNullOrEmpty(mediaVolumeSerial) || mediaVolumeSerial == "0"
            ? ""
            : mediaVolumeSerial;
        configurationManager.UNCUsername = "";
        configurationManager.UNCPassword = "";

        await ClearBackupHistoryAsync();
        SyncDatabaseToCurrentMedium(databaseFile);
    }

    public async Task SwitchToFtpAsync(SwitchStorageFtpTarget ftp, string databaseFile)
    {
        ArgumentNullException.ThrowIfNull(ftp);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);

        configurationManager.MediumType = MediaType.FileTransferServer;
        configurationManager.BackupFolder = "";
        configurationManager.FtpHost = ftp.Host ?? "";
        configurationManager.FtpPort = string.IsNullOrWhiteSpace(ftp.Port) ? "21" : ftp.Port;
        configurationManager.FtpUser = ftp.User ?? "";
        configurationManager.FtpPass = ftp.Password ?? "";
        configurationManager.FtpFolder = ftp.Folder ?? "";
        configurationManager.FtpCoding = string.IsNullOrWhiteSpace(ftp.Encoding) ? "UTF8" : ftp.Encoding;
        configurationManager.FtpEncryptionMode = ftp.EnforceUnencrypted ? "0" : "3";
        configurationManager.FtpSslProtocols = "0";

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

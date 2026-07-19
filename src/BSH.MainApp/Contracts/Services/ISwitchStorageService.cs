// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface ISwitchStorageService
{
    bool LocalTargetContainsBackupData(string driveRoot);

    void SyncDatabaseToCurrentMedium(string databaseFile);

    Task SwitchToLocalAsync(string driveRoot, string? mediaVolumeSerial, string databaseFile);

    Task SwitchToFtpAsync(SwitchStorageFtpTarget ftp, string databaseFile);
}

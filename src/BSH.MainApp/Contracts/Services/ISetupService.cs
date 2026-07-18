// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface ISetupService
{
    string? GetDefaultSourceFolder();

    bool TryAddSourceFolder(IList<string> sources, string folderPath, out string? error);

    string BuildLocalBackupFolder(string driveRoot);

    bool IsLocalBackupFolderAvailable(string backupFolder);

    void ApplyNewConfiguration(NewSetupConfiguration configuration);

    IReadOnlyList<DiscoveredBackup> DiscoverBackupsOnDrive(string driveRoot);

    bool BackupDatabaseExists(string folderPath);

    bool CanRemapSourcePath(string originalPath, string newPath, out string? error);

    void ReplaceDatabaseWithCopy(string sourceDatabasePath, string destinationDatabasePath);

    void PrepareDatabaseReplacement(string destinationDatabasePath);

    Task RemapSourcesAsync(IReadOnlyList<SourceRemap> remaps);

    Task ConvertFileTypesForLocalImportAsync();

    Task ConvertFileTypesForFtpImportAsync();

    Task ConvertFileTypesForWebDavImportAsync();
}

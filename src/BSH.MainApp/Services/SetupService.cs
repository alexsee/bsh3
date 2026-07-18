// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Security.Cryptography;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Security;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
using BSH.MainApp.Models;

namespace BSH.MainApp.Services;

public class SetupService : ISetupService
{
    private readonly IConfigurationManager configurationManager;
    private readonly IQueryManager queryManager;
    private readonly IDbClientFactory dbClientFactory;

    public SetupService(
        IConfigurationManager configurationManager,
        IQueryManager queryManager,
        IDbClientFactory dbClientFactory)
    {
        this.configurationManager = configurationManager;
        this.queryManager = queryManager;
        this.dbClientFactory = dbClientFactory;
    }

    public string? GetDefaultSourceFolder()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return documents.Length > 2 ? documents : null;
    }

    public bool TryAddSourceFolder(IList<string> sources, string folderPath, out string? error)
    {
        error = null;

        if (!PathRules.TryNormalizeFolderPath(folderPath, out var fullPath))
        {
            error = "Selected source folder path is invalid.";
            return false;
        }

        if (PathRules.IsDriveRoot(fullPath))
        {
            error = "Selecting a drive root is risky. Choose a specific folder instead.";
            return false;
        }

        var folderName = Path.GetFileName(fullPath);
        if (sources.Any(source =>
            string.Equals(
                Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                folderName,
                StringComparison.OrdinalIgnoreCase)))
        {
            error = "A source folder with the same name is already configured.";
            return false;
        }

        sources.Add(fullPath);
        return true;
    }

    public string BuildLocalBackupFolder(string driveRoot)
    {
        var root = driveRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return Path.Combine(root, "Backups", Environment.MachineName, Environment.UserName);
    }

    public bool IsLocalBackupFolderAvailable(string backupFolder)
    {
        return !Directory.Exists(backupFolder);
    }

    public void ApplyNewConfiguration(NewSetupConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configurationManager.TaskType = configuration.TaskType;
        configurationManager.SourceFolder = string.Join("|", configuration.SourceFolders);
        configurationManager.Medium = "1";

        switch (configuration.TargetKind)
        {
            case SetupTargetKind.LocalDrive:
                ApplyLocalTarget(configuration.LocalBackupFolder, configuration.MediaVolumeSerial, createFolder: true);
                break;
            case SetupTargetKind.Unc:
                ApplyUncTarget(configuration.UncPath, configuration.UncUsername, configuration.UncPassword);
                break;
            case SetupTargetKind.Ftp:
                ApplyFtpTarget(configuration);
                break;
            case SetupTargetKind.WebDav:
                ApplyWebDavTarget(configuration);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(configuration), configuration.TargetKind, "Unsupported setup target.");
        }

        configurationManager.IsConfigured = "1";
    }

    public IReadOnlyList<DiscoveredBackup> DiscoverBackupsOnDrive(string driveRoot)
    {
        var results = new List<DiscoveredBackup>();
        var backupsRoot = Path.Combine(
            driveRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            "Backups");

        if (!Directory.Exists(backupsRoot))
        {
            return results;
        }

        foreach (var computerDirectory in Directory.GetDirectories(backupsRoot))
        {
            var computerName = Path.GetFileName(computerDirectory);
            foreach (var userDirectory in Directory.GetDirectories(computerDirectory))
            {
                if (!BackupDatabaseExists(userDirectory))
                {
                    continue;
                }

                results.Add(new DiscoveredBackup
                {
                    ComputerName = computerName,
                    UserName = Path.GetFileName(userDirectory),
                    FolderPath = userDirectory
                });
            }
        }

        return results;
    }

    public bool BackupDatabaseExists(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return false;
        }

        return File.Exists(Path.Combine(folderPath, "backup.bshdb"));
    }

    public bool CanRemapSourcePath(string originalPath, string newPath, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(originalPath) || string.IsNullOrWhiteSpace(newPath))
        {
            error = "Source path cannot be empty.";
            return false;
        }

        var originalName = Path.GetFileName(originalPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var newName = Path.GetFileName(newPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        if (string.Equals(originalName, newName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (IsDriveRootPath(originalPath) && IsDriveRootPath(newPath))
        {
            return true;
        }

        error = "Cannot change the directory because the folder name does not match.";
        return false;
    }

    public static string ApplySourceRemaps(string versionSources, IReadOnlyList<SourceRemap> remaps)
    {
        var sources = versionSources
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(path => new VersionEntry(path, false))
            .ToList();

        foreach (var remap in remaps)
        {
            if (remap.CurrentPath.Length <= 3)
            {
                continue;
            }

            foreach (var source in sources)
            {
                source.Path = source.Path.Replace(remap.OriginalPath, remap.CurrentPath);
                source.Changed = true;
            }
        }

        foreach (var remap in remaps)
        {
            if (remap.CurrentPath.Length <= 3)
            {
                continue;
            }

            foreach (var source in sources.Where(entry => !entry.Changed))
            {
                source.Path = source.Path.Replace(remap.OriginalPath, remap.CurrentPath);
            }
        }

        return string.Join("|", sources.Select(entry => entry.Path));
    }

    public void ReplaceDatabaseWithCopy(string sourceDatabasePath, string destinationDatabasePath)
    {
        PrepareDatabaseReplacement(destinationDatabasePath);
        File.Copy(sourceDatabasePath, destinationDatabasePath, true);
    }

    public void PrepareDatabaseReplacement(string destinationDatabasePath)
    {
        DbClientFactory.ClosePool();
        DeleteDatabaseFiles(destinationDatabasePath);

        var destinationDirectory = Path.GetDirectoryName(destinationDatabasePath);
        if (!string.IsNullOrEmpty(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }
    }

    public async Task RemapSourcesAsync(IReadOnlyList<SourceRemap> remaps)
    {
        foreach (var version in queryManager.GetVersions())
        {
            var remapped = ApplySourceRemaps(version.Sources ?? string.Empty, remaps);
            await dbClientFactory.ExecuteNonQueryAsync(
                "UPDATE versiontable SET versionSources = \"" + EscapeSql(remapped) + "\" WHERE versionID = " + version.Id);
        }

        configurationManager.SourceFolder = string.Join("|", remaps.Select(remap => remap.CurrentPath));
        configurationManager.TaskType = TaskType.Manual;
    }

    public async Task ConvertFileTypesForLocalImportAsync()
    {
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 1 WHERE fileType = 3");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 2 WHERE fileType = 4");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 6 WHERE fileType = 5");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 1 WHERE fileType = 7");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 2 WHERE fileType = 8");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 6 WHERE fileType = 9");
    }

    public async Task ConvertFileTypesForFtpImportAsync()
    {
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 3 WHERE fileType = 1");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 4 WHERE fileType = 2");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 5 WHERE fileType = 6");
    }

    public async Task ConvertFileTypesForWebDavImportAsync()
    {
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 7 WHERE fileType = 1");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 8 WHERE fileType = 2");
        await dbClientFactory.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 9 WHERE fileType = 6");
    }

    private void ApplyLocalTarget(string? backupFolder, string? mediaVolumeSerial, bool createFolder)
    {
        if (string.IsNullOrWhiteSpace(backupFolder))
        {
            throw new ArgumentException("Local backup folder is required.", nameof(backupFolder));
        }

        if (createFolder)
        {
            Directory.CreateDirectory(backupFolder);
        }

        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = backupFolder;
        configurationManager.MediaVolumeSerial = string.IsNullOrEmpty(mediaVolumeSerial) || mediaVolumeSerial == "0"
            ? ""
            : mediaVolumeSerial;
        configurationManager.UNCUsername = "";
        configurationManager.UNCPassword = "";
    }

    private void ApplyUncTarget(string? uncPath, string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(uncPath))
        {
            throw new ArgumentException("UNC path is required.", nameof(uncPath));
        }

        var normalized = uncPath.Replace("//", @"\", StringComparison.Ordinal);
        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = normalized;
        configurationManager.MediaVolumeSerial = "";

        if (normalized.StartsWith(@"\\", StringComparison.Ordinal))
        {
            configurationManager.UNCUsername = username ?? "";
            configurationManager.UNCPassword = string.IsNullOrEmpty(password)
                ? ""
                : Crypto.EncryptString(password, DataProtectionScope.LocalMachine);
        }
        else
        {
            configurationManager.UNCUsername = "";
            configurationManager.UNCPassword = "";
        }
    }

    private void ApplyFtpTarget(NewSetupConfiguration configuration)
    {
        configurationManager.MediumType = MediaType.FileTransferServer;
        configurationManager.BackupFolder = "";
        configurationManager.FtpHost = configuration.FtpHost ?? "";
        configurationManager.FtpPort = configuration.FtpPort ?? "21";
        configurationManager.FtpUser = configuration.FtpUser ?? "";
        configurationManager.FtpPass = configuration.FtpPassword ?? "";
        configurationManager.FtpFolder = configuration.FtpFolder ?? "";
        configurationManager.FtpCoding = configuration.FtpEncoding ?? "UTF8";
        configurationManager.FtpEncryptionMode = configuration.FtpEnforceUnencrypted ? "0" : "3";
        configurationManager.FtpSslProtocols = "0";
    }

    private void ApplyWebDavTarget(NewSetupConfiguration configuration)
    {
        configurationManager.MediumType = MediaType.WebDav;
        configurationManager.BackupFolder = "";
        configurationManager.FtpHost = configuration.FtpHost ?? "";
        configurationManager.FtpPort = configuration.FtpPort ?? "443";
        configurationManager.FtpUser = configuration.FtpUser ?? "";
        configurationManager.FtpPass = configuration.FtpPassword ?? "";
        configurationManager.FtpFolder = configuration.FtpFolder ?? "";
    }

    private static string EscapeSql(string value)
    {
        return value.Replace("\"", "\"\"", StringComparison.Ordinal);
    }

    private static bool IsDriveRootPath(string path)
    {
        try
        {
            return PathRules.IsDriveRoot(path);
        }
        catch
        {
            return path.Length == 3;
        }
    }

    private static void DeleteDatabaseFiles(string databaseFile)
    {
        foreach (var path in new[] { databaseFile, databaseFile + "-wal", databaseFile + "-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private sealed class VersionEntry
    {
        public VersionEntry(string path, bool changed)
        {
            Path = path;
            Changed = changed;
        }

        public string Path
        {
            get; set;
        }

        public bool Changed
        {
            get; set;
        }
    }
}

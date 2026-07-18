// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;

namespace BSH.MainApp.Models;

public enum SetupTargetKind
{
    LocalDrive,
    Unc,
    Ftp
}

public enum SetupImportSourceKind
{
    LocalMedia,
    Ftp,
    ExplicitPath
}

public enum SetupWizardStep
{
    Welcome,
    Sources,
    Target,
    Mode,
    ImportMedia,
    ImportSelect,
    ImportRemap,
    Progress
}

public sealed class NewSetupConfiguration
{
    public required IReadOnlyList<string> SourceFolders
    {
        get; init;
    }

    public required SetupTargetKind TargetKind
    {
        get; init;
    }

    public string? LocalBackupFolder
    {
        get; init;
    }

    public string? MediaVolumeSerial
    {
        get; init;
    }

    public string? UncPath
    {
        get; init;
    }

    public string? UncUsername
    {
        get; init;
    }

    public string? UncPassword
    {
        get; init;
    }

    public string? FtpHost
    {
        get; init;
    }

    public string? FtpPort
    {
        get; init;
    }

    public string? FtpUser
    {
        get; init;
    }

    public string? FtpPassword
    {
        get; init;
    }

    public string? FtpFolder
    {
        get; init;
    }

    public string? FtpEncoding
    {
        get; init;
    }

    public bool FtpEnforceUnencrypted
    {
        get; init;
    }

    public required TaskType TaskType
    {
        get; init;
    }
}

public sealed class DiscoveredBackup
{
    public required string ComputerName
    {
        get; init;
    }

    public required string UserName
    {
        get; init;
    }

    public required string FolderPath
    {
        get; init;
    }

    public string DisplayName => $"{ComputerName} / {UserName} — {FolderPath}";
}

public sealed class SourceRemap
{
    public SourceRemap(string originalPath, string currentPath)
    {
        OriginalPath = originalPath;
        CurrentPath = currentPath;
    }

    public string OriginalPath
    {
        get;
    }

    public string CurrentPath
    {
        get; set;
    }
}

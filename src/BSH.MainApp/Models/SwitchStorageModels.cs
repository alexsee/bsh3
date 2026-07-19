// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Models;

public sealed record SwitchStorageFtpTarget(
    string Host,
    string Port,
    string User,
    string Password,
    string Folder,
    string Encoding,
    bool EnforceUnencrypted);

public sealed class SwitchStorageSelection
{
    public required bool IsLocal
    {
        get; init;
    }

    public string? DriveRoot
    {
        get; init;
    }

    public string? MediaVolumeSerial
    {
        get; init;
    }

    public SwitchStorageFtpTarget? Ftp
    {
        get; init;
    }
}

public sealed class SwitchStorageDriveItem
{
    public required string DisplayName
    {
        get; init;
    }

    public required string RootPath
    {
        get; init;
    }

    public required DriveType DriveType
    {
        get; init;
    }
}

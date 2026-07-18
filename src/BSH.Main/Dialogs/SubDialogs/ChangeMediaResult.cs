// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Main;

public enum ChangeMediaKind
{
    LocalDevice,
    Ftp,
    Unc,
}

/// <summary>
/// Typed selection from <see cref="frmChangeMedia"/> after a successful OK.
/// </summary>
public sealed class ChangeMediaResult
{
    public required ChangeMediaKind Kind { get; init; }

    public string LocalBackupFolder { get; init; } = "";

    public string UncPath { get; init; } = "";

    public string UncUsername { get; init; } = "";

    public string UncPassword { get; init; } = "";

    public string FtpHost { get; init; } = "";

    public string FtpPort { get; init; } = "";

    public string FtpUsername { get; init; } = "";

    public string FtpPassword { get; init; } = "";

    public string FtpPath { get; init; } = "";

    public string FtpEncoding { get; init; } = "";
}

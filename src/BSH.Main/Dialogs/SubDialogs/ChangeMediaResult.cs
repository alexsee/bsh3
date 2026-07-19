// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Main;

/// <summary>
/// Typed selection from <see cref="frmChangeMedia"/> after a successful OK.
/// </summary>
public abstract record ChangeMediaSelection;

public sealed record ChangeMediaLocalSelection(string BackupFolder) : ChangeMediaSelection;

public sealed record ChangeMediaUncSelection(string Path, string Username, string Password) : ChangeMediaSelection;

public sealed record ChangeMediaFtpSelection(
    string Host,
    string Port,
    string Username,
    string Password,
    string Path,
    string Encoding,
    bool EnforceUnencrypted) : ChangeMediaSelection;

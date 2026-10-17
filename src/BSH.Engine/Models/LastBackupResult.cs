// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Brightbits.BSH.Engine.Models;

public enum BackupOutcome
{
    Succeeded = 0,
    CompletedWithErrors = 1,
    Failed = 2
}

public enum BackupFailureReason
{
    None = 0,
    DeviceNotReady = 1,
    InsufficientDiskSpace = 2,
    Other = 3
}

public sealed class LastBackupFailedFile
{
    public string Path { get; set; } = "";

    public string Message { get; set; } = "";
}

/// <summary>
/// The outcome of the most recent backup run, persisted so it survives an application restart.
/// </summary>
public sealed class LastBackupResult
{
    public const int MaxStoredFiles = 200;

    private static readonly JsonSerializerOptions SerializerOptions = new();

    public DateTime CompletedAt { get; set; }

    public BackupOutcome Outcome { get; set; }

    public BackupFailureReason FailureReason { get; set; }

    public string FailureMessage { get; set; } = "";

    /// <summary>
    /// Total number of files that could not be backed up; <see cref="FailedFiles"/> holds at most
    /// <see cref="MaxStoredFiles"/> of them.
    /// </summary>
    public int FailedFileCount { get; set; }

    public List<LastBackupFailedFile> FailedFiles { get; set; } = [];

    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);

    public static LastBackupResult FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LastBackupResult>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

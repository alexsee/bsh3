// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Security;

namespace Brightbits.BSH.Engine.Services;

public enum UncProbeStatus
{
    Ok,
    InvalidPath,
    Unreachable,
    ContainsBackupData
}

public readonly record struct UncProbeResult(UncProbeStatus Status, string NormalizedPath, string? Detail = null);

/// <summary>
/// Shared UNC path normalize + connectivity + empty-medium checks for setup / media switch.
/// </summary>
public static class UncTargetProbe
{
    public const string BackupDatabaseFileName = "backup.bshdb";

    public static UncProbeResult Probe(
        string? uncPath,
        string? username,
        string? password,
        bool requireEmptyTarget)
    {
        var path = (uncPath ?? "").Trim().Replace('/', '\\');
        if (!MediaTargetApplier.IsUncPath(path))
        {
            return new UncProbeResult(UncProbeStatus.InvalidPath, path);
        }

        try
        {
            using var networkConnection = new NetworkConnection(path, username ?? "", password ?? "");
            if (!Directory.Exists(path))
            {
                return new UncProbeResult(UncProbeStatus.Unreachable, path);
            }

            if (requireEmptyTarget && File.Exists(Path.Combine(path, BackupDatabaseFileName)))
            {
                return new UncProbeResult(UncProbeStatus.ContainsBackupData, path);
            }

            return new UncProbeResult(UncProbeStatus.Ok, path);
        }
        catch (Exception ex)
        {
            return new UncProbeResult(UncProbeStatus.Unreachable, path, ex.Message);
        }
    }
}

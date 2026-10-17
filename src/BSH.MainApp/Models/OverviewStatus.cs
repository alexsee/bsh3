// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;

namespace BSH.MainApp.Models;

public enum OverviewSeverity
{
    Protected,
    Paused,
    Warning,
    Error,
    NotConfigured
}

public enum OverviewIssueKind
{
    BackupFailed,
    BackupLocationUnavailable,
    BackupOverdue,
    NoBackupYet,
    ScheduleEmpty,
    FilesSkipped,
    LowDiskSpace,
    TurnedOff,
    PausedOnBattery,
    NotConfigured
}

public enum OverviewAction
{
    None,
    BackUpNow,
    ViewFailedFiles,
    ManageBackups,
    ChangeBackupLocation,
    SetSchedule,
    TurnOn
}

public sealed record OverviewIssue(OverviewIssueKind Kind, OverviewSeverity Severity, OverviewAction Action);

public sealed class OverviewInputs
{
    public SystemStatus SystemStatus { get; init; }

    public TaskType TaskType { get; init; }

    public LastBackupResult? LastResult { get; init; }

    /// <summary>
    /// Creation date of the newest backup version, or null when there is none.
    /// </summary>
    public DateTime? LastBackupDate { get; init; }

    /// <summary>
    /// The "remind me after N days without a backup" setting, or null when it is off.
    /// </summary>
    public int? RemindAfterDays { get; init; }

    public bool HasScheduleEntries { get; init; }

    /// <summary>
    /// Result of the live backup location check, or null while it has not completed.
    /// </summary>
    public bool? IsBackupLocationAvailable { get; init; }

    public double FreeSpaceBytes { get; init; }

    /// <summary>
    /// The "warn me when free space is low" limit in megabytes, or null when it is off.
    /// </summary>
    public double? RemindSpaceMegabytes { get; init; }

    public DateTime Now { get; init; }
}

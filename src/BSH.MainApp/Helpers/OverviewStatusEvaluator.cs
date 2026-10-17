// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;

namespace BSH.MainApp.Helpers;

/// <summary>
/// Decides what the overview tells the user about the safety of their data. Issues are returned
/// most urgent first; the first one drives the status headline.
/// </summary>
public static class OverviewStatusEvaluator
{
    public static IReadOnlyList<OverviewIssue> Evaluate(OverviewInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.SystemStatus == SystemStatus.NOT_CONFIGURED)
        {
            return [new OverviewIssue(OverviewIssueKind.NotConfigured, OverviewSeverity.NotConfigured, OverviewAction.None)];
        }

        var issues = new List<OverviewIssue>();
        var lastResult = inputs.LastResult;

        if (lastResult?.Outcome == BackupOutcome.Failed)
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.BackupFailed, OverviewSeverity.Error, GetFailureAction(lastResult)));
        }

        if (inputs.IsBackupLocationAvailable == false)
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.BackupLocationUnavailable, OverviewSeverity.Error, OverviewAction.ChangeBackupLocation));
        }

        if (inputs.LastBackupDate is null)
        {
            if (lastResult?.Outcome != BackupOutcome.Failed)
            {
                issues.Add(new OverviewIssue(OverviewIssueKind.NoBackupYet, OverviewSeverity.Warning, OverviewAction.BackUpNow));
            }
        }
        else if (IsOverdue(inputs))
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.BackupOverdue, OverviewSeverity.Warning, OverviewAction.BackUpNow));
        }

        if (inputs.TaskType == TaskType.Schedule && !inputs.HasScheduleEntries)
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.ScheduleEmpty, OverviewSeverity.Warning, OverviewAction.SetSchedule));
        }

        if (lastResult?.Outcome == BackupOutcome.CompletedWithErrors)
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.FilesSkipped, OverviewSeverity.Warning, OverviewAction.ViewFailedFiles));
        }

        if (IsLowOnDiskSpace(inputs))
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.LowDiskSpace, OverviewSeverity.Warning, OverviewAction.ManageBackups));
        }

        if (inputs.SystemStatus == SystemStatus.DEACTIVATED)
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.TurnedOff, OverviewSeverity.Paused, OverviewAction.TurnOn));
        }
        else if (inputs.SystemStatus == SystemStatus.PAUSED_DUE_TO_BATTERY)
        {
            issues.Add(new OverviewIssue(OverviewIssueKind.PausedOnBattery, OverviewSeverity.Paused, OverviewAction.None));
        }

        // Stable sort: severity first, then the order above.
        return issues
            .Select((issue, index) => (issue, index))
            .OrderByDescending(entry => entry.issue.Severity)
            .ThenBy(entry => entry.index)
            .Select(entry => entry.issue)
            .ToList();
    }

    public static OverviewSeverity GetOverallSeverity(IReadOnlyList<OverviewIssue> issues)
    {
        return issues.Count == 0 ? OverviewSeverity.Protected : issues[0].Severity;
    }

    /// <summary>
    /// Days since the newest backup, using the same whole-day rule as the outdated-backup reminder.
    /// </summary>
    public static int GetDaysSinceLastBackup(OverviewInputs inputs)
    {
        return inputs.LastBackupDate is { } lastBackup ? inputs.Now.Subtract(lastBackup).Days : 0;
    }

    private static OverviewAction GetFailureAction(LastBackupResult result)
    {
        return result.FailureReason switch
        {
            BackupFailureReason.InsufficientDiskSpace => OverviewAction.ManageBackups,
            BackupFailureReason.DeviceNotReady => OverviewAction.BackUpNow,
            _ when result.FailedFileCount > 0 => OverviewAction.ViewFailedFiles,
            _ => OverviewAction.BackUpNow,
        };
    }

    private static bool IsOverdue(OverviewInputs inputs)
    {
        return inputs.RemindAfterDays is int days && days > 0 && GetDaysSinceLastBackup(inputs) > days;
    }

    private static bool IsLowOnDiskSpace(OverviewInputs inputs)
    {
        return inputs.RemindSpaceMegabytes is double megabytes
            && megabytes >= 0
            && inputs.FreeSpaceBytes > 0
            && inputs.FreeSpaceBytes < megabytes * 1024d * 1024d;
    }
}

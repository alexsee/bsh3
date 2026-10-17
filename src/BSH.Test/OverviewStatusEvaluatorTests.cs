// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Helpers;
using BSH.MainApp.Models;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class OverviewStatusEvaluatorTests
{
    private static readonly DateTime Now = new(2026, 10, 17, 12, 0, 0);

    private static OverviewInputs Healthy()
    {
        return new OverviewInputs
        {
            SystemStatus = SystemStatus.ACTIVATED,
            TaskType = TaskType.Auto,
            LastResult = new LastBackupResult { CompletedAt = Now.AddHours(-1), Outcome = BackupOutcome.Succeeded },
            LastBackupDate = Now.AddHours(-1),
            RemindAfterDays = 7,
            IsBackupLocationAvailable = true,
            FreeSpaceBytes = 500d * 1024 * 1024 * 1024,
            RemindSpaceMegabytes = 1024,
            Now = Now,
        };
    }

    private static IReadOnlyList<OverviewIssueKind> Kinds(OverviewInputs inputs)
    {
        return OverviewStatusEvaluator.Evaluate(inputs).Select(issue => issue.Kind).ToList();
    }

    [Test]
    public void HealthySetup_IsProtectedWithoutActions()
    {
        var issues = OverviewStatusEvaluator.Evaluate(Healthy());

        Assert.That(issues, Is.Empty);
        Assert.That(OverviewStatusEvaluator.GetOverallSeverity(issues), Is.EqualTo(OverviewSeverity.Protected));
    }

    [Test]
    public void NotConfigured_HidesEveryOtherIssue()
    {
        var inputs = new OverviewInputs { SystemStatus = SystemStatus.NOT_CONFIGURED, IsBackupLocationAvailable = false, Now = Now };

        Assert.That(Kinds(inputs), Is.EqualTo(new[] { OverviewIssueKind.NotConfigured }));
    }

    [Test]
    public void FailedForDiskSpace_AsksToManageBackups()
    {
        var inputs = new OverviewInputs
        {
            SystemStatus = SystemStatus.ACTIVATED,
            LastBackupDate = Now.AddDays(-1),
            IsBackupLocationAvailable = true,
            LastResult = new LastBackupResult { Outcome = BackupOutcome.Failed, FailureReason = BackupFailureReason.InsufficientDiskSpace },
            Now = Now,
        };

        var issue = OverviewStatusEvaluator.Evaluate(inputs).Single();

        Assert.That(issue.Kind, Is.EqualTo(OverviewIssueKind.BackupFailed));
        Assert.That(issue.Severity, Is.EqualTo(OverviewSeverity.Error));
        Assert.That(issue.Action, Is.EqualTo(OverviewAction.ManageBackups));
    }

    [Test]
    public void FailedFirstBackup_DoesNotAlsoReportNoBackupYet()
    {
        var inputs = new OverviewInputs
        {
            SystemStatus = SystemStatus.ACTIVATED,
            LastResult = new LastBackupResult { Outcome = BackupOutcome.Failed, FailureReason = BackupFailureReason.DeviceNotReady },
            Now = Now,
        };

        Assert.That(Kinds(inputs), Is.EqualTo(new[] { OverviewIssueKind.BackupFailed }));
    }

    [Test]
    public void PartialBackup_IsAWarningThatOffersTheFailedFiles()
    {
        var inputs = new OverviewInputs
        {
            SystemStatus = SystemStatus.ACTIVATED,
            LastBackupDate = Now.AddHours(-1),
            LastResult = new LastBackupResult { Outcome = BackupOutcome.CompletedWithErrors, FailedFileCount = 3 },
            Now = Now,
        };

        var issue = OverviewStatusEvaluator.Evaluate(inputs).Single();

        Assert.That(issue.Kind, Is.EqualTo(OverviewIssueKind.FilesSkipped));
        Assert.That(issue.Severity, Is.EqualTo(OverviewSeverity.Warning));
        Assert.That(issue.Action, Is.EqualTo(OverviewAction.ViewFailedFiles));
    }

    [TestCase(7, 7, false)]
    [TestCase(7, 8, true)]
    [TestCase(null, 100, false)]
    [TestCase(0, 100, false)]
    public void Overdue_UsesTheDaysWithoutBackupReminder(int? remindAfterDays, int daysAgo, bool expectOverdue)
    {
        var inputs = new OverviewInputs
        {
            SystemStatus = SystemStatus.ACTIVATED,
            LastBackupDate = Now.AddDays(-daysAgo),
            RemindAfterDays = remindAfterDays,
            Now = Now,
        };

        Assert.That(Kinds(inputs).Contains(OverviewIssueKind.BackupOverdue), Is.EqualTo(expectOverdue));
    }

    [TestCase(-1d, 1d, false)]
    [TestCase(1024d, 2048d, false)]
    [TestCase(1024d, 512d, true)]
    [TestCase(1024d, 0d, false)]
    public void LowDiskSpace_UsesTheFreeSpaceReminder(double remindSpaceMegabytes, double freeMegabytes, bool expectLow)
    {
        var inputs = new OverviewInputs
        {
            SystemStatus = SystemStatus.ACTIVATED,
            LastBackupDate = Now,
            FreeSpaceBytes = freeMegabytes * 1024 * 1024,
            RemindSpaceMegabytes = remindSpaceMegabytes,
            Now = Now,
        };

        Assert.That(Kinds(inputs).Contains(OverviewIssueKind.LowDiskSpace), Is.EqualTo(expectLow));
    }

    [Test]
    public void LocationCheckStillRunning_IsNotReportedAsUnavailable()
    {
        var inputs = new OverviewInputs { SystemStatus = SystemStatus.ACTIVATED, LastBackupDate = Now, IsBackupLocationAvailable = null, Now = Now };

        Assert.That(Kinds(inputs), Is.Empty);
    }

    [Test]
    public void Issues_AreOrderedErrorsFirstThenWarningsThenPaused()
    {
        var inputs = new OverviewInputs
        {
            SystemStatus = SystemStatus.DEACTIVATED,
            TaskType = TaskType.Schedule,
            HasScheduleEntries = false,
            LastBackupDate = Now,
            IsBackupLocationAvailable = false,
            Now = Now,
        };

        Assert.That(Kinds(inputs), Is.EqualTo(new[]
        {
            OverviewIssueKind.BackupLocationUnavailable,
            OverviewIssueKind.ScheduleEmpty,
            OverviewIssueKind.TurnedOff,
        }));
        Assert.That(
            OverviewStatusEvaluator.GetOverallSeverity(OverviewStatusEvaluator.Evaluate(inputs)),
            Is.EqualTo(OverviewSeverity.Error));
    }
}

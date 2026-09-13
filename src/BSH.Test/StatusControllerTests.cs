// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Covers the hardened <see cref="StatusController"/> observer fan-out:
/// snapshot isolation, failure isolation and late-observer replay.
/// </summary>
public class StatusControllerTests
{
    [Test]
    public void AddObserverAndRemoveObserverRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => StatusController.Current.AddObserver(null!));
        Assert.Throws<ArgumentNullException>(() => StatusController.Current.RemoveObserver(null!));
    }

    [Test]
    public void ReportMethodsFanOutToAllObservers()
    {
        var first = new RecordingStatusReport();
        var second = new RecordingStatusReport();
        StatusController.Current.AddObserver(first);
        StatusController.Current.AddObserver(second);
        try
        {
            StatusController.Current.SetSystemStatus(SystemStatus.ACTIVATED);
            StatusController.Current.ReportAction(ActionType.Backup, silent: true);
            StatusController.Current.ReportState(JobState.RUNNING);
            StatusController.Current.ReportStatus("title", "text");
            StatusController.Current.ReportProgress(10, 4);
            StatusController.Current.ReportFileProgress("file.txt");

            foreach (var observer in new[] { first, second })
            {
                Assert.That(observer.SystemStatuses, Is.EqualTo(new[] { SystemStatus.ACTIVATED }));
                Assert.That(observer.Actions, Is.EqualTo(new[] { (ActionType.Backup, true) }));
                Assert.That(observer.States, Is.EqualTo(new[] { JobState.RUNNING }));
                Assert.That(observer.Statuses, Is.EqualTo(new[] { ("title", "text") }));
                Assert.That(observer.Progress, Is.EqualTo(new[] { (10, 4) }));
                Assert.That(observer.FileProgress, Is.EqualTo(new[] { "file.txt" }));
            }
        }
        finally
        {
            StatusController.Current.RemoveObserver(first);
            StatusController.Current.RemoveObserver(second);
        }
    }

    [Test]
    public void FailingObserverDoesNotBreakOtherObservers()
    {
        var failing = new ThrowingStatusReport();
        var recording = new RecordingStatusReport();
        StatusController.Current.AddObserver(failing);
        StatusController.Current.AddObserver(recording);
        try
        {
            // use a non-backup action so the finished-state balloon logic
            // (which needs the WinForms BackupLogic statics) stays untouched
            StatusController.Current.ReportAction(ActionType.Check, silent: true);
            recording.Actions.Clear();

            Assert.DoesNotThrow(() => StatusController.Current.ReportState(JobState.FINISHED));

            Assert.That(recording.States, Is.EqualTo(new[] { JobState.FINISHED }));
        }
        finally
        {
            StatusController.Current.RemoveObserver(failing);
            StatusController.Current.RemoveObserver(recording);
        }
    }

    [Test]
    public void LateObserverReplaysLastState()
    {
        var early = new RecordingStatusReport();
        StatusController.Current.AddObserver(early);
        try
        {
            StatusController.Current.ReportState(JobState.RUNNING);
            StatusController.Current.ReportStatus("title", "text");
            StatusController.Current.ReportProgress(10, 4);
            StatusController.Current.ReportFileProgress("file.txt");

            var late = new RecordingStatusReport();
            StatusController.Current.AddObserver(late, triggerLastState: true);
            try
            {
                Assert.That(late.States, Is.EqualTo(new[] { JobState.RUNNING }));
                Assert.That(late.Statuses, Is.EqualTo(new[] { ("title", "text") }));
                Assert.That(late.Progress, Is.EqualTo(new[] { (10, 4) }));
                Assert.That(late.FileProgress, Is.EqualTo(new[] { "file.txt" }));
            }
            finally
            {
                StatusController.Current.RemoveObserver(late);
            }
        }
        finally
        {
            StatusController.Current.RemoveObserver(early);
        }
    }

    [Test]
    public void LateObserverReplayFailureDoesNotThrow()
    {
        var failing = new ThrowingStatusReport();

        Assert.DoesNotThrow(() => StatusController.Current.AddObserver(failing, triggerLastState: true));

        StatusController.Current.RemoveObserver(failing);
    }

    [Test]
    public void RemovedObserverStopsReceivingNotifications()
    {
        var observer = new RecordingStatusReport();
        StatusController.Current.AddObserver(observer);
        StatusController.Current.RemoveObserver(observer);

        StatusController.Current.ReportState(JobState.CANCELED);

        Assert.That(observer.States, Is.Empty);
    }

    private sealed class RecordingStatusReport : IStatusReport
    {
        public System.Collections.Generic.List<SystemStatus> SystemStatuses { get; } = new();
        public System.Collections.Generic.List<(ActionType Action, bool Silent)> Actions { get; } = new();
        public System.Collections.Generic.List<JobState> States { get; } = new();
        public System.Collections.Generic.List<(string Title, string Text)> Statuses { get; } = new();
        public System.Collections.Generic.List<(int Total, int Current)> Progress { get; } = new();
        public System.Collections.Generic.List<string> FileProgress { get; } = new();

        public void ReportAction(ActionType action, bool silent) => Actions.Add((action, silent));
        public void ReportState(JobState jobState) => States.Add(jobState);
        public void ReportStatus(string title, string text) => Statuses.Add((title, text));
        public void ReportProgress(int total, int current) => Progress.Add((total, current));
        public void ReportFileProgress(string file) => FileProgress.Add(file);
        public void ReportSystemStatus(SystemStatus systemStatus) => SystemStatuses.Add(systemStatus);
    }

    private sealed class ThrowingStatusReport : IStatusReport
    {
        public void ReportAction(ActionType action, bool silent) => throw new InvalidOperationException("observer failed");
        public void ReportState(JobState jobState) => throw new InvalidOperationException("observer failed");
        public void ReportStatus(string title, string text) => throw new InvalidOperationException("observer failed");
        public void ReportProgress(int total, int current) => throw new InvalidOperationException("observer failed");
        public void ReportFileProgress(string file) => throw new InvalidOperationException("observer failed");
        public void ReportSystemStatus(SystemStatus systemStatus) => throw new InvalidOperationException("observer failed");
    }
}

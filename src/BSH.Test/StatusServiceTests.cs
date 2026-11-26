// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using BSH.MainApp.Contracts;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class StatusServiceTests
{
    [Test]
    public void AddObserverReplaysProgressWhenJobIsRunning()
    {
        var statusService = CreateService();
        statusService.ReportState(JobState.RUNNING);
        statusService.ReportStatus("Copying", "Working");
        statusService.ReportProgress(12, 4);
        statusService.ReportFileProgress(@"C:\file.txt");

        var observer = new RecordingStatusReport();
        statusService.AddObserver(observer, triggerLastState: true);

        Assert.That(observer.States, Does.Contain(JobState.RUNNING));
        Assert.That(observer.StatusTitles, Does.Contain("Copying"));
        Assert.That(observer.ProgressUpdates, Does.Contain((12, 4)));
        Assert.That(observer.FileUpdates, Does.Contain(@"C:\file.txt"));
    }

    [Test]
    public void RemoveObserverDuringNotifyDoesNotThrow()
    {
        var statusService = CreateService();
        var first = new RecordingStatusReport();
        var second = new RecordingStatusReport();
        first.OnProgress = () => statusService.RemoveObserver(second);

        statusService.AddObserver(first);
        statusService.AddObserver(second);

        Assert.DoesNotThrow(() => statusService.ReportProgress(5, 1));
        Assert.That(first.ProgressUpdates, Does.Contain((5, 1)));
    }

    private static StatusService CreateService()
    {
        return new StatusService(
            new FakeConfigurationManager(),
            new FakePresentationService());
    }

    private sealed class RecordingStatusReport : IStatusReport
    {
        public List<JobState> States { get; } = [];
        public List<string> StatusTitles { get; } = [];
        public List<(int Total, int Current)> ProgressUpdates { get; } = [];
        public List<string> FileUpdates { get; } = [];
        public Action? OnProgress { get; set; }

        public void ReportAction(ActionType action, bool silent) { }
        public void ReportState(JobState jobState) => States.Add(jobState);
        public void ReportStatus(string title, string text) => StatusTitles.Add(title);
        public void ReportProgress(int total, int current)
        {
            ProgressUpdates.Add((total, current));
            OnProgress?.Invoke();
        }
        public void ReportFileProgress(string file) => FileUpdates.Add(file);
        public void ReportSystemStatus(SystemStatus systemStatus) { }
    }
}

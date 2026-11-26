// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class WinUIJobSessionPresenterTests
{
    [Test]
    public async Task RequestOverwrite_CancelSkipsFileAndCancelsJob()
    {
        var canceled = false;
        var presenter = CreatePresenter(RequestOverwriteResult.None, () => canceled = true);

        var result = await presenter.RequestOverwrite(new FileTableRow { FileName = "a.txt" }, new FileTableRow { FileName = "a.txt" });

        Assert.That(result, Is.EqualTo(RequestOverwriteResult.NoOverwrite));
        Assert.That(canceled, Is.True);
    }

    [Test]
    public async Task RequestOverwrite_OverwriteDoesNotCancelJob()
    {
        var canceled = false;
        var presenter = CreatePresenter(RequestOverwriteResult.Overwrite, () => canceled = true);

        var result = await presenter.RequestOverwrite(new FileTableRow { FileName = "a.txt" }, new FileTableRow { FileName = "a.txt" });

        Assert.That(result, Is.EqualTo(RequestOverwriteResult.Overwrite));
        Assert.That(canceled, Is.False);
    }

    [Test]
    public async Task RequestOverwrite_SkipDoesNotCancelJob()
    {
        var canceled = false;
        var presenter = CreatePresenter(RequestOverwriteResult.NoOverwrite, () => canceled = true);

        var result = await presenter.RequestOverwrite(new FileTableRow { FileName = "a.txt" }, new FileTableRow { FileName = "a.txt" });

        Assert.That(result, Is.EqualTo(RequestOverwriteResult.NoOverwrite));
        Assert.That(canceled, Is.False);
    }

    private static WinUIJobSessionPresenter CreatePresenter(RequestOverwriteResult overwriteResult, Action cancel)
    {
        return new WinUIJobSessionPresenter(
            new FakePresentationService(),
            new StubStatusService { OverwriteResult = overwriteResult },
            cancel,
            new CompletionActionService());
    }

    private sealed class StubStatusService : IStatusService
    {
        public RequestOverwriteResult OverwriteResult { get; set; } = RequestOverwriteResult.None;

        public JobState JobState { get; set; }
        public RequestOverwriteResult LastFileOverwriteChoice => RequestOverwriteResult.None;
        public string LastFileProgress { get; set; } = "";
        public Collection<FileExceptionEntry> LastFilesException { get; set; } = [];
        public int LastProgressCurrent { get; set; }
        public int LastProgressTotal { get; set; }
        public string LastStatusText { get; set; } = "";
        public string LastStatusTitle { get; set; } = "";
        public SystemStatus SystemStatus { get; set; }

        public void AddObserver(IStatusReport jobReport, bool triggerLastState = false) { }
        public bool IsTaskRunning() => false;
        public void RemoveObserver(IStatusReport jobReport) { }
        public void ReportAction(ActionType action, bool silent) { }
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) { }
        public void ReportFileProgress(string file) { }
        public void ReportProgress(int total, int current) { }
        public void ReportState(JobState jobState) => JobState = jobState;
        public void ReportStatus(string title, string text) { }
        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(OverwriteResult);
        public Task RequestShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public void SetSystemStatus(SystemStatus status) => SystemStatus = status;
        public void ShowExceptionDialog() { }
        public void Initialize() { }
    }
}

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
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Windows.UI.Popups;

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
            new StubPresentationService(),
            new StubStatusService { OverwriteResult = overwriteResult },
            cancel);
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

    private sealed class StubPresentationService : IPresentationService
    {
        public Task CloseBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task CloseMainWindowAsync() => Task.CompletedTask;
        public Task<TaskCompleteAction> CloseStatusWindowAsync() => Task.FromResult(TaskCompleteAction.NoAction);
        public Task OpenCurrentEventLogAsync() => Task.CompletedTask;
        public Task OpenHelpSupportAsync() => Task.CompletedTask;
        public Task<(string? password, bool persist)> RequestPasswordAsync() => Task.FromResult<(string?, bool)>((null, false));
        public Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);
        public Task ResetConfigurationAsync() => Task.CompletedTask;
        public Task ShowAboutWindowAsync() => Task.CompletedTask;
        public Task ShowBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task ShowCompressionExclusionsWindowAsync() => Task.CompletedTask;
        public Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new NewBackupViewModel()));
        public Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.None);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
        public Task<bool> ShowSwitchStorageWindowAsync() => Task.FromResult(false);
    }
}

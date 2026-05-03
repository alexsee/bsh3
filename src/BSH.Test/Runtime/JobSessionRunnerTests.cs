// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime;
using Brightbits.BSH.Engine.Runtime.Ports;
using Brightbits.BSH.Engine.Security;
using NUnit.Framework;

namespace BSH.Test.Runtime;

public class JobSessionRunnerTests
{
    [Test]
    public async Task RunSingleBackupAsync_ReturnsTaskRunning_WhenAnotherTaskIsRunning()
    {
        var backupService = new BackupServiceStub();
        using var jobRuntime = new JobRuntime(backupService, () => true, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.False);
        Assert.That(result.Failure, Is.EqualTo(JobSessionStartFailure.TaskRunning));
        Assert.That(backupService.StartBackupCalls, Is.EqualTo(0));
        Assert.That(presenter.ShowErrorTaskRunningCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task RunSingleBackupAsync_DoesNotShowStartupErrors_WhenStartedSilently()
    {
        var backupService = new BackupServiceStub();
        using var jobRuntime = new JobRuntime(backupService, () => true, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: false);

        Assert.That(result.Started, Is.False);
        Assert.That(result.Failure, Is.EqualTo(JobSessionStartFailure.TaskRunning));
        Assert.That(presenter.ShowStatusWindowCalls, Is.EqualTo(0));
        Assert.That(presenter.ShowErrorTaskRunningCalls, Is.EqualTo(0));
        Assert.That(presenter.ShowErrorDeviceNotReadyCalls, Is.EqualTo(0));
        Assert.That(presenter.ShowErrorPasswordRequiredCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task RunSingleBackupAsync_ReturnsDeviceNotReady_WhenMediaIsMissing()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = false };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.False);
        Assert.That(result.Failure, Is.EqualTo(JobSessionStartFailure.DeviceNotReady));
        Assert.That(backupService.StartBackupCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task RunSingleBackupAsync_ReturnsPasswordRequired_WhenPasswordRequestFails()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true, HasPasswordResult = false };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(false));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime, () => true, () => Hash.GetMD5Hash("secret"), new StoredPasswordAdapterStub());

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.False);
        Assert.That(result.Failure, Is.EqualTo(JobSessionStartFailure.PasswordRequired));
        Assert.That(backupService.StartBackupCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task RunSingleBackupAsync_StartsBackup_WhenSessionCanBePrepared()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true, fullBackup: true, sourceFolders: "C:\\Data");

        Assert.That(result.Started, Is.True);
        Assert.That(result.Canceled, Is.False);
        Assert.That(result.Failure, Is.EqualTo(JobSessionStartFailure.None));
        Assert.That(backupService.StartBackupCalls, Is.EqualTo(1));
        Assert.That(backupService.LastTitle, Is.EqualTo("title"));
        Assert.That(backupService.LastDescription, Is.EqualTo("description"));
        Assert.That(backupService.LastFullBackup, Is.True);
        Assert.That(backupService.LastSources, Is.EqualTo("C:\\Data"));
        Assert.That(backupService.LastSilent, Is.False);
    }

    [Test]
    public async Task RunSingleBackupAsync_UsesStoredPasswordBeforePrompting()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true, HasPasswordResult = false };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(false));
        var presenter = new JobReportStub();
        var storedPasswordAdapter = new StoredPasswordAdapterStub { StoredPassword = "secret" };
        var runner = new JobSessionRunner(backupService, jobRuntime, () => true, () => Hash.GetMD5Hash("secret"), storedPasswordAdapter);

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.LastPasswordSet, Is.EqualTo("secret"));
        Assert.That(presenter.RequestPasswordCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task RunSingleBackupAsync_PromptsAndPersistsPassword_WhenStoredPasswordIsUnavailable()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true, HasPasswordResult = false };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(false));
        var presenter = new JobReportStub
        {
            NextPasswordRequest = new JobSessionPasswordRequest("secret", true)
        };
        var storedPasswordAdapter = new StoredPasswordAdapterStub();
        var runner = new JobSessionRunner(backupService, jobRuntime, () => true, () => Hash.GetMD5Hash("secret"), storedPasswordAdapter);

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.LastPasswordSet, Is.EqualTo("secret"));
        Assert.That(storedPasswordAdapter.StoredPassword, Is.EqualTo("secret"));
    }

    [Test]
    public async Task RunSingleBackupAsync_RetriesPrompt_WhenPasswordIsWrong()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true, HasPasswordResult = false };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(false));
        var presenter = new JobReportStub();
        presenter.PasswordRequests.Enqueue(new JobSessionPasswordRequest("wrong", false));
        presenter.PasswordRequests.Enqueue(new JobSessionPasswordRequest("secret", false));
        var runner = new JobSessionRunner(backupService, jobRuntime, () => true, () => Hash.GetMD5Hash("secret"), new StoredPasswordAdapterStub());

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(presenter.ShowErrorPasswordWrongCalls, Is.EqualTo(1));
        Assert.That(presenter.RequestPasswordCalls, Is.EqualTo(2));
    }

    [Test]
    public async Task RunSingleBackupAsync_UsesLatestPasswordHash_WhenConfigurationChangesAfterConstruction()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true, HasPasswordResult = false };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(false));
        var presenter = new JobReportStub
        {
            NextPasswordRequest = new JobSessionPasswordRequest("new-secret", false)
        };
        var currentHash = Hash.GetMD5Hash("old-secret");
        var runner = new JobSessionRunner(backupService, jobRuntime, () => true, () => currentHash, new StoredPasswordAdapterStub());

        currentHash = Hash.GetMD5Hash("new-secret");

        var result = await runner.RunSingleBackupAsync("title", "description", presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.LastPasswordSet, Is.EqualTo("new-secret"));
        Assert.That(presenter.ShowErrorPasswordWrongCalls, Is.EqualTo(0));
        Assert.That(presenter.RequestPasswordCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task RunSingleRestoreAsync_StartsRestore_WhenSessionCanBePrepared()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleRestoreAsync("5", "\\file.txt", "C:\\Restore", presenter, statusDialog: true, overwrite: FileOverwrite.Overwrite);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.StartRestoreCalls, Is.EqualTo(1));
        Assert.That(backupService.LastVersion, Is.EqualTo("5"));
        Assert.That(backupService.LastFile, Is.EqualTo("\\file.txt"));
        Assert.That(backupService.LastDestination, Is.EqualTo("C:\\Restore"));
        Assert.That(backupService.LastOverwrite, Is.EqualTo(FileOverwrite.Overwrite));
        Assert.That(backupService.LastSilent, Is.False);
    }

    [Test]
    public async Task RunSingleDeleteAsync_StartsDelete_WhenSessionCanBePrepared()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleDeleteAsync("7", presenter, statusDialog: false);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.StartDeleteCalls, Is.EqualTo(1));
        Assert.That(backupService.LastVersion, Is.EqualTo("7"));
        Assert.That(backupService.LastSilent, Is.True);
    }

    [Test]
    public async Task RunSingleDeleteSingleAsync_StartsDeleteSingle_WhenSessionCanBePrepared()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleDeleteSingleAsync("*.tmp", "\\cache\\", presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.StartDeleteSingleCalls, Is.EqualTo(1));
        Assert.That(backupService.LastFileFilter, Is.EqualTo("*.tmp"));
        Assert.That(backupService.LastPathFilter, Is.EqualTo("\\cache\\"));
        Assert.That(backupService.LastSilent, Is.False);
    }

    [Test]
    public async Task RunSingleModifyAsync_StartsEdit_WhenSessionCanBePrepared()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunSingleModifyAsync(presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.StartEditCalls, Is.EqualTo(1));
        Assert.That(backupService.LastSilent, Is.False);
    }

    [Test]
    public async Task RunBatchRestoreAsync_CarriesForwardOverwriteChoice_AndReportsFinishedState()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        presenter.BatchOverwriteChoices.Enqueue(FileOverwrite.Overwrite);
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunBatchRestoreAsync("11", new[] { "\\a.txt", "\\b.txt" }, "C:\\Restore", presenter, statusDialog: true);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.StartRestoreCalls, Is.EqualTo(2));
        Assert.That(backupService.RestoreOverwrites, Is.EqualTo(new[] { FileOverwrite.Ask, FileOverwrite.Overwrite }));
        Assert.That(presenter.ReportedStates, Is.EqualTo(new[] { JobState.RUNNING, JobState.FINISHED }));
        Assert.That(presenter.ProgressUpdates, Is.EqualTo(new[] { "2/1", "2/2" }));
    }

    [Test]
    public async Task RunBatchDeleteAsync_ReportsError_WhenAnyItemFails()
    {
        var backupService = new BackupServiceStub { CheckMediaResult = true };
        backupService.DeleteFailures.Enqueue(new FileExceptionEntry() { Exception = new InvalidOperationException("boom") });
        using var jobRuntime = new JobRuntime(backupService, () => false, () => false, (_, _, _) => Task.FromResult(false), () => Task.FromResult(true));
        var presenter = new JobReportStub();
        var runner = new JobSessionRunner(backupService, jobRuntime);

        var result = await runner.RunBatchDeleteAsync(new[] { "5", "6" }, presenter, statusDialog: false);

        Assert.That(result.Started, Is.True);
        Assert.That(backupService.StartDeleteCalls, Is.EqualTo(2));
        Assert.That(presenter.ReportedStates, Is.EqualTo(new[] { JobState.RUNNING, JobState.ERROR }));
        Assert.That(presenter.ReportedExceptions.Count, Is.EqualTo(1));
        Assert.That(presenter.ReportedExceptions[0].Exception.Message, Is.EqualTo("boom"));
    }

    private sealed class BackupServiceStub : IBackupService
    {
        public bool CheckMediaResult { get; set; } = true;
        public bool HasPasswordResult { get; set; } = true;
        public int StartBackupCalls { get; private set; }
        public int StartRestoreCalls { get; private set; }
        public int StartDeleteCalls { get; private set; }
        public int StartDeleteSingleCalls { get; private set; }
        public int StartEditCalls { get; private set; }
        public string LastTitle { get; private set; }
        public string LastDescription { get; private set; }
        public string LastVersion { get; private set; }
        public string LastFile { get; private set; }
        public string LastDestination { get; private set; }
        public string LastFileFilter { get; private set; }
        public string LastPathFilter { get; private set; }
        public bool LastFullBackup { get; private set; }
        public string LastSources { get; private set; }
        public bool LastSilent { get; private set; }
        public string LastPasswordSet { get; private set; }
        public FileOverwrite LastOverwrite { get; private set; }
        public System.Collections.Generic.List<FileOverwrite> RestoreOverwrites { get; } = new();
        public System.Collections.Generic.Queue<FileExceptionEntry> DeleteFailures { get; } = new();

        public Task<bool> CheckMedia(bool quickCheck = false) => Task.FromResult(CheckMediaResult);
        public string GetPassword() => string.Empty;
        public bool HasPassword() => HasPasswordResult;
        public void SetPassword(string password) => LastPasswordSet = password;
        public Task SetStableAsync(string version, bool stable) => Task.CompletedTask;
        public Task UpdateVersionAsync(string version, VersionDetails versionDetails) => Task.CompletedTask;

        public Task StartBackup(string title, string description, ref IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false)
        {
            StartBackupCalls++;
            LastTitle = title;
            LastDescription = description;
            LastFullBackup = fullBackup;
            LastSources = sources;
            LastSilent = silent;
            return Task.CompletedTask;
        }

        public Task StartDelete(string version, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false)
        {
            StartDeleteCalls++;
            LastVersion = version;
            LastSilent = silent;
            if (DeleteFailures.Count > 0)
            {
                jobReport.ReportExceptions(new Collection<FileExceptionEntry> { DeleteFailures.Dequeue() }, silent);
            }

            return Task.CompletedTask;
        }

        public Task StartDeleteSingle(string fileFilter, string pathFilter, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false)
        {
            StartDeleteSingleCalls++;
            LastFileFilter = fileFilter;
            LastPathFilter = pathFilter;
            LastSilent = silent;
            return Task.CompletedTask;
        }

        public Task StartEdit(ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false)
        {
            StartEditCalls++;
            LastSilent = silent;
            return Task.CompletedTask;
        }

        public Task StartRestore(string version, string file, string destination, ref IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false)
        {
            StartRestoreCalls++;
            LastVersion = version;
            LastFile = file;
            LastDestination = destination;
            LastOverwrite = overwrite;
            LastSilent = silent;
            RestoreOverwrites.Add(overwrite);
            return Task.CompletedTask;
        }
        public void UpdateDatabaseFile(string databaseFile) { }
    }

    private sealed class JobReportStub : IJobSessionPresenter
    {
        public void ReportAction(ActionType action, bool silent) { }
        public void ReportState(JobState jobState) => ReportedStates.Add(jobState);
        public void ReportStatus(string title, string text) { }
        public void ReportProgress(int total, int current) => ProgressUpdates.Add($"{total}/{current}");
        public void ReportFileProgress(string file) { }
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
        {
            foreach (var file in files)
            {
                ReportedExceptions.Add(file);
            }
        }
        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);
        public Task RequestShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;

        public int ShowStatusWindowCalls { get; private set; }
        public int ShowErrorTaskRunningCalls { get; private set; }
        public int ShowErrorDeviceNotReadyCalls { get; private set; }
        public int ShowErrorPasswordRequiredCalls { get; private set; }
        public int ShowErrorPasswordWrongCalls { get; private set; }
        public int RequestPasswordCalls { get; private set; }
        public JobSessionPasswordRequest NextPasswordRequest { get; set; }
        public System.Collections.Generic.Queue<JobSessionPasswordRequest> PasswordRequests { get; } = new();
        public System.Collections.Generic.Queue<FileOverwrite> BatchOverwriteChoices { get; } = new();
        public System.Collections.Generic.List<JobState> ReportedStates { get; } = new();
        public System.Collections.Generic.List<string> ProgressUpdates { get; } = new();
        public System.Collections.Generic.List<FileExceptionEntry> ReportedExceptions { get; } = new();

        public Task ShowStatusWindowAsync()
        {
            ShowStatusWindowCalls++;
            return Task.CompletedTask;
        }

        public Task CompleteAsync(bool triggerShutdown = false, bool triggerHibernate = false, bool honorCompletionActions = true) => Task.CompletedTask;

        public Task ShowErrorTaskRunningAsync()
        {
            ShowErrorTaskRunningCalls++;
            return Task.CompletedTask;
        }

        public Task ShowErrorDeviceNotReadyAsync()
        {
            ShowErrorDeviceNotReadyCalls++;
            return Task.CompletedTask;
        }

        public Task ShowErrorPasswordRequiredAsync()
        {
            ShowErrorPasswordRequiredCalls++;
            return Task.CompletedTask;
        }

        public Task<JobSessionPasswordRequest> RequestPasswordAsync()
        {
            RequestPasswordCalls++;
            if (PasswordRequests.Count > 0)
            {
                return Task.FromResult(PasswordRequests.Dequeue());
            }

            return Task.FromResult(NextPasswordRequest);
        }

        public Task ShowErrorPasswordWrongAsync()
        {
            ShowErrorPasswordWrongCalls++;
            return Task.CompletedTask;
        }

        public Task CancelAsync() => Task.CompletedTask;
        public CancellationToken GetCancellationToken() => CancellationToken.None;
        public void SetCancellationToken(CancellationToken cancellationToken) { }

        public FileOverwrite ResolveBatchOverwriteChoice(FileOverwrite currentOverwrite)
        {
            if (BatchOverwriteChoices.Count > 0)
            {
                return BatchOverwriteChoices.Dequeue();
            }

            return currentOverwrite;
        }
    }

    private sealed class StoredPasswordAdapterStub : IStoredPasswordAdapter
    {
        public string StoredPassword { get; set; } = string.Empty;

        public string GetPassword() => StoredPassword;

        public void StorePassword(string password) => StoredPassword = password;
    }
}

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

    private sealed class BackupServiceStub : IBackupService
    {
        public bool CheckMediaResult { get; set; } = true;
        public bool HasPasswordResult { get; set; } = true;
        public int StartBackupCalls { get; private set; }
        public string LastTitle { get; private set; }
        public string LastDescription { get; private set; }
        public bool LastFullBackup { get; private set; }
        public string LastSources { get; private set; }
        public bool LastSilent { get; private set; }
        public string LastPasswordSet { get; private set; }

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

        public Task StartDelete(string version, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => throw new NotImplementedException();
        public Task StartDeleteSingle(string fileFilter, string pathFilter, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => throw new NotImplementedException();
        public Task StartEdit(ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => throw new NotImplementedException();
        public Task StartRestore(string version, string file, string destination, ref IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => throw new NotImplementedException();
        public void UpdateDatabaseFile(string databaseFile) { }
    }

    private sealed class JobReportStub : IJobSessionPresenter
    {
        public void ReportAction(ActionType action, bool silent) { }
        public void ReportState(JobState jobState) { }
        public void ReportStatus(string title, string text) { }
        public void ReportProgress(int total, int current) { }
        public void ReportFileProgress(string file) { }
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) { }
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
    }

    private sealed class StoredPasswordAdapterStub : IStoredPasswordAdapter
    {
        public string StoredPassword { get; set; } = string.Empty;

        public string GetPassword() => StoredPassword;

        public void StorePassword(string password) => StoredPassword = password;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BSH.Test;

public class BrowserPreviewServiceTests
{
    [Test]
    public async Task PreviewFileAsync_WhenMediaUnavailable_DoesNotRetrieveOrLaunch()
    {
        var jobService = new PreviewJobService { CheckMediaResult = false };
        var queryManager = new PreviewQueryManager();
        var host = new RecordingPreviewHost();
        var service = CreateService(jobService, queryManager: queryManager, previewHost: host);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(jobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Restore }));
        Assert.That(jobService.RequestPasswordCallCount, Is.EqualTo(0));
        Assert.That(queryManager.GetFileNameCalls, Is.Empty);
        Assert.That(host.ShownFiles, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_WhenPasswordRejected_DoesNotRetrieveOrLaunch()
    {
        var jobService = new PreviewJobService { RequestPasswordResult = false };
        var queryManager = new PreviewQueryManager();
        var host = new RecordingPreviewHost();
        var service = CreateService(jobService, queryManager: queryManager, previewHost: host);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(jobService.RequestPasswordCallCount, Is.EqualTo(1));
        Assert.That(queryManager.GetFileNameCalls, Is.Empty);
        Assert.That(host.ShownFiles, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_RetrievesFileLaunchesPreviewAndCleansTemporaryFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "bsh-preview-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tempFile, "preview-content");

        var queryManager = new PreviewQueryManager
        {
            FileResult = (tempFile, true)
        };
        var host = new RecordingPreviewHost();
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(queryManager.GetFileNameCalls.Single(), Is.EqualTo((2, "report.txt", @"\source\docs\", "secret")));
        Assert.That(host.ShownFiles.Single(), Is.EqualTo((tempFile, true)));
        Assert.That(File.Exists(tempFile), Is.False);
        Assert.That(presentation.MessageBoxes, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_WhenLaunchFails_SurfacesErrorThroughPresentationService()
    {
        var queryManager = new PreviewQueryManager
        {
            FileResult = (@"C:\backed-up\report.txt", false)
        };
        var host = new RecordingPreviewHost { ThrowOnShow = true };
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(presentation.MessageBoxes, Has.Count.EqualTo(1));
        Assert.That(presentation.MessageBoxes[0].Title, Is.EqualTo("Feature not available"));
        Assert.That(presentation.MessageBoxes[0].Content, Does.Contain("not available"));
    }

    [Test]
    public async Task PreviewFileAsync_WhenFileIsNotTemporary_LaunchesWithoutDeletingSource()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), "bsh-preview-source-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(sourcePath, "source-content");

        try
        {
            var queryManager = new PreviewQueryManager
            {
                FileResult = (sourcePath, false)
            };
            var host = new RecordingPreviewHost();
            var service = CreateService(queryManager: queryManager, previewHost: host);

            await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

            Assert.That(host.ShownFiles.Single(), Is.EqualTo((sourcePath, false)));
            Assert.That(File.Exists(sourcePath), Is.True);
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    [Test]
    public async Task PreviewFileAsync_WhenRetrievedPathIsEmpty_DoesNotLaunch()
    {
        var queryManager = new PreviewQueryManager
        {
            FileResult = (null, false)
        };
        var host = new RecordingPreviewHost();
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(host.ShownFiles, Is.Empty);
        Assert.That(presentation.MessageBoxes, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_WhenLaunchFailsForTemporaryFile_StillCleansUp()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "bsh-preview-fail-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tempFile, "preview-content");

        var queryManager = new PreviewQueryManager
        {
            FileResult = (tempFile, true)
        };
        var host = new RecordingPreviewHost { ThrowOnShow = true };
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(presentation.MessageBoxes, Has.Count.EqualTo(1));
        Assert.That(File.Exists(tempFile), Is.False);
    }

    private static BrowserPreviewService CreateService(
        PreviewJobService? jobService = null,
        PreviewBackupService? backupService = null,
        PreviewQueryManager? queryManager = null,
        PreviewPresentationService? presentationService = null,
        RecordingPreviewHost? previewHost = null)
    {
        return new BrowserPreviewService(
            jobService ?? new PreviewJobService(),
            backupService ?? new PreviewBackupService(),
            queryManager ?? new PreviewQueryManager(),
            presentationService ?? new PreviewPresentationService(),
            previewHost ?? new RecordingPreviewHost());
    }

    private sealed class RecordingPreviewHost : ISmartPreviewHost
    {
        public List<(string FilePath, bool IsTemporary)> ShownFiles { get; } = [];
        public bool ThrowOnShow { get; set; }

        public Task ShowAsync(string filePath, bool isTemporary)
        {
            if (ThrowOnShow)
            {
                throw new InvalidOperationException("SmartPreview failed to start.");
            }

            ShownFiles.Add((filePath, isTemporary));
            return Task.CompletedTask;
        }
    }

    private sealed class PreviewJobService : IJobService
    {
        public bool CheckMediaResult { get; set; } = true;
        public bool RequestPasswordResult { get; set; } = true;
        public List<ActionType> CheckMediaCalls { get; } = [];
        public int RequestPasswordCallCount { get; private set; }

        public bool IsCancellationRequested => false;
        public void Cancel() { }
        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
        {
            CheckMediaCalls.Add(action);
            return Task.FromResult(CheckMediaResult);
        }

        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "") => Task.FromResult(true);
        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true) => Task.CompletedTask;
        public CancellationToken GetNewCancellationToken() => CancellationToken.None;
        public Task<bool> RequestPassword()
        {
            RequestPasswordCallCount++;
            return Task.FromResult(RequestPasswordResult);
        }

        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
    }

    private sealed class PreviewBackupService : IBackupService
    {
        public Task<bool> CheckMedia(bool quickCheck = false) => Task.FromResult(true);
        public string GetPassword() => "secret";
        public bool HasPassword() => true;
        public void SetPassword(string password) { }
        public Task SetStableAsync(string version, bool stable) => Task.CompletedTask;
        public Task UpdateVersionAsync(string version, VersionDetails versionDetails) => Task.CompletedTask;
        public Task StartBackup(string title, string description, IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false) => Task.CompletedTask;
        public Task StartDelete(string version, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartDeleteSingle(string fileFilter, string pathFilter, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartEdit(IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartRestore(string version, string file, string destination, IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => Task.CompletedTask;
        public void UpdateDatabaseFile(string databaseFile) { }
    }

    private sealed class PreviewQueryManager : IQueryManager
    {
        public (string Path, bool IsTemporary)? FileResult { get; set; } = (@"C:\backed-up\report.txt", false);
        public List<(int VersionId, string FileName, string FilePath, string Password)> GetFileNameCalls { get; } = [];

        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password)
        {
            GetFileNameCalls.Add((versionId, fileName, filePath, password));
            return Task.FromResult(FileResult ?? ((string)null, false));
        }

        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public string GetFileNameFromDrive(FileTableRow file) => file.FileName;
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult<FileDetails>(null);
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult(folder);
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(new VersionDetails { Id = "1" });
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult(new VersionDetails { Id = "1" });
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(1);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult(new VersionDetails { Id = "1" });
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult(new VersionDetails { Id = id });
        public List<VersionDetails> GetVersions(bool desc = true) => [new VersionDetails { Id = "1" }];
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }

    private sealed class PreviewPresentationService : IPresentationService
    {
        public List<(string Title, string Content)> MessageBoxes { get; } = [];

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
        public Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new NewBackupViewModel()));
        public Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
        {
            MessageBoxes.Add((title, content));
            return Task.FromResult(ContentDialogResult.None);
        }

        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
    }
}

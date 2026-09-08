// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;
using BSH.MainApp.ViewModels.Windows;
using BSH.Test.Fakes;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using NUnit.Framework;
using Windows.UI.Popups;

namespace BSH.Test;

public class SetupImportRemapTests
{
    private const string OriginalPath = @"C:\OldPc\Documents";
    private const string RemappedPath = @"D:\ThisPc\Documents";

    [Test]
    public void TryUpdateSourceRemapUpdatesBoundListCurrentPath()
    {
        var viewModel = CreateViewModel();
        viewModel.LoadSourceRemaps([OriginalPath]);
        var boundItem = viewModel.SourceRemaps[0];
        var notifiedProperties = new List<string?>();
        boundItem.PropertyChanged += (_, args) => notifiedProperties.Add(args.PropertyName);

        var updated = viewModel.TryUpdateSourceRemap(0, RemappedPath);

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.True);
            Assert.That(viewModel.SourceRemaps[0], Is.SameAs(boundItem));
            Assert.That(boundItem.OriginalPath, Is.EqualTo(OriginalPath));
            Assert.That(boundItem.CurrentPath, Is.EqualTo(RemappedPath));
            Assert.That(viewModel.SourceRemaps[0].CurrentPath, Is.EqualTo(RemappedPath));
            Assert.That(notifiedProperties, Does.Contain(nameof(SourceRemap.CurrentPath)));
        });
    }

    [Test]
    public async Task FinishImportAsyncAppliesRemappedPathsNotOriginals()
    {
        var setupService = new RecordingSetupService();
        var viewModel = CreateViewModel(setupService);
        viewModel.LoadSourceRemaps([OriginalPath]);
        Assert.That(viewModel.TryUpdateSourceRemap(0, RemappedPath), Is.True);

        var finished = await viewModel.FinishImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(finished, Is.True);
            Assert.That(setupService.LastRemaps, Is.Not.Null);
            Assert.That(setupService.LastRemaps!, Has.Count.EqualTo(1));
            Assert.That(setupService.LastRemaps![0].OriginalPath, Is.EqualTo(OriginalPath));
            Assert.That(setupService.LastRemaps![0].CurrentPath, Is.EqualTo(RemappedPath));
        });
    }

    private static SetupViewModel CreateViewModel(RecordingSetupService? setupService = null)
    {
        return new SetupViewModel(
            new FakeConfigurationManager(),
            setupService ?? new RecordingSetupService(),
            new StubOrchestrationService(),
            new StubJobService(),
            new StubNavigationService(),
            new StubPresentationService());
    }

    private sealed class RecordingSetupService : ISetupService
    {
        public IReadOnlyList<SourceRemap>? LastRemaps { get; private set; }

        public bool CanRemapSourcePath(string originalPath, string newPath, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(originalPath) || string.IsNullOrWhiteSpace(newPath))
            {
                error = "empty";
                return false;
            }

            return true;
        }

        public Task RemapSourcesAsync(IReadOnlyList<SourceRemap> remaps)
        {
            LastRemaps = remaps;
            return Task.CompletedTask;
        }

        public string? GetDefaultSourceFolder() => throw new NotSupportedException();
        public bool TryAddSourceFolder(IList<string> sources, string folderPath, out string? error) => throw new NotSupportedException();
        public string BuildLocalBackupFolder(string driveRoot) => throw new NotSupportedException();
        public bool IsLocalBackupFolderAvailable(string backupFolder) => throw new NotSupportedException();
        public void ApplyNewConfiguration(NewSetupConfiguration configuration) => throw new NotSupportedException();
        public IReadOnlyList<DiscoveredBackup> DiscoverBackupsOnDrive(string driveRoot) => throw new NotSupportedException();
        public bool BackupDatabaseExists(string folderPath) => throw new NotSupportedException();
        public void ReplaceDatabaseWithCopy(string sourceDatabasePath, string destinationDatabasePath) => throw new NotSupportedException();
        public void PrepareDatabaseReplacement(string destinationDatabasePath) => throw new NotSupportedException();
        public Task ConvertFileTypesForLocalImportAsync() => throw new NotSupportedException();
        public Task ConvertFileTypesForFtpImportAsync() => throw new NotSupportedException();
    }

    private sealed class StubOrchestrationService : IOrchestrationService
    {
        public Task InitializeAsync() => Task.CompletedTask;
        public Task StartAsync(bool turnOn = false) => Task.CompletedTask;
        public Task StopAsync(bool turnOff = false) => Task.CompletedTask;
        public Task RefreshAutomationAsync() => Task.CompletedTask;
    }

    private sealed class StubNavigationService : INavigationService
    {
        public event NavigatedEventHandler Navigated { add { } remove { } }
        public bool CanGoBack => false;
        public Frame? Frame { get; set; }
        public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false) => true;
        public bool GoBack() => false;
    }

    private sealed class StubJobService : IJobService
    {
        public bool IsCancellationRequested => false;
        public void Cancel() { }
        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(true);
        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "") => Task.FromResult(true);
        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task<JobSessionResult> DeleteBackupsAsync(List<string> versions, bool statusDialog = true) => Task.FromResult(new JobSessionResult { Started = true });
        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true, IReadOnlyList<int>? versionIds = null) => Task.CompletedTask;
        public Task<bool> RequestPassword() => Task.FromResult(true);
        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
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

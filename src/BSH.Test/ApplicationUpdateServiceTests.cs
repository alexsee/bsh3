// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoUpdaterDotNET;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime.Ports;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Windows.UI.Popups;

namespace BSH.Test;

public class ApplicationUpdateServiceTests
{
    [Test]
    public async Task DownloadBetaPreferenceIsPersisted()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(settings);

        Assert.That(await service.GetDownloadBetaAsync(), Is.False);

        await service.SetDownloadBetaAsync(true);

        Assert.That(await service.GetDownloadBetaAsync(), Is.True);
        Assert.That(await settings.ReadSettingAsync<bool>(ApplicationUpdateService.DownloadBetaSettingKey), Is.True);
    }

    [Test]
    public async Task AutoSearchPreferenceDefaultsToTrueAndIsPersisted()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(settings);

        Assert.That(await service.GetAutoSearchEnabledAsync(), Is.True);

        await service.SetAutoSearchEnabledAsync(false);

        Assert.That(await service.GetAutoSearchEnabledAsync(), Is.False);
        Assert.That(await settings.ReadSettingAsync<bool>(ApplicationUpdateService.AutoSearchUpdatesSettingKey), Is.False);
    }

    [TestCase(null)]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CheckAsyncAlwaysUsesThePreviewFeed(bool? downloadBeta)
    {
        var settings = new TestLocalSettingsService();
        if (downloadBeta.HasValue)
        {
            await settings.SaveSettingAsync(ApplicationUpdateService.DownloadBetaSettingKey, downloadBeta.Value);
        }

        var started = new List<string>();
        var service = CreateService(settings, started.Add);

        await service.CheckAsync(notifyWhenUpToDate: true);

        Assert.That(started, Is.EqualTo(new[] { "https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml" }));
    }

    [Test]
    public async Task MaybeCheckOnStartupAsyncSkipsWhenDisabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.AutoSearchUpdatesSettingKey, false);
        var started = new List<string>();
        var service = CreateService(settings, started.Add);

        await service.MaybeCheckOnStartupAsync();

        Assert.That(started, Is.Empty);
    }

    [Test]
    public async Task MaybeCheckOnStartupAsyncChecksWhenEnabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.AutoSearchUpdatesSettingKey, true);
        var started = new List<string>();
        var service = CreateService(settings, started.Add);

        await service.MaybeCheckOnStartupAsync();

        Assert.That(started, Is.EqualTo(new[] { ApplicationUpdateService.PreviewUpdateFeedUrl }));
    }

    [Test]
    public async Task ResetUniqueUserIdAsyncReplacesStoredId()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.UniqueUserIdSettingKey, "old-id");
        var service = CreateService(settings);

        var newId = await service.ResetUniqueUserIdAsync();

        Assert.That(newId, Is.Not.EqualTo("old-id"));
        Assert.That(await settings.ReadSettingAsync<string>(ApplicationUpdateService.UniqueUserIdSettingKey), Is.EqualTo(newId));
    }

    [Test]
    public async Task WinUIStoredPasswordAdapterClearsPassword()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(WinUIStoredPasswordAdapter.SettingKey, "encrypted-password");
        var adapter = new WinUIStoredPasswordAdapter(settings);

        await adapter.StorePasswordAsync(string.Empty);

        Assert.That(await settings.ReadSettingAsync<string>(WinUIStoredPasswordAdapter.SettingKey), Is.EqualTo(string.Empty));
    }

    [Test]
    public void MainWindowViewModelRoutesUpdateAndPasswordSupportActions()
    {
        var updates = new RecordingUpdateService();
        var passwords = new RecordingStoredPasswordAdapter();
        var viewModel = new MainWindowViewModel(
            presentationService: null,
            navigationService: null,
            updateService: updates,
            storedPasswordAdapter: passwords,
            buildNavigationItems: false);

        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.CheckForUpdates), Is.True);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ClearStoredPassword), Is.True);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ResetUniqueUserId), Is.True);

        Assert.That(updates.CheckCalls, Is.EqualTo(1));
        Assert.That(updates.LastNotifyWhenUpToDate, Is.True);
        Assert.That(updates.ResetUniqueUserIdCalls, Is.EqualTo(1));
        Assert.That(passwords.StoreCalls, Is.EqualTo(1));
        Assert.That(passwords.LastStoredPassword, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task InitializeAsyncIdentifiesTheProductAndDisablesErrorDialogs()
    {
        var service = CreateService();

        await service.InitializeAsync(() => { });
        await service.InitializeAsync(() => { });

        Assert.That(AutoUpdater.ReportErrors, Is.False);
        Assert.That(AutoUpdater.HttpUserAgent, Does.StartWith("Backup Service Home 4/"));
    }

    [Test]
    public async Task ManualCheckShowsADialogWhenTheFeedCannotBeRead()
    {
        var dialogs = new List<(string Title, string Content)>();
        var forms = new List<UpdateInfoEventArgs>();
        var service = CreateCheckService(dialogs, forms);

        await service.CheckAsync(notifyWhenUpToDate: true);
        service.OnCheckForUpdate(new UpdateInfoEventArgs
        {
            Error = new InvalidOperationException("404"),
            IsUpdateAvailable = false
        });

        Assert.That(dialogs, Has.Count.EqualTo(1));
        Assert.That(forms, Is.Empty);
    }

    [Test]
    public void BackgroundCheckStaysSilentWhenTheFeedCannotBeRead()
    {
        var dialogs = new List<(string Title, string Content)>();
        var forms = new List<UpdateInfoEventArgs>();
        var service = CreateCheckService(dialogs, forms);

        service.OnCheckForUpdate(new UpdateInfoEventArgs
        {
            Error = new InvalidOperationException("404"),
            IsUpdateAvailable = false
        });

        Assert.That(dialogs, Is.Empty);
        Assert.That(forms, Is.Empty);
    }

    [Test]
    public async Task ManualCheckShowsADialogWhenTheInstallationIsCurrent()
    {
        var dialogs = new List<(string Title, string Content)>();
        var service = CreateCheckService(dialogs, new List<UpdateInfoEventArgs>());

        await service.CheckAsync(notifyWhenUpToDate: true);
        service.OnCheckForUpdate(new UpdateInfoEventArgs { IsUpdateAvailable = false });

        Assert.That(dialogs, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task AnAvailableUpdateOpensTheUpdateForm()
    {
        var dialogs = new List<(string Title, string Content)>();
        var forms = new List<UpdateInfoEventArgs>();
        var service = CreateCheckService(dialogs, forms);

        await service.CheckAsync(notifyWhenUpToDate: false);
        var args = new UpdateInfoEventArgs { IsUpdateAvailable = true };
        service.OnCheckForUpdate(args);

        Assert.That(dialogs, Is.Empty);
        Assert.That(forms, Is.EqualTo(new[] { args }));
    }

    private static ApplicationUpdateService CreateCheckService(
        List<(string Title, string Content)> dialogs,
        List<UpdateInfoEventArgs> forms)
    {
        return new ApplicationUpdateService(
            new TestLocalSettingsService(),
            () => new RecordingPresentation(dialogs),
            _ => { },
            forms.Add);
    }

    private static ApplicationUpdateService CreateService(
        ILocalSettingsService? localSettingsService = null,
        Action<string>? startUpdateCheck = null)
    {
        return new ApplicationUpdateService(
            localSettingsService ?? new TestLocalSettingsService(),
            presentationServiceFactory: () => throw new InvalidOperationException("Presentation is not used by these tests."),
            startUpdateCheck: startUpdateCheck ?? (_ => { }));
    }

    private sealed class TestLocalSettingsService : ILocalSettingsService
    {
        private readonly Dictionary<string, object?> values = new();

        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (!values.TryGetValue(key, out var value) || value is null)
            {
                return Task.FromResult<T?>(default);
            }

            if (value is T typed)
            {
                return Task.FromResult<T?>(typed);
            }

            return Task.FromResult<T?>((T)Convert.ChangeType(value, typeof(T)));
        }

        public Task SaveSettingAsync<T>(string key, T value)
        {
            values[key] = value;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingUpdateService : IUpdateService
    {
        public int CheckCalls { get; private set; }
        public bool? LastNotifyWhenUpToDate { get; private set; }
        public int ResetUniqueUserIdCalls { get; private set; }

        public Task InitializeAsync(Action onApplicationExitRequested) => Task.CompletedTask;

        public Task CheckAsync(bool notifyWhenUpToDate)
        {
            CheckCalls++;
            LastNotifyWhenUpToDate = notifyWhenUpToDate;
            return Task.CompletedTask;
        }

        public Task MaybeCheckOnStartupAsync() => Task.CompletedTask;
        public Task<bool> GetAutoSearchEnabledAsync() => Task.FromResult(true);
        public Task SetAutoSearchEnabledAsync(bool enabled) => Task.CompletedTask;
        public Task<bool> GetDownloadBetaAsync() => Task.FromResult(false);
        public Task SetDownloadBetaAsync(bool enabled) => Task.CompletedTask;

        public Task<string> ResetUniqueUserIdAsync()
        {
            ResetUniqueUserIdCalls++;
            return Task.FromResult("new-id");
        }
    }

    private sealed class RecordingStoredPasswordAdapter : IStoredPasswordAdapter
    {
        public int StoreCalls { get; private set; }
        public string? LastStoredPassword { get; private set; }

        public Task<string> GetPasswordAsync() => Task.FromResult(string.Empty);

        public Task StorePasswordAsync(string password)
        {
            StoreCalls++;
            LastStoredPassword = password;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPresentation : IPresentationService
    {
        private readonly List<(string Title, string Content)> dialogs;

        public RecordingPresentation(List<(string Title, string Content)> dialogs)
        {
            this.dialogs = dialogs;
        }

        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
        {
            dialogs.Add((title, content));
            return Task.FromResult(ContentDialogResult.None);
        }

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
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
        public Task<bool> ShowSwitchStorageWindowAsync() => Task.FromResult(false);
    }
}

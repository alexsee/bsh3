// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Concurrent;
using System.Reflection;
using AutoUpdaterDotNET;
using BSH.MainApp.Contracts.Services;
using Windows.UI.Popups;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.Services;

public sealed class ApplicationUpdateService : IUpdateService
{
    public const string UniqueUserIdSettingKey = "UniqueUserId";
    public const string DownloadBetaSettingKey = "DownloadBeta";
    public const string AutoSearchUpdatesSettingKey = "AutoSearchUpdates";
    public const string StableUpdateFeedUrl = "https://updates.brightbits.de/backup_service_home/v4/auto_updater.xml";
    public const string BetaUpdateFeedUrl = "https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml";

    private readonly ILocalSettingsService localSettingsService;
    private readonly Func<IPresentationService> presentationServiceFactory;
    private readonly Action<string> startUpdateCheck;
    private readonly ConcurrentQueue<bool> pendingNotifyWhenUpToDate = new();
    private bool eventsHooked;
    private bool initialized;

    public ApplicationUpdateService(
        ILocalSettingsService localSettingsService,
        Func<IPresentationService>? presentationServiceFactory = null,
        Action<string>? startUpdateCheck = null)
    {
        this.localSettingsService = localSettingsService;
        this.presentationServiceFactory = presentationServiceFactory ?? (() => App.GetService<IPresentationService>());
        this.startUpdateCheck = startUpdateCheck ?? (feedUrl => AutoUpdater.Start(feedUrl));
    }

    public async Task InitializeAsync(Action onApplicationExitRequested)
    {
        if (initialized)
        {
            return;
        }

        var uniqueUserId = await GetOrCreateUniqueUserIdAsync();
        AutoUpdater.HttpUserAgent = $"Backup Service Home/{GetApplicationVersion()} {uniqueUserId}";
        AutoUpdater.TopMost = true;
        AutoUpdater.ApplicationExitEvent += () => onApplicationExitRequested();
        EnsureEventsHooked();
        initialized = true;
    }

    public async Task CheckAsync(bool notifyWhenUpToDate)
    {
        var feedUrl = await GetDownloadBetaAsync() ? BetaUpdateFeedUrl : StableUpdateFeedUrl;
        pendingNotifyWhenUpToDate.Enqueue(notifyWhenUpToDate);
        EnsureEventsHooked();
        startUpdateCheck(feedUrl);
    }

    public async Task MaybeCheckOnStartupAsync()
    {
        if (!await GetAutoSearchEnabledAsync())
        {
            return;
        }

        await CheckAsync(notifyWhenUpToDate: false);
    }

    public async Task<bool> GetAutoSearchEnabledAsync()
    {
        return await localSettingsService.ReadSettingAsync<bool?>(AutoSearchUpdatesSettingKey) ?? true;
    }

    public Task SetAutoSearchEnabledAsync(bool enabled)
    {
        return localSettingsService.SaveSettingAsync(AutoSearchUpdatesSettingKey, enabled);
    }

    public async Task<bool> GetDownloadBetaAsync()
    {
        return await localSettingsService.ReadSettingAsync<bool?>(DownloadBetaSettingKey) ?? false;
    }

    public Task SetDownloadBetaAsync(bool enabled)
    {
        return localSettingsService.SaveSettingAsync(DownloadBetaSettingKey, enabled);
    }

    public async Task<string> ResetUniqueUserIdAsync()
    {
        var uniqueUserId = Guid.NewGuid().ToString();
        await localSettingsService.SaveSettingAsync(UniqueUserIdSettingKey, uniqueUserId);
        return uniqueUserId;
    }

    private async Task<string> GetOrCreateUniqueUserIdAsync()
    {
        var existing = await localSettingsService.ReadSettingAsync<string>(UniqueUserIdSettingKey);
        if (!string.IsNullOrEmpty(existing))
        {
            return existing;
        }

        return await ResetUniqueUserIdAsync();
    }

    private void EnsureEventsHooked()
    {
        if (eventsHooked)
        {
            return;
        }

        eventsHooked = true;
        AutoUpdater.CheckForUpdateEvent += OnCheckForUpdate;
    }

    private void OnCheckForUpdate(UpdateInfoEventArgs args)
    {
        pendingNotifyWhenUpToDate.TryDequeue(out var notifyWhenUpToDate);

        if (!args.IsUpdateAvailable)
        {
            if (notifyWhenUpToDate)
            {
                _ = presentationServiceFactory().ShowMessageBoxAsync(
                    "MSG_NO_UPDATE_FOUND_TITLE".GetLocalized(),
                    "ApplicationUpdate_UpToDate_Text".GetLocalized(),
                    [new UICommand("MsgBox_OK".GetLocalized())]);
            }

            return;
        }

        AutoUpdater.ShowUpdateForm(args);
    }

    private static string GetApplicationVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
    }
}

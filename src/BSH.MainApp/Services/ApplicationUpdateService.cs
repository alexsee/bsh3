// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Concurrent;
using System.Reflection;
using AutoUpdaterDotNET;
using BSH.MainApp.Contracts.Services;
using CommunityToolkit.WinUI;
using Serilog;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public sealed class ApplicationUpdateService : IUpdateService
{
    public const string UniqueUserIdSettingKey = "UniqueUserId";
    public const string DownloadBetaSettingKey = "DownloadBeta";
    public const string AutoSearchUpdatesSettingKey = "AutoSearchUpdates";
    public const string StableUpdateFeedUrl = "https://updates.brightbits.de/backup_service_home/v4/auto_updater.xml";
    public const string BetaUpdateFeedUrl = "https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml";

    /// <summary>
    /// Preview builds only offer the WinUI beta artifact. The stable v4 feed is not used.
    /// </summary>
    public const string PreviewUpdateFeedUrl = BetaUpdateFeedUrl;

    private readonly ILocalSettingsService localSettingsService;
    private readonly Func<IPresentationService> presentationServiceFactory;
    private readonly Action<string> startUpdateCheck;
    private readonly Action<UpdateInfoEventArgs> showUpdateForm;
    private readonly ConcurrentQueue<bool> pendingNotifyWhenUpToDate = new();
    private bool eventsHooked;
    private bool initialized;

    public ApplicationUpdateService(
        ILocalSettingsService localSettingsService,
        Func<IPresentationService>? presentationServiceFactory = null,
        Action<string>? startUpdateCheck = null,
        Action<UpdateInfoEventArgs>? showUpdateForm = null)
    {
        this.localSettingsService = localSettingsService;
        this.presentationServiceFactory = presentationServiceFactory ?? (() => App.GetService<IPresentationService>());
        this.startUpdateCheck = startUpdateCheck ?? (feedUrl => AutoUpdater.Start(feedUrl));
        this.showUpdateForm = showUpdateForm ?? AutoUpdater.ShowUpdateForm;
    }

    public async Task InitializeAsync(Action onApplicationExitRequested)
    {
        if (initialized)
        {
            return;
        }

        var uniqueUserId = await GetOrCreateUniqueUserIdAsync();
        AutoUpdater.ReportErrors = false;
        AutoUpdater.HttpUserAgent = $"Backup Service Home 4/{GetApplicationVersion()} {uniqueUserId}";
        AutoUpdater.TopMost = true;
        AutoUpdater.ApplicationExitEvent += () => onApplicationExitRequested();
        EnsureEventsHooked();
        initialized = true;
    }

    public Task CheckAsync(bool notifyWhenUpToDate)
    {
        pendingNotifyWhenUpToDate.Enqueue(notifyWhenUpToDate);
        EnsureEventsHooked();
        startUpdateCheck(PreviewUpdateFeedUrl);
        return Task.CompletedTask;
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

    internal void OnCheckForUpdate(UpdateInfoEventArgs args)
    {
        pendingNotifyWhenUpToDate.TryDequeue(out var notifyWhenUpToDate);
        if (args.Error != null)
        {
            Log.Warning(args.Error, "Update check failed.");
            if (notifyWhenUpToDate)
            {
                ShowMessage("ApplicationUpdate_CheckFailed_Title", "ApplicationUpdate_CheckFailed_Text");
            }

            return;
        }

        if (args.IsUpdateAvailable)
        {
            showUpdateForm(args);
            return;
        }

        if (notifyWhenUpToDate)
        {
            ShowMessage("MSG_NO_UPDATE_FOUND_TITLE", "ApplicationUpdate_UpToDate_Text");
        }
    }

    private void ShowMessage(string titleKey, string textKey)
    {
        _ = presentationServiceFactory().ShowMessageBoxAsync(
            titleKey.GetLocalized() ?? string.Empty,
            textKey.GetLocalized() ?? string.Empty,
            [new UICommand("MsgBox_OK".GetLocalized() ?? string.Empty)]);
    }

    private static string GetApplicationVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
    }
}

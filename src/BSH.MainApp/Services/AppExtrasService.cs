// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Reflection;
using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;

public sealed class AppExtrasService : IAppExtrasService
{
    public const string StartupValueName = "BackupServiceHome3Run";
    public const string BackupPasswordSettingKey = "BackupPassword";
    public const string UniqueUserIdSettingKey = "UniqueUserId";
    public const string DownloadBetaSettingKey = "DownloadBeta";
    public const string AutoSearchUpdatesSettingKey = "AutoSearchUpdates";
    public const string StableUpdateFeedUrl = "https://updates.brightbits.de/backup_service_home/v4/auto_updater.xml";
    public const string BetaUpdateFeedUrl = "https://updates.brightbits.de/backup_service_home/v4/auto_updater_beta.xml";

    private readonly ILocalSettingsService localSettingsService;
    private readonly IOrchestrationService orchestrationService;
    private readonly IStartupLaunchAdapter startupLaunchAdapter;
    private readonly IUpdateCheckAdapter updateCheckAdapter;
    private readonly Func<string> getExecutablePath;
    private readonly Action exitApplication;

    public AppExtrasService(
        ILocalSettingsService localSettingsService,
        IOrchestrationService orchestrationService,
        IStartupLaunchAdapter startupLaunchAdapter,
        IUpdateCheckAdapter updateCheckAdapter,
        Func<string>? getExecutablePath = null,
        Action? exitApplication = null)
    {
        this.localSettingsService = localSettingsService;
        this.orchestrationService = orchestrationService;
        this.startupLaunchAdapter = startupLaunchAdapter;
        this.updateCheckAdapter = updateCheckAdapter;
        this.getExecutablePath = getExecutablePath ?? (() => Environment.ProcessPath ?? AppContext.BaseDirectory);
        this.exitApplication = exitApplication ?? (() => { });
    }

    public bool IsLaunchAtStartupEnabled()
    {
        try
        {
            return startupLaunchAdapter.IsEnabled(StartupValueName);
        }
        catch
        {
            return false;
        }
    }

    public bool TrySetLaunchAtStartup(bool enabled)
    {
        try
        {
            if (enabled)
            {
                var executablePath = getExecutablePath();
                startupLaunchAdapter.SetEnabled(StartupValueName, $"\"{executablePath}\" -delayedstart");
            }
            else
            {
                startupLaunchAdapter.Disable(StartupValueName);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> GetDownloadBetaAsync()
    {
        return await localSettingsService.ReadSettingAsync<bool?>(DownloadBetaSettingKey) ?? false;
    }

    public Task SetDownloadBetaAsync(bool enabled)
    {
        return localSettingsService.SaveSettingAsync(DownloadBetaSettingKey, enabled);
    }

    public async Task<bool> GetAutoSearchUpdatesAsync()
    {
        return await localSettingsService.ReadSettingAsync<bool?>(AutoSearchUpdatesSettingKey) ?? true;
    }

    public Task SetAutoSearchUpdatesAsync(bool enabled)
    {
        return localSettingsService.SaveSettingAsync(AutoSearchUpdatesSettingKey, enabled);
    }

    public async Task CheckForUpdatesAsync()
    {
        var feedUrl = await GetDownloadBetaAsync() ? BetaUpdateFeedUrl : StableUpdateFeedUrl;
        updateCheckAdapter.Start(feedUrl, notifyWhenUpToDate: true);
    }

    public async Task MaybeCheckForUpdatesOnStartupAsync()
    {
        if (!await GetAutoSearchUpdatesAsync())
        {
            return;
        }

        var feedUrl = await GetDownloadBetaAsync() ? BetaUpdateFeedUrl : StableUpdateFeedUrl;
        updateCheckAdapter.Start(feedUrl, notifyWhenUpToDate: false);
    }

    public Task ClearStoredPasswordAsync()
    {
        return localSettingsService.SaveSettingAsync(BackupPasswordSettingKey, string.Empty);
    }

    public async Task<string> ResetUniqueUserIdAsync()
    {
        var uniqueUserId = Guid.NewGuid().ToString();
        await localSettingsService.SaveSettingAsync(UniqueUserIdSettingKey, uniqueUserId);
        return uniqueUserId;
    }

    public async Task<string> GetOrCreateUniqueUserIdAsync()
    {
        var existing = await localSettingsService.ReadSettingAsync<string>(UniqueUserIdSettingKey);
        if (!string.IsNullOrEmpty(existing))
        {
            return existing;
        }

        return await ResetUniqueUserIdAsync();
    }

    public async Task ExitApplicationAsync()
    {
        await orchestrationService.StopAsync();
        exitApplication();
    }

    public static string GetApplicationVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
    }
}

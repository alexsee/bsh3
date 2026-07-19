// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;

public class BrowserViewPreferencesService : IBrowserViewPreferencesService
{
    public const string InfoPaneVisibleSettingsKey = "BrowserInfoPaneVisible";

    private readonly ILocalSettingsService localSettingsService;

    public BrowserViewPreferencesService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
    }

    public async Task<bool> GetInfoPaneVisibleAsync()
    {
        return await localSettingsService.ReadSettingAsync<bool?>(InfoPaneVisibleSettingsKey) ?? false;
    }

    public Task SetInfoPaneVisibleAsync(bool visible)
    {
        return localSettingsService.SaveSettingAsync(InfoPaneVisibleSettingsKey, visible);
    }
}

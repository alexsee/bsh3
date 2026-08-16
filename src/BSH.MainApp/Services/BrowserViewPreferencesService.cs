// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;

public class BrowserViewPreferencesService : IBrowserViewPreferencesService
{
    public const string InfoPaneVisibleSettingsKey = "BrowserInfoPaneVisible";

    private readonly ILocalSettingsService localSettingsService;
    private readonly SemaphoreSlim writeGate = new(1, 1);

    public BrowserViewPreferencesService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
    }

    public async Task<bool> GetInfoPaneVisibleAsync()
    {
        return await localSettingsService.ReadSettingAsync<bool?>(InfoPaneVisibleSettingsKey) ?? false;
    }

    public async Task SetInfoPaneVisibleAsync(bool visible)
    {
        await writeGate.WaitAsync();
        try
        {
            await localSettingsService.SaveSettingAsync(InfoPaneVisibleSettingsKey, visible);
        }
        finally
        {
            writeGate.Release();
        }
    }
}

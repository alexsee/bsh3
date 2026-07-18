// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Runtime.Ports;
using Brightbits.BSH.Engine.Security;
using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;

/// <summary>
/// WinUI adapter for durable backup password storage.
/// </summary>
public sealed class WinUIStoredPasswordAdapter : IStoredPasswordAdapter
{
    private readonly ILocalSettingsService localSettingsService;

    public WinUIStoredPasswordAdapter(ILocalSettingsService localSettingsService)
    {
        ArgumentNullException.ThrowIfNull(localSettingsService);

        this.localSettingsService = localSettingsService;
    }

    public async Task<string> GetPasswordAsync()
    {
        var encryptedPassword = await localSettingsService.ReadSettingAsync<string>(AppExtrasService.BackupPasswordSettingKey);
        if (string.IsNullOrEmpty(encryptedPassword))
        {
            return string.Empty;
        }

        return Crypto.DecryptString(encryptedPassword);
    }

    public Task StorePasswordAsync(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return localSettingsService.SaveSettingAsync(AppExtrasService.BackupPasswordSettingKey, string.Empty);
        }

        return localSettingsService.SaveSettingAsync(AppExtrasService.BackupPasswordSettingKey, Crypto.EncryptString(password));
    }
}

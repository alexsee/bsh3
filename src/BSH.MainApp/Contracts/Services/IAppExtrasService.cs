// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IAppExtrasService
{
    bool IsLaunchAtStartupEnabled();
    bool TrySetLaunchAtStartup(bool enabled);

    Task<bool> GetDownloadBetaAsync();
    Task SetDownloadBetaAsync(bool enabled);

    Task<bool> GetAutoSearchUpdatesAsync();
    Task SetAutoSearchUpdatesAsync(bool enabled);

    Task CheckForUpdatesAsync();
    Task MaybeCheckForUpdatesOnStartupAsync();

    Task ClearStoredPasswordAsync();
    Task<string> ResetUniqueUserIdAsync();
    Task<string> GetOrCreateUniqueUserIdAsync();

    Task ExitApplicationAsync();
}

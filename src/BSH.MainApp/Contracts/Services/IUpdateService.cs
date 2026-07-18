// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IUpdateService
{
    Task InitializeAsync(Action onApplicationExitRequested);

    Task CheckAsync(bool notifyWhenUpToDate);

    Task MaybeCheckOnStartupAsync();

    Task<bool> GetAutoSearchEnabledAsync();

    Task SetAutoSearchEnabledAsync(bool enabled);

    Task<bool> GetDownloadBetaAsync();

    Task SetDownloadBetaAsync(bool enabled);

    Task<string> ResetUniqueUserIdAsync();
}

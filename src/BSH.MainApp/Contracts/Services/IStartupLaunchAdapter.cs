// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IStartupLaunchAdapter
{
    bool IsEnabled();

    /// <summary>
    /// Enables or disables launch at Windows sign-in.
    /// Returns false when the change could not be applied (for example access denied).
    /// </summary>
    bool TrySetEnabled(bool enabled);
}

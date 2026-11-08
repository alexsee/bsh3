// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp;

/// <summary>
/// Remembers a manual second launch that arrives while autostart is still
/// initializing. The activate event can fire before the main window exists.
/// </summary>
public sealed class PreviewForegroundCoordinator
{
    private readonly object gate = new();
    private bool startupReady;
    private bool pending;

    /// <summary>
    /// Returns true when the window can be shown immediately.
    /// Before startup finishes, records the request and returns false.
    /// </summary>
    public bool TryPresentNow()
    {
        lock (gate)
        {
            if (!startupReady)
            {
                pending = true;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Marks startup complete and reports whether a foreground request arrived
    /// while initialization was still running.
    /// </summary>
    public bool CompleteStartup()
    {
        lock (gate)
        {
            startupReady = true;
            var wasPending = pending;
            pending = false;
            return wasPending;
        }
    }
}

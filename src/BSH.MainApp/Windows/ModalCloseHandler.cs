// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Windows;

/// <summary>
/// Completes a modal wait when the window is dismissed, and avoids calling Close
/// again if the title-bar X already closed it.
/// </summary>
public sealed class ModalCloseHandler
{
    private bool wasClosed;

    public void HandleClosed(Action completeCanceled)
    {
        wasClosed = true;
        completeCanceled();
    }

    public async Task<T> AwaitThenCloseAsync<T>(Task<T> wait, Action close)
    {
        var result = await wait;
        if (!wasClosed)
        {
            close();
        }

        return result;
    }
}

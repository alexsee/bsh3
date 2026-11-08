// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

namespace BSH.MainApp;

/// <summary>
/// Posts work to the dispatcher captured on the UI thread.
/// The main window is prepared inside that callback, never in order to find the dispatcher.
/// </summary>
internal static class UiThread
{
    /// <summary>
    /// Dispatcher captured on the UI thread. A volatile write publishes it to
    /// background callers before they post work.
    /// </summary>
    internal static volatile DispatcherQueue Queue;

    public static Task RunAsync(Action prepare, Action work)
    {
        return RunAsync(Queue, prepare, work);
    }

    public static Task RunAsync(Action prepare, Func<Task> work)
    {
        return RunAsync(Queue, prepare, work);
    }

    public static Task<T> RunAsync<T>(Action prepare, Func<Task<T>> work)
    {
        return RunAsync(Queue, prepare, work);
    }

    public static Task RunAsync(DispatcherQueue queue, Action prepare, Action work)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(prepare);
        ArgumentNullException.ThrowIfNull(work);
        return queue.EnqueueAsync(() =>
        {
            prepare();
            work();
        });
    }

    public static Task RunAsync(DispatcherQueue queue, Action prepare, Func<Task> work)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(prepare);
        ArgumentNullException.ThrowIfNull(work);
        return queue.EnqueueAsync(async () =>
        {
            prepare();
            await work();
        });
    }

    public static Task<T> RunAsync<T>(DispatcherQueue queue, Action prepare, Func<Task<T>> work)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(prepare);
        ArgumentNullException.ThrowIfNull(work);
        return queue.EnqueueAsync(async () =>
        {
            prepare();
            return await work();
        });
    }
}

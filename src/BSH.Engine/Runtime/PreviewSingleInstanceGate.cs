// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading;

namespace Brightbits.BSH.Engine.Runtime;

public enum PreviewStartupResult
{
    Started,
    ActivateExisting,
    BlockedByOtherEngine
}

public interface IPreviewInstanceSync
{
    bool TryAcquireMutex(string name, out IDisposable ownership);

    IDisposable Listen(string name, Action onSignal);

    void Signal(string name);
}

/// <summary>
/// Keeps one Preview process, and keeps that process from opening the shared
/// database while the stable app holds <see cref="PreviewSingleInstanceGate.SharedEngineMutexName"/>.
/// </summary>
public sealed class PreviewSingleInstanceGate : IDisposable
{
    public const string PreviewMutexName = "BackupServiceHome3Preview";
    public const string SharedEngineMutexName = "BackupServiceHome3";
    public const string ActivateEventName = "BackupServiceHome3Preview.Activate";

    private readonly IDisposable previewOwnership;
    private readonly IDisposable engineOwnership;
    private readonly IDisposable listener;
    private bool disposed;

    private PreviewSingleInstanceGate(IDisposable previewOwnership, IDisposable engineOwnership, IDisposable listener)
    {
        this.previewOwnership = previewOwnership;
        this.engineOwnership = engineOwnership;
        this.listener = listener;
    }

    public static PreviewStartupResult Acquire(Action onActivateRequested, out PreviewSingleInstanceGate gate)
    {
        return Acquire(
            onActivateRequested,
            out gate,
            PreviewMutexName,
            SharedEngineMutexName,
            ActivateEventName,
            WindowsPreviewInstanceSync.Shared);
    }

    public static PreviewStartupResult Acquire(
        Action onActivateRequested,
        out PreviewSingleInstanceGate gate,
        string previewMutexName,
        string engineMutexName,
        string activateEventName,
        IPreviewInstanceSync sync)
    {
        gate = null;
        ArgumentNullException.ThrowIfNull(onActivateRequested);
        ArgumentNullException.ThrowIfNull(sync);

        if (!sync.TryAcquireMutex(previewMutexName, out var previewOwnership))
        {
            sync.Signal(activateEventName);
            return PreviewStartupResult.ActivateExisting;
        }

        if (!sync.TryAcquireMutex(engineMutexName, out var engineOwnership))
        {
            previewOwnership.Dispose();
            return PreviewStartupResult.BlockedByOtherEngine;
        }

        var subscription = sync.Listen(activateEventName, onActivateRequested);
        gate = new PreviewSingleInstanceGate(previewOwnership, engineOwnership, subscription);
        return PreviewStartupResult.Started;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        listener.Dispose();
        engineOwnership.Dispose();
        previewOwnership.Dispose();
    }
}

public sealed class WindowsPreviewInstanceSync : IPreviewInstanceSync
{
    public static readonly WindowsPreviewInstanceSync Shared = new();

    private WindowsPreviewInstanceSync()
    {
    }

    public bool TryAcquireMutex(string name, out IDisposable ownership)
    {
        var mutex = new Mutex(false, name);
        try
        {
            if (mutex.WaitOne(0))
            {
                ownership = new MutexOwnership(mutex);
                return true;
            }
        }
        catch (AbandonedMutexException)
        {
            ownership = new MutexOwnership(mutex);
            return true;
        }

        mutex.Dispose();
        ownership = null;
        return false;
    }

    public IDisposable Listen(string name, Action onSignal)
    {
        var handle = new EventWaitHandle(false, EventResetMode.AutoReset, name);
        var listener = new EventListener(handle, onSignal);
        listener.Start();
        return listener;
    }

    public void Signal(string name)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using var handle = EventWaitHandle.OpenExisting(name);
                handle.Set();
                return;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Thread.Sleep(50);
            }
        }
    }

    private sealed class MutexOwnership : IDisposable
    {
        private readonly Mutex mutex;
        private bool disposed;

        public MutexOwnership(Mutex mutex)
        {
            this.mutex = mutex;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            try
            {
                mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
            }

            mutex.Dispose();
        }
    }

    private sealed class EventListener : IDisposable
    {
        private readonly EventWaitHandle handle;
        private readonly Action onSignal;
        private readonly CancellationTokenSource stop = new();
        private readonly Thread thread;

        public EventListener(EventWaitHandle handle, Action onSignal)
        {
            this.handle = handle;
            this.onSignal = onSignal;
            thread = new Thread(Listen)
            {
                IsBackground = true,
                Name = "PreviewActivate"
            };
        }

        public void Start()
        {
            thread.Start();
        }

        public void Dispose()
        {
            stop.Cancel();
            try
            {
                handle.Set();
            }
            catch (ObjectDisposedException)
            {
            }

            if (thread.IsAlive && Thread.CurrentThread != thread)
            {
                thread.Join(TimeSpan.FromSeconds(2));
            }

            handle.Dispose();
            stop.Dispose();
        }

        private void Listen()
        {
            while (!stop.IsCancellationRequested)
            {
                var signaled = false;
                try
                {
                    signaled = handle.WaitOne(TimeSpan.FromMilliseconds(250));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                if (!signaled || stop.IsCancellationRequested)
                {
                    continue;
                }

                try
                {
                    onSignal();
                }
                catch (Exception)
                {
                    // Activation is best-effort. The running instance stays up.
                }
            }
        }
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Exceptions;
using Serilog;

namespace Brightbits.BSH.Engine.Runtime;

public sealed class JobRuntime : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<JobRuntime>();
    private readonly IBackupService backupService;
    private readonly Func<bool> isTaskRunning;
    private readonly Func<bool, bool> shouldPromptForMedia;
    private readonly Func<ActionType, CancellationTokenSource, Task<bool>> waitForMediaAsync;
    private readonly object cancellationTokenSync = new();
    private CancellationTokenSource cancellationTokenSource;
    private bool sessionActive;
    private bool disposed;

    public bool IsCancellationRequested
    {
        get
        {
            lock (cancellationTokenSync)
            {
                return cancellationTokenSource?.IsCancellationRequested ?? false;
            }
        }
    }

    public JobRuntime(
        IBackupService backupService,
        Func<bool> isTaskRunning,
        Func<bool, bool> shouldPromptForMedia,
        Func<ActionType, CancellationTokenSource, Task<bool>> waitForMediaAsync)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(isTaskRunning);
        ArgumentNullException.ThrowIfNull(shouldPromptForMedia);
        ArgumentNullException.ThrowIfNull(waitForMediaAsync);

        this.backupService = backupService;
        this.isTaskRunning = isTaskRunning;
        this.shouldPromptForMedia = shouldPromptForMedia;
        this.waitForMediaAsync = waitForMediaAsync;
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Cancel()
    {
        _logger.Debug("Cancellation of current task requested by runtime.");

        lock (cancellationTokenSync)
        {
            if (disposed || cancellationTokenSource == null)
            {
                return;
            }

            cancellationTokenSource.Cancel();
        }
    }

    public async Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
    {
        _logger.Debug("Media check requested by task {Action}.", action);

        CancellationTokenSource cts;
        lock (cancellationTokenSync)
        {
            ThrowIfDisposed();
            cts = cancellationTokenSource;
        }

        if (await backupService.CheckMedia())
        {
            return true;
        }

        if (shouldPromptForMedia(silent))
        {
            return await waitForMediaAsync(action, cts);
        }

        return false;
    }

    public async Task<CancellationToken> PrepareAsync(ActionType action, bool statusDialog)
    {
        if (isTaskRunning())
        {
            throw new TaskRunningException();
        }

        CancellationToken newCancellationToken;
        lock (cancellationTokenSync)
        {
            ThrowIfDisposed();
            if (sessionActive)
            {
                throw new TaskRunningException();
            }

            sessionActive = true;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            newCancellationToken = cancellationTokenSource.Token;
        }

        try
        {
            if (!await CheckMediaAsync(action, !statusDialog))
            {
                throw new DeviceNotReadyException();
            }

            return newCancellationToken;
        }
        catch
        {
            CompleteSession();
            throw;
        }
    }

    internal void CompleteSession()
    {
        lock (cancellationTokenSync)
        {
            sessionActive = false;
        }
    }

    public void Dispose()
    {
        lock (cancellationTokenSync)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(JobRuntime));
        }
    }
}

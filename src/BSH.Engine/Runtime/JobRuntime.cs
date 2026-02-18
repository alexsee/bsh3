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
    private readonly Func<bool> shouldWaitForMedia;
    private readonly Func<ActionType, bool, CancellationTokenSource, Task<bool>> waitForMediaAsync;
    private readonly Func<Task<bool>> requestPasswordAsync;
    private readonly object cancellationTokenSync = new();
    private CancellationTokenSource cancellationTokenSource;
    private CancellationToken cancellationToken;
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
        Func<bool> shouldWaitForMedia,
        Func<ActionType, bool, CancellationTokenSource, Task<bool>> waitForMediaAsync,
        Func<Task<bool>> requestPasswordAsync)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(isTaskRunning);
        ArgumentNullException.ThrowIfNull(shouldWaitForMedia);
        ArgumentNullException.ThrowIfNull(waitForMediaAsync);
        ArgumentNullException.ThrowIfNull(requestPasswordAsync);

        this.backupService = backupService;
        this.isTaskRunning = isTaskRunning;
        this.shouldWaitForMedia = shouldWaitForMedia;
        this.waitForMediaAsync = waitForMediaAsync;
        this.requestPasswordAsync = requestPasswordAsync;

        GetNewCancellationToken();
    }

    public CancellationToken GetNewCancellationToken()
    {
        lock (cancellationTokenSync)
        {
            ThrowIfDisposed();

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            return cancellationToken;
        }
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
        _logger.Debug("Media check requested by task {action}.", action);

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

        if (shouldWaitForMedia() && !silent)
        {
            return await waitForMediaAsync(action, silent, cts);
        }

        return false;
    }

    public async Task<CancellationToken> PrepareAsync(ActionType action, bool statusDialog)
    {
        var newCancellationToken = GetNewCancellationToken();

        if (isTaskRunning())
        {
            throw new TaskRunningException();
        }

        if (!await CheckMediaAsync(action, !statusDialog))
        {
            throw new DeviceNotReadyException();
        }

        if (!await requestPasswordAsync())
        {
            throw new PasswordRequiredException();
        }

        return newCancellationToken;
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
            cancellationToken = default;
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

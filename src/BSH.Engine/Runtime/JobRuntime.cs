// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Service.Contracts;
using Brightbits.BSH.Engine.Types.Exceptions;
using Serilog;

using Brightbits.BSH.Engine.Types;
namespace Brightbits.BSH.Engine.Runtime;

public sealed class JobRuntime : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<JobRuntime>();
    private readonly IBackupService backupService;
    private readonly Func<bool> isTaskRunning;
    private readonly Func<bool, MediaWaitMode> selectMediaWaitMode;
    private readonly Func<ActionType, MediaWaitMode, CancellationTokenSource, Task<bool>> waitForMediaAsync;
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
        : this(
            backupService,
            isTaskRunning,
            silent => shouldWaitForMedia() && !silent ? MediaWaitMode.PromptUser : MediaWaitMode.None,
            (action, mode, cancellationTokenSource) => waitForMediaAsync(action, mode == MediaWaitMode.SilentPolling, cancellationTokenSource),
            requestPasswordAsync)
    {
    }

    public JobRuntime(
        IBackupService backupService,
        Func<bool> isTaskRunning,
        Func<bool, bool> shouldWaitForMedia,
        Func<ActionType, bool, CancellationTokenSource, Task<bool>> waitForMediaAsync,
        Func<Task<bool>> requestPasswordAsync)
        : this(
            backupService,
            isTaskRunning,
            silent =>
            {
                if (!shouldWaitForMedia(silent))
                {
                    return MediaWaitMode.None;
                }

                return silent ? MediaWaitMode.SilentPolling : MediaWaitMode.PromptUser;
            },
            (action, mode, cancellationTokenSource) => waitForMediaAsync(action, mode == MediaWaitMode.SilentPolling, cancellationTokenSource),
            requestPasswordAsync)
    {
    }

    public JobRuntime(
        IBackupService backupService,
        Func<bool> isTaskRunning,
        Func<bool, MediaWaitMode> selectMediaWaitMode,
        Func<ActionType, MediaWaitMode, CancellationTokenSource, Task<bool>> waitForMediaAsync,
        Func<Task<bool>> requestPasswordAsync)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(isTaskRunning);
        ArgumentNullException.ThrowIfNull(selectMediaWaitMode);
        ArgumentNullException.ThrowIfNull(waitForMediaAsync);
        ArgumentNullException.ThrowIfNull(requestPasswordAsync);

        this.backupService = backupService;
        this.isTaskRunning = isTaskRunning;
        this.selectMediaWaitMode = selectMediaWaitMode;
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

        var waitMode = selectMediaWaitMode(silent);
        if (waitMode != MediaWaitMode.None)
        {
            return await waitForMediaAsync(action, waitMode, cts);
        }

        return false;
    }

    public async Task<CancellationToken> PrepareAsync(ActionType action, bool statusDialog, bool requirePassword = true)
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

        if (requirePassword && !await requestPasswordAsync())
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

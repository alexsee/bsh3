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

public sealed class JobRuntime
{
    private readonly ILogger _logger = Log.ForContext<JobRuntime>();
    private readonly IBackupService backupService;
    private readonly Func<bool> isTaskRunning;
    private readonly Func<bool> shouldWaitForMedia;
    private readonly Func<ActionType, bool, CancellationTokenSource, Task<bool>> waitForMediaAsync;
    private readonly Func<Task<bool>> requestPasswordAsync;
    private CancellationTokenSource cancellationTokenSource;
    private CancellationToken cancellationToken;

    public bool IsCancellationRequested => cancellationTokenSource?.IsCancellationRequested ?? false;

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
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
        return cancellationToken;
    }

    public void Cancel()
    {
        _logger.Debug("Cancellation of current task requested by runtime.");

        if (cancellationTokenSource == null)
        {
            return;
        }

        cancellationTokenSource.Cancel();
    }

    public async Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
    {
        _logger.Debug("Media check requested by task {action}.", action);

        if (await backupService.CheckMedia())
        {
            return true;
        }

        if (shouldWaitForMedia() && !silent)
        {
            return await waitForMediaAsync(action, silent, cancellationTokenSource);
        }

        return false;
    }

    public async Task PrepareAsync(ActionType action, bool statusDialog)
    {
        GetNewCancellationToken();

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
    }
}

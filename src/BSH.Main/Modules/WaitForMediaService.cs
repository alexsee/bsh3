// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Services;

namespace Brightbits.BSH.Main;

public class WaitForMediaService
{
    private const int THREAD_SLEEP_SECONDS = 5000;

    private const int MAX_WAITING_SECONDS = 300000;

    private readonly IBackupService backupService;

    private readonly bool silent;

    private readonly ActionType action;

    private readonly CancellationTokenSource cancellationTokenSource;

    private frmWaitForMedia window;

    private long currentWaitingTime = 0L;

    public WaitForMediaService(IBackupService backupService, bool silent, ActionType action, CancellationTokenSource cancellationTokenSource)
    {
        this.backupService = backupService;
        this.silent = silent;
        this.action = action;
        this.cancellationTokenSource = cancellationTokenSource;
    }

    public async Task<bool> ExecuteAsync()
    {
        // show window?
        if (!silent)
        {
            window = new frmWaitForMedia();
            window.OnAbort_Click += cancellationTokenSource.Cancel;
            window.Show();
        }

        // wait for media
        var result = await Task.Run(() =>
            {
                while (true)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        Thread.Sleep(THREAD_SLEEP_SECONDS);
                        currentWaitingTime += THREAD_SLEEP_SECONDS;

                        if (silent && currentWaitingTime > MAX_WAITING_SECONDS)
                        {
                            break;
                        }

                        if (backupService.CheckMedia())
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }

                return false;
            });

        // close window
        if (!silent)
        {
            window.Hide();
            window.Dispose();
        }

        return result;
    }
}
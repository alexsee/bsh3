﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts.Services;
using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;

public class WaitForMediaService : IWaitForMediaService
{
    private const int THREAD_SLEEP_SECONDS = 5000;

    private const int MAX_WAITING_SECONDS = 300000;

    private readonly IBackupService backupService;

    private long currentWaitingTime = 0L;

    public WaitForMediaService(IBackupService backupService)
    {
        this.backupService = backupService;
    }

    public async Task<bool> ExecuteAsync(bool silent, CancellationTokenSource cancellationTokenSource)
    {
        //// show window?
        //if (!silent)
        //{
        //    window = new frmWaitForMedia();
        //    window.OnAbort_Click += cancellationTokenSource.Cancel;
        //    window.Show();
        //}

        // wait for media
        bool result = await Task.Run(() =>
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

        //// close window
        //if (!silent)
        //{
        //    window.Hide();
        //    window.Dispose();
        //}

        return result;
    }
}

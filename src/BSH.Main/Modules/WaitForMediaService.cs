// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Services;

namespace Brightbits.BSH.Main;

public class WaitForMediaService
{
    private const int PollIntervalMilliseconds = 5000;

    private const int MaxWaitingMilliseconds = 300000;

    private readonly IBackupService backupService;

    private readonly bool silent;

    private readonly CancellationTokenSource cancellationTokenSource;

    private frmWaitForMedia window;

    private long currentWaitingTime = 0L;

    public WaitForMediaService(IBackupService backupService, bool silent, CancellationTokenSource cancellationTokenSource)
    {
        this.backupService = backupService;
        this.silent = silent;
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
        var result = await Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        await Task.Delay(PollIntervalMilliseconds);
                        currentWaitingTime += PollIntervalMilliseconds;

                        if (silent && currentWaitingTime > MaxWaitingMilliseconds)
                        {
                            break;
                        }

                        if (await backupService.CheckMedia())
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

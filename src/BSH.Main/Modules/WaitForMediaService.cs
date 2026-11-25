// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Services;

namespace Brightbits.BSH.Main;

public class WaitForMediaService
{
    private const int PollIntervalMilliseconds = 5000;

    private readonly IBackupService backupService;

    private readonly CancellationTokenSource cancellationTokenSource;

    public WaitForMediaService(IBackupService backupService, CancellationTokenSource cancellationTokenSource)
    {
        this.backupService = backupService;
        this.cancellationTokenSource = cancellationTokenSource;
    }

    public async Task<bool> ExecuteAsync()
    {
        using var window = new frmWaitForMedia();
        window.OnAbort_Click += cancellationTokenSource.Cancel;
        window.Show();

        return await Task.Run(async () =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(PollIntervalMilliseconds);
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
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts.Services;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Windows;
using CommunityToolkit.WinUI;
using WinUIEx;

namespace BSH.MainApp.Services;

public class WaitForMediaService : IWaitForMediaService
{
    private const int THREAD_SLEEP_SECONDS = 5000;

    private const int MAX_WAITING_SECONDS = 300000;

    private readonly IBackupService backupService;

    private long currentWaitingTime = 0L;

    private WaitForMediumWindow? window;

    public WaitForMediaService(IBackupService backupService)
    {
        this.backupService = backupService;
    }

    public async Task<bool> ExecuteAsync(bool silent, CancellationTokenSource cancellationTokenSource)
    {
        await ShowWindowAsync(silent, cancellationTokenSource);
        var result = await WaitForMediaAvailabilityAsync(silent, cancellationTokenSource);
        await CloseWindowAsync(silent, cancellationTokenSource);
        return result;
    }

    private async Task ShowWindowAsync(bool silent, CancellationTokenSource cancellationTokenSource)
    {
        if (silent)
        {
            return;
        }

        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            window = new WaitForMediumWindow();
            window.ViewModel.OnCancelRequested += cancellationTokenSource.Cancel;
            window.CenterOnScreen();
            window.Activate();
        });
    }

    private async Task<bool> WaitForMediaAvailabilityAsync(bool silent, CancellationTokenSource cancellationTokenSource)
    {
        return await Task.Run(async () =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(THREAD_SLEEP_SECONDS);
                    currentWaitingTime += THREAD_SLEEP_SECONDS;

                    if (silent && currentWaitingTime > MAX_WAITING_SECONDS)
                    {
                        return false;
                    }

                    if (await backupService.CheckMedia())
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        });
    }

    private async Task CloseWindowAsync(bool silent, CancellationTokenSource cancellationTokenSource)
    {
        if (silent || window == null)
        {
            return;
        }

        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            window.ViewModel.OnCancelRequested -= cancellationTokenSource.Cancel;
            window.Close();
            window = null;
        });
    }
}

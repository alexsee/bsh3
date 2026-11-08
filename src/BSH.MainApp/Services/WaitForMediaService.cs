// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts.Services;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Windows;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.Services;

public class WaitForMediaService : IWaitForMediaService
{
    private const int THREAD_SLEEP_SECONDS = 5000;

    private readonly IBackupService backupService;

    private WaitForMediumWindow? window;

    public WaitForMediaService(IBackupService backupService)
    {
        this.backupService = backupService;
    }

    public async Task<bool> ExecuteAsync(CancellationTokenSource cancellationTokenSource)
    {
        await ShowWindowAsync(cancellationTokenSource);
        var result = await WaitForMediaAvailabilityAsync(cancellationTokenSource);
        await CloseWindowAsync(cancellationTokenSource);
        return result;
    }

    private async Task ShowWindowAsync(CancellationTokenSource cancellationTokenSource)
    {
        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            window = new WaitForMediumWindow();
            window.ViewModel.OnCancelRequested += cancellationTokenSource.Cancel;
            window.CenterOnMainWindow();
            window.Activate();
        });
    }

    private async Task<bool> WaitForMediaAvailabilityAsync(CancellationTokenSource cancellationTokenSource)
    {
        return await Task.Run(async () =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(THREAD_SLEEP_SECONDS);

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

    private async Task CloseWindowAsync(CancellationTokenSource cancellationTokenSource)
    {
        if (window == null)
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

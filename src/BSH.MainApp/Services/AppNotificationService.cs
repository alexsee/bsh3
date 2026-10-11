// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppNotifications;

namespace BSH.MainApp.Notifications;

public class AppNotificationService : IAppNotificationService, IDisposable
{
    private readonly INavigationService _navigationService;
    private bool _isRegistered;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();
        _isRegistered = true;
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        Activate(args?.Argument);
    }

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);
        AppNotificationManager.Default.Show(appNotification);
        return appNotification.Id != 0;
    }

    public void Activate(string? arguments)
    {
        var pageKey = ToastNotificationActivation.ResolvePageKey(arguments);
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            try
            {
                if (App.MainWindow.ViewModel.IsShellNavigationEnabled)
                {
                    _navigationService.NavigateTo(pageKey);
                }

                App.MainWindow.Activate();
                App.MainWindow.AppWindow.MoveInZOrderAtTop();
            }
            catch
            {
                // Fail safe: never crash from toast activation.
            }
        });
    }

    public void Unregister()
    {
        if (!_isRegistered)
        {
            return;
        }

        _isRegistered = false;
        AppNotificationManager.Default.NotificationInvoked -= OnNotificationInvoked;
        AppNotificationManager.Default.Unregister();
    }

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }
}

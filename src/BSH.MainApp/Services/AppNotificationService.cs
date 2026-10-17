// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppNotifications;
using Serilog;

namespace BSH.MainApp.Notifications;

public class AppNotificationService : IAppNotificationService, IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<AppNotificationService>();

    private readonly INavigationService _navigationService;
    private readonly Action registerNotifications;
    private readonly Action detachNotifications;
    private readonly Action unregisterNotifications;
    private readonly Func<string, bool> showNotification;
    private bool _isRegistered;

    public AppNotificationService(INavigationService navigationService)
        : this(navigationService, register: null, detach: null, showPayload: null, unregister: null)
    {
    }

    internal AppNotificationService(
        INavigationService navigationService,
        Action? register,
        Action? detach,
        Func<string, bool>? showPayload,
        Action? unregister = null)
    {
        _navigationService = navigationService;
        registerNotifications = register ?? RegisterWithPlatform;
        detachNotifications = detach ?? DetachFromPlatform;
        showNotification = showPayload ?? ShowWithPlatform;
        unregisterNotifications = unregister ?? UnregisterFromPlatform;
    }

    internal void MarkRegisteredForTests()
    {
        _isRegistered = true;
    }

    public void Initialize()
    {
        try
        {
            registerNotifications();
            _isRegistered = true;
        }
        catch (Exception ex)
        {
            _isRegistered = false;
            try
            {
                detachNotifications();
            }
            catch (Exception)
            {
                // Register failed before the callback was attached.
            }

            Logger.Warning(ex, "App notifications could not be registered.");
        }
    }

    private void RegisterWithPlatform()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();
    }

    private void DetachFromPlatform()
    {
        AppNotificationManager.Default.NotificationInvoked -= OnNotificationInvoked;
    }

    private void UnregisterFromPlatform()
    {
        AppNotificationManager.Default.NotificationInvoked -= OnNotificationInvoked;
        AppNotificationManager.Default.Unregister();
    }

    private static bool ShowWithPlatform(string payload)
    {
        var appNotification = new AppNotification(payload);
        AppNotificationManager.Default.Show(appNotification);
        return appNotification.Id != 0;
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        Activate(args?.Argument);
    }

    public bool Show(string payload)
    {
        if (!_isRegistered)
        {
            return false;
        }

        try
        {
            return showNotification(payload);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "App notification could not be shown.");
            return false;
        }
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
        unregisterNotifications();
    }

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }
}

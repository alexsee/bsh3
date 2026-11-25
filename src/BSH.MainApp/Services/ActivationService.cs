// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;

using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;

namespace BSH.MainApp.Services;

public class ActivationService : IActivationService
{
    private readonly IAppNotificationService _notificationService;
    private readonly INavigationService _navigationService;
    private readonly SetupRouting _setupRouting;

    public ActivationService(
        IAppNotificationService notificationService,
        INavigationService navigationService,
        SetupRouting setupRouting)
    {
        _notificationService = notificationService;
        _navigationService = navigationService;
        _setupRouting = setupRouting;
    }

    public void ActivateForLaunch(LaunchActivatedEventArgs args, bool showWindow)
    {
        var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
        if (activation?.Kind == ExtendedActivationKind.AppNotification)
        {
            _notificationService.Activate((activation.Data as AppNotificationActivatedEventArgs)?.Argument);
        }

        if (_navigationService.Frame?.Content == null)
        {
            _setupRouting.NavigateForStartup(args.Arguments);
        }

        if (showWindow)
        {
            ShowMainWindow();
        }
    }

    public void ShowMainWindow()
    {
        App.MainWindow.Activate();
    }
}

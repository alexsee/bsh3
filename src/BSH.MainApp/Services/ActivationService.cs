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
    private readonly SetupRouting _setupRouting;
    private readonly Func<bool> _hasFrameContent;
    private readonly Action _activateWindow;

    public ActivationService(
        IAppNotificationService notificationService,
        INavigationService navigationService,
        SetupRouting setupRouting)
        : this(notificationService, setupRouting,
            () => navigationService.Frame?.Content != null, static () => App.MainWindow.Activate())
    {
    }

    internal ActivationService(
        IAppNotificationService notificationService,
        SetupRouting setupRouting,
        Func<bool> hasFrameContent,
        Action activateWindow)
    {
        _notificationService = notificationService;
        _setupRouting = setupRouting;
        _hasFrameContent = hasFrameContent;
        _activateWindow = activateWindow;
    }

    public void ActivateForLaunch(LaunchActivatedEventArgs args, bool showWindow)
    {
        var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
        ActivateForLaunch(args.Arguments, showWindow, activation?.Kind,
            () => (activation!.Data as AppNotificationActivatedEventArgs)?.Argument);
    }

    internal void ActivateForLaunch(
        string? launchArguments, bool showWindow, ExtendedActivationKind? activationKind, Func<string?> getNotificationArgument)
    {
        if (activationKind == ExtendedActivationKind.AppNotification)
        {
            _notificationService.Activate(getNotificationArgument());
        }

        if (!_hasFrameContent())
        {
            _setupRouting.NavigateForStartup(launchArguments);
        }

        if (showWindow)
        {
            ShowMainWindow();
        }
    }

    public void ShowMainWindow()
    {
        _activateWindow();
    }
}

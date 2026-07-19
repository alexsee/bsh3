// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;

namespace BSH.MainApp.Activation;

public class AppNotificationActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly IAppNotificationService _notificationService;

    public AppNotificationActivationHandler(IAppNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs? args)
    {
        return AppInstance.GetCurrent().GetActivatedEventArgs()?.Kind == ExtendedActivationKind.AppNotification;
    }

    protected override Task HandleInternalAsync(LaunchActivatedEventArgs? args)
    {
        var argument = (AppInstance.GetCurrent().GetActivatedEventArgs()?.Data as AppNotificationActivatedEventArgs)?.Argument;
        _notificationService.Activate(argument);
        return Task.CompletedTask;
    }
}

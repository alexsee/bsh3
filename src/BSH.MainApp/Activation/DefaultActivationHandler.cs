// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;

using Microsoft.UI.Xaml;

namespace BSH.MainApp.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;
    private readonly SetupRouting _setupRouting;

    public DefaultActivationHandler(INavigationService navigationService, SetupRouting setupRouting)
    {
        _navigationService = navigationService;
        _setupRouting = setupRouting;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _setupRouting.NavigateForStartup(args.Arguments);

        await Task.CompletedTask;
    }
}

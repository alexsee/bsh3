// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Config;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels;

namespace BSH.MainApp.Services;

public class SetupRouting
{
    private readonly IConfigurationManager configurationManager;
    private readonly INavigationService navigationService;

    public SetupRouting(IConfigurationManager configurationManager, INavigationService navigationService)
    {
        this.configurationManager = configurationManager;
        this.navigationService = navigationService;
    }

    public bool IsSetupRequired => configurationManager.IsConfigured == "0";

    public void NavigateForStartup(object? parameter = null)
    {
        if (IsSetupRequired)
        {
            navigationService.NavigateTo(typeof(SetupViewModel).FullName!, parameter, clearNavigation: true);
            return;
        }

        navigationService.NavigateTo(typeof(MainViewModel).FullName!, parameter);
    }

    public void NavigateToSetup()
    {
        // Force a fresh SetupPage instance even if setup is already showing.
        navigationService.NavigateTo(typeof(SetupViewModel).FullName!, parameter: Guid.NewGuid().ToString("N"), clearNavigation: true);
    }
}

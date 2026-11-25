// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml;

namespace BSH.MainApp.Contracts.Services;

public interface IActivationService
{
    void ActivateForLaunch(LaunchActivatedEventArgs args, bool showWindow);

    void ShowMainWindow();
}

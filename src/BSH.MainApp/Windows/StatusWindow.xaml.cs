// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.Windows;

public sealed partial class StatusWindow : WinUIEx.WindowEx
{
    public StatusViewModel ViewModel { get; set; } = new StatusViewModel();

    public StatusWindow()
    {
        InitializeComponent();
        Closed += StatusWindow_Closed;

        var statusService = App.GetService<IStatusService>();
        statusService.AddObserver(ViewModel, true);
    }

    private void StatusWindow_Closed(object sender, WindowEventArgs args)
    {
        ViewModel.Detach();
        App.GetService<IStatusService>().RemoveObserver(ViewModel);
    }
}

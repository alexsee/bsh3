// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels.Windows;

namespace BSH.MainApp.Windows;

public sealed partial class StatusWindow : WinUIEx.WindowEx
{
    public StatusViewModel ViewModel { get; set; } = new StatusViewModel();

    public StatusWindow()
    {
        InitializeComponent();

        var statusService = App.GetService<IStatusService>();
        statusService.AddObserver(ViewModel, true);
    }
}

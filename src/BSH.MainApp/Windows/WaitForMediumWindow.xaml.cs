// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;

namespace BSH.MainApp.Windows;

public sealed partial class WaitForMediumWindow : WinUIEx.WindowEx
{
    public WaitForMediumViewModel ViewModel { get; } = new WaitForMediumViewModel();

    public WaitForMediumWindow()
    {
        this.InitializeComponent();
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views.SettingsPages;

public sealed partial class OptionsSettingsPage : Page
{
    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public OptionsSettingsPage()
    {
        this.InitializeComponent();
    }
}

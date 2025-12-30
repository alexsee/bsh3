// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;
using BSH.MainApp.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views.SettingsPages;

public sealed partial class SourcesSettingsPage : Page
{
    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public SourcesSettingsPage()
    {
        this.InitializeComponent();
    }

    private async void ExcludeSettingsCard_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FilterWindow();
        await dialog.ShowDialogAsync();
    }
}

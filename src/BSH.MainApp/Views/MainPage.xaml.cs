// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainPage()
    {
        DataContext = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private async void btnExecuteBackup_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
    }
}

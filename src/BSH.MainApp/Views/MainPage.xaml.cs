// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = App.GetService<MainViewModel>();

    public MainPage()
    {
        InitializeComponent();
    }

    public static InfoBarSeverity ToInfoBarSeverity(OverviewSeverity severity)
    {
        return severity switch
        {
            OverviewSeverity.Error => InfoBarSeverity.Error,
            OverviewSeverity.Warning => InfoBarSeverity.Warning,
            OverviewSeverity.Protected => InfoBarSeverity.Success,
            _ => InfoBarSeverity.Informational,
        };
    }

    public static Visibility Invert(bool value)
    {
        return value ? Visibility.Collapsed : Visibility.Visible;
    }
}

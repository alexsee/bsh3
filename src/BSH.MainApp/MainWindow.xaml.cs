// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels.Windows;
using BSH.MainApp.Views;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Media;
using WinUIEx;

namespace BSH.MainApp;

public sealed partial class MainWindow : WinUIEx.WindowEx
{
    public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

    public MainWindow()
    {
        InitializeComponent();

        SystemBackdrop = new MicaBackdrop();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);

        // Load the window/taskbar icon from the multi-resolution .ico so Windows can
        // pick a crisp size per DPI instead of upscaling a single small frame.
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app_ico.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }

        this.CenterOnScreen();
        Title = "AppDisplayName".GetLocalized();
    }

    private void WindowEx_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        args.Handled = true;
        this.Hide();
    }

    private void EnsureCurrentPageSelected()
    {
        var pageService = App.GetService<IPageService>();

        foreach (var item in ViewModel.NavigationItems)
        {
            if (pageService.GetPageType(item.Tag?.ToString() ?? string.Empty) == ContentArea.Content?.GetType())
            {
                ViewModel.CurrentPage = item;
                return;
            }
        }
    }

    private void ContentArea_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        var isSetup = e.Content is SetupPage;
        ViewModel.SetSetupMode(isSetup);
        if (!isSetup)
        {
            EnsureCurrentPageSelected();
        }
    }
}

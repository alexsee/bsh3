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

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Initializes a native WinUI window; verified by the real app screenshot fixtures.")]
    public MainWindow()
    {
        InitializeComponent();

        SystemBackdrop = new MicaBackdrop();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);
        var root = (Microsoft.UI.Xaml.FrameworkElement)Content;
        root.ActualThemeChanged += (_, _) => UpdateCaptionColors(root.ActualTheme);
        UpdateCaptionColors(root.ActualTheme);

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

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Native caption rendering requires an interactive window; verified with light and dark app screenshots.")]
    private void UpdateCaptionColors(Microsoft.UI.Xaml.ElementTheme theme)
    {
        // Native caption buttons do not inherit the XAML root's requested theme.
        var foreground = new global::Windows.UI.ViewManagement.AccessibilitySettings().HighContrast
            ? new global::Windows.UI.ViewManagement.UISettings().GetColorValue(global::Windows.UI.ViewManagement.UIColorType.Foreground)
            : theme == Microsoft.UI.Xaml.ElementTheme.Dark ? Microsoft.UI.Colors.White : Microsoft.UI.Colors.Black;
        AppWindow.TitleBar.ButtonForegroundColor = foreground;
        AppWindow.TitleBar.ButtonInactiveForegroundColor = foreground;
        AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
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

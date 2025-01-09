// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
using Microsoft.UI.Xaml.Media;

namespace BSH.MainApp;

public sealed partial class MainWindow : WinUIEx.WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        SystemBackdrop = new MicaBackdrop();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/app_ico.ico"));
        Title = "AppDisplayName".GetLocalized();
    }

    private void MainNavigation_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer == nviOverviewPage)
        {
            App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.MainViewModel");
        }
        else if (args.InvokedItemContainer == nviBackupBrowser)
        {
            App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.BrowserViewModel");
        }
        else if (args.IsSettingsInvoked)
        {
            App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.SettingsViewModel");
        }
    }
}

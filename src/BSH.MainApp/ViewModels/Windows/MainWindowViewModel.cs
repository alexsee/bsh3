// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using BSH.MainApp.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    private static class ViewModelKeys
    {
        public const string Main = "BSH.MainApp.ViewModels.MainViewModel";
        public const string Browser = "BSH.MainApp.ViewModels.BrowserViewModel";
    }

    [ObservableProperty]
    private NavigationViewItem? currentPage;

    public ObservableCollection<NavigationViewItem> NavigationItems { get; } = [];

    public MainWindowViewModel()
    {
        NavigationItems = [
            new NavigationViewItem { Tag = ViewModelKeys.Main, Icon = new SymbolIcon(Symbol.Home), Content = "Overview" },
            new NavigationViewItem { Tag = ViewModelKeys.Browser, Icon = new SymbolIcon(Symbol.BrowsePhotos), Content = "Backup browser" },
        ];
        CurrentPage = NavigationItems[0];
    }

    [RelayCommand]
    private void NavigateToMainPage(NavigationViewItemInvokedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.IsSettingsInvoked)
        {
            App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.SettingsViewModel");
            return;
        }

        if (args.InvokedItemContainer.Tag == null)
        {
            return;
        }

        var page = args.InvokedItemContainer.Tag.ToString();
        if (!string.IsNullOrEmpty(page))
        {
            App.GetService<INavigationService>().NavigateTo(page);
        }
    }
}

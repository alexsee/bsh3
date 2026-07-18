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
        public const string Settings = "BSH.MainApp.ViewModels.SettingsViewModel";
    }

    public static class SupportActionKeys
    {
        public const string About = "Support.About";
        public const string Help = "Support.Help";
        public const string EventLog = "Support.EventLog";
        public const string CheckForUpdates = "Support.CheckForUpdates";
        public const string ClearStoredPassword = "Support.ClearStoredPassword";
        public const string ResetUniqueUserId = "Support.ResetUniqueUserId";
        public const string ResetConfiguration = "Support.ResetConfiguration";
    }

    public static IReadOnlyList<string> SupportActionTags
    {
        get;
    } =
    [
        SupportActionKeys.About,
        SupportActionKeys.Help,
        SupportActionKeys.EventLog,
        SupportActionKeys.CheckForUpdates,
        SupportActionKeys.ClearStoredPassword,
        SupportActionKeys.ResetUniqueUserId,
        SupportActionKeys.ResetConfiguration
    ];

    private readonly IPresentationService? presentationService;
    private readonly INavigationService? navigationService;
    private readonly IAppExtrasService? appExtrasService;

    [ObservableProperty]
    private NavigationViewItem? currentPage;

    public ObservableCollection<NavigationViewItem> NavigationItems { get; } = [];
    public ObservableCollection<NavigationViewItem> FooterNavigationItems { get; } = [];

    public MainWindowViewModel(
        IPresentationService? presentationService = null,
        INavigationService? navigationService = null,
        IAppExtrasService? appExtrasService = null,
        bool buildNavigationItems = true)
    {
        this.presentationService = presentationService;
        this.navigationService = navigationService;
        this.appExtrasService = appExtrasService;

        if (!buildNavigationItems)
        {
            return;
        }

        NavigationItems = [
            new NavigationViewItem { Tag = ViewModelKeys.Main, Icon = new SymbolIcon(Symbol.Home), Content = "Overview" },
            new NavigationViewItem { Tag = ViewModelKeys.Browser, Icon = new SymbolIcon(Symbol.BrowsePhotos), Content = "Backup browser" },
        ];
        FooterNavigationItems = [
            new NavigationViewItem
            {
                SelectsOnInvoked = false,
                Icon = new FontIcon { Glyph = "\uE946" },
                Content = "Extras and Support",
                MenuItems =
                {
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.About, Icon = new FontIcon { Glyph = "\uE946" }, Content = "About Backup Service Home 3" },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.Help, Icon = new SymbolIcon(Symbol.Link), Content = "Help and Support" },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.EventLog, Icon = new SymbolIcon(Symbol.Document), Content = "Show Event Logs" },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.CheckForUpdates, Icon = new SymbolIcon(Symbol.Sync), Content = "Check for Updates" },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.ClearStoredPassword, Icon = new SymbolIcon(Symbol.Permissions), Content = "Clear Stored Password" },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.ResetUniqueUserId, Icon = new SymbolIcon(Symbol.Contact), Content = "Reset Unique User Id" },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.ResetConfiguration, Icon = new SymbolIcon(Symbol.Delete), Content = "Reset Configuration" },
                }
            }
        ];
        CurrentPage = NavigationItems[0];
    }

    [RelayCommand]
    private void NavigateToMainPage(NavigationViewItemInvokedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.IsSettingsInvoked)
        {
            GetNavigationService().NavigateTo(ViewModelKeys.Settings);
            return;
        }

        if (args.InvokedItemContainer.Tag == null)
        {
            return;
        }

        var page = args.InvokedItemContainer.Tag.ToString();
        if (HandleSupportAction(page))
        {
            return;
        }

        if (!string.IsNullOrEmpty(page))
        {
            GetNavigationService().NavigateTo(page);
        }
    }

    public bool HandleSupportAction(string? action)
    {
        switch (action)
        {
            case SupportActionKeys.About:
                _ = GetPresentationService().ShowAboutWindowAsync();
                return true;
            case SupportActionKeys.Help:
                _ = GetPresentationService().OpenHelpSupportAsync();
                return true;
            case SupportActionKeys.EventLog:
                _ = GetPresentationService().OpenCurrentEventLogAsync();
                return true;
            case SupportActionKeys.CheckForUpdates:
                _ = GetAppExtrasService().CheckForUpdatesAsync();
                return true;
            case SupportActionKeys.ClearStoredPassword:
                _ = GetAppExtrasService().ClearStoredPasswordAsync();
                return true;
            case SupportActionKeys.ResetUniqueUserId:
                _ = GetAppExtrasService().ResetUniqueUserIdAsync();
                return true;
            case SupportActionKeys.ResetConfiguration:
                _ = GetPresentationService().ResetConfigurationAsync();
                return true;
            default:
                return false;
        }
    }

    private IPresentationService GetPresentationService()
    {
        return presentationService ?? App.GetService<IPresentationService>();
    }

    private INavigationService GetNavigationService()
    {
        return navigationService ?? App.GetService<INavigationService>();
    }

    private IAppExtrasService GetAppExtrasService()
    {
        return appExtrasService ?? App.GetService<IAppExtrasService>();
    }
}

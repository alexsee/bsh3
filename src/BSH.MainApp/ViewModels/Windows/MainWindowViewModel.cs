// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Runtime.Ports;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
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
    private readonly IUpdateService? updateService;
    private readonly IStoredPasswordAdapter? storedPasswordAdapter;

    [ObservableProperty]
    private NavigationViewItem? currentPage;

    public ObservableCollection<NavigationViewItem> NavigationItems { get; } = [];
    public ObservableCollection<NavigationViewItem> FooterNavigationItems { get; } = [];

    public MainWindowViewModel(
        IPresentationService? presentationService = null,
        INavigationService? navigationService = null,
        IUpdateService? updateService = null,
        IStoredPasswordAdapter? storedPasswordAdapter = null,
        bool buildNavigationItems = true)
    {
        this.presentationService = presentationService;
        this.navigationService = navigationService;
        this.updateService = updateService;
        this.storedPasswordAdapter = storedPasswordAdapter;

        if (!buildNavigationItems)
        {
            return;
        }

        NavigationItems = [
            new NavigationViewItem { Tag = ViewModelKeys.Main, Icon = new SymbolIcon(Symbol.Home), Content = "Nav_Overview".GetLocalized() },
            new NavigationViewItem { Tag = ViewModelKeys.Browser, Icon = new SymbolIcon(Symbol.BrowsePhotos), Content = "Nav_BackupBrowser".GetLocalized() },
        ];
        FooterNavigationItems = [
            new NavigationViewItem
            {
                SelectsOnInvoked = false,
                Icon = new FontIcon { Glyph = "\uE946" },
                Content = "Nav_ExtrasAndSupport".GetLocalized(),
                MenuItems =
                {
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.About, Icon = new FontIcon { Glyph = "\uE946" }, Content = "Support_About".GetLocalized() },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.Help, Icon = new SymbolIcon(Symbol.Link), Content = "Support_Help".GetLocalized() },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.EventLog, Icon = new SymbolIcon(Symbol.Document), Content = "Support_EventLog".GetLocalized() },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.CheckForUpdates, Icon = new SymbolIcon(Symbol.Sync), Content = "Support_CheckForUpdates".GetLocalized() },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.ClearStoredPassword, Icon = new SymbolIcon(Symbol.Permissions), Content = "Support_ClearStoredPassword".GetLocalized() },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.ResetUniqueUserId, Icon = new SymbolIcon(Symbol.Contact), Content = "Support_ResetUniqueUserId".GetLocalized() },
                    new NavigationViewItem { SelectsOnInvoked = false, Tag = SupportActionKeys.ResetConfiguration, Icon = new SymbolIcon(Symbol.Delete), Content = "Support_ResetConfiguration".GetLocalized() },
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
                _ = GetUpdateService().CheckAsync(notifyWhenUpToDate: true);
                return true;
            case SupportActionKeys.ClearStoredPassword:
                _ = GetStoredPasswordAdapter().StorePasswordAsync(string.Empty);
                return true;
            case SupportActionKeys.ResetUniqueUserId:
                _ = GetUpdateService().ResetUniqueUserIdAsync();
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

    private IUpdateService GetUpdateService()
    {
        return updateService ?? App.GetService<IUpdateService>();
    }

    private IStoredPasswordAdapter GetStoredPasswordAdapter()
    {
        return storedPasswordAdapter ?? App.GetService<IStoredPasswordAdapter>();
    }
}

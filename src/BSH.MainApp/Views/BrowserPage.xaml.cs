// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;
using BSH.MainApp.Utils;
using BSH.MainApp.ViewModels;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BSH.MainApp.Views;

public sealed partial class BrowserPage : Page
{
    private XamlRoot? iconXamlRoot;
    private double iconRasterizationScale;

    public BrowserViewModel ViewModel { get; } = App.GetService<BrowserViewModel>();

    public BrowserPage()
    {
        InitializeComponent();
        Loaded += BrowserPage_Loaded;
        Unloaded += BrowserPage_Unloaded;
    }

    private async void BrowserPage_Loaded(object sender, RoutedEventArgs e)
    {
        iconXamlRoot = XamlRoot;
        iconXamlRoot.Changed += IconXamlRoot_Changed;
        await RefreshIconsForDisplayScaleAsync();
    }

    private void BrowserPage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (iconXamlRoot != null)
        {
            iconXamlRoot.Changed -= IconXamlRoot_Changed;
            iconXamlRoot = null;
        }

        iconRasterizationScale = 0;
    }

    private async void IconXamlRoot_Changed(XamlRoot sender, XamlRootChangedEventArgs args)
    {
        await RefreshIconsForDisplayScaleAsync();
    }

    private async Task RefreshIconsForDisplayScaleAsync()
    {
        var scale = iconXamlRoot?.RasterizationScale ?? 1;
        if (scale == iconRasterizationScale)
        {
            return;
        }

        iconRasterizationScale = scale;
        var dpi = (uint)Math.Round(scale * 96);
        foreach (var item in ViewModel.Items.Where(item => item.IsFile).ToArray())
        {
            item.Icon16 = await FileSystemIconHelpers.GetFileIconAsync(item.FileNameOnDrive, 16, dpi);
            item.Icon64 = await FileSystemIconHelpers.GetFileIconAsync(item.FileNameOnDrive, 64, dpi);
        }
    }

    private ListViewSelectionMode GetFilesSelectionMode(bool isMultiSelectMode) =>
        isMultiSelectMode ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;

    private async void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        await ViewModel.LoadFolderWithParamCommand.ExecuteAsync(args.Item);
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        await ViewModel.CommitSearchCommand.ExecuteAsync(null);
    }

    private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedItems.Clear();
        foreach (var item in FilesListView.SelectedItems.OfType<FileOrFolderItem>())
        {
            ViewModel.SelectedItems.Add(item);
        }

        if (FilesListView.SelectedItems.Count == 1)
        {
            ViewModel.CurrentItem = FilesListView.SelectedItems.OfType<FileOrFolderItem>().First();
        }
        else if (FilesListView.SelectedItems.Count == 0)
        {
            ViewModel.CurrentItem = null;
        }
    }

    private void FilesListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement { DataContext: FileOrFolderItem item })
        {
            return;
        }

        if (!FilesListView.SelectedItems.Contains(item))
        {
            FilesListView.SelectedItem = item;
        }

        ViewModel.CurrentItem = item;
    }

    private void FavoritesListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // Make the context menu act on the right-clicked favorite rather than the
        // currently selected one by selecting it before the flyout opens.
        if (e.OriginalSource is FrameworkElement { DataContext: BrowserFavoriteItem item })
        {
            ViewModel.CurrentFavorite = item;
        }
    }

    private void VersionsListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: VersionDetails version }
            && sender is ListView list)
        {
            list.SelectedItem = version;
        }
    }

    private async void JumpToFileVersion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: VersionDetails version })
        {
            await ViewModel.JumpToFileVersionCommand.ExecuteAsync(version);
        }
    }

    private async void RestoreFileVersion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: VersionDetails version })
        {
            await ViewModel.RestoreFileVersionCommand.ExecuteAsync(version);
        }
    }

    private async void VersionToolTip_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ToolTip { Content: VersionDetails version } tooltip
            || tooltip.FindDescendant<StackPanel>(static panel => panel.Name == "VersionChangesPanel") is not StackPanel panel)
        {
            return;
        }

        panel.Visibility = Visibility.Collapsed;
        try
        {
            var statistics = await ViewModel.GetVersionChangeStatisticsAsync(version.Id);
            if (tooltip.IsOpen && ReferenceEquals(tooltip.Content, version))
            {
                panel.DataContext = new BrowserVersionChangeStatistics(statistics.Added, statistics.Modified, statistics.Deleted);
                panel.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to load backup version change statistics");
        }
    }
}

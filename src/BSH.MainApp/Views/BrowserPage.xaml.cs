// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BSH.MainApp.Views;

public sealed partial class BrowserPage : Page
{
    public BrowserViewModel ViewModel { get; } = App.GetService<BrowserViewModel>();

    public BrowserPage()
    {
        InitializeComponent();
    }

    private async void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        await ViewModel.LoadFolderWithParamCommand.ExecuteAsync(args.Item);
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
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class FilterWindow : WindowEx
{
    private bool isValidationDialogOpen;

    public FilterViewModel ViewModel { get; } = App.GetService<FilterViewModel>();

    public FilterWindow()
    {
        InitializeComponent();
        ViewModel.ValidationFailed += ViewModel_ValidationFailed;
        ViewModel.ParentWindowId = this.AppWindow.Id;
        ContentFrame.DataContext = ViewModel;

        FilterSelectorBar.SelectedItem = FilesItem;
        NavigateToSelectedItem();
    }

    public async Task<bool> ShowDialogAsync()
    {
        Activate();
        this.CenterOnScreen();
        var result = await ViewModel.TaskCompletionSource.Task;

        Close();
        return result;
    }

    private void FilterSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        NavigateToSelectedItem();
    }

    private async void ViewModel_ValidationFailed(string message)
    {
        if (isValidationDialogOpen || Content.XamlRoot == null)
        {
            return;
        }

        isValidationDialogOpen = true;

        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Filter_ValidationError_Title".GetLocalized(),
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                },
                PrimaryButtonText = "MsgBox_OK".GetLocalized(),
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();
        }
        finally
        {
            isValidationDialogOpen = false;
        }
    }

    private void NavigateToSelectedItem()
    {
        var selected = FilterSelectorBar.SelectedItem;

        Type pageType;
        if (ReferenceEquals(selected, FoldersItem))
        {
            pageType = typeof(Views.FilterPages.ExcludeFoldersPage);
        }
        else if (ReferenceEquals(selected, FilesItem))
        {
            pageType = typeof(Views.FilterPages.ExcludeFilesPage);
        }
        else if (ReferenceEquals(selected, FileTypesItem))
        {
            pageType = typeof(Views.FilterPages.ExcludeFileTypesPage);
        }
        else if (ReferenceEquals(selected, MaxFileSizeItem))
        {
            pageType = typeof(Views.FilterPages.ExcludeMaxFileSizePage);
        }
        else
        {
            pageType = typeof(Views.FilterPages.ExcludeRegexPage);
        }

        ContentFrame.NavigateToType(pageType, null, new FrameNavigationOptions());
    }
}

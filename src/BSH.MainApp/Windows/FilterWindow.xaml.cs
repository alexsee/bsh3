// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class FilterWindow : WindowEx
{
    public FilterViewModel ViewModel { get; } = App.GetService<FilterViewModel>();

    public FilterWindow()
    {
        InitializeComponent();
        ViewModel.WindowHandle = this.GetWindowHandle();
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
        else
        {
            pageType = typeof(Views.FilterPages.ExcludeRegexPage);
        }

        ContentFrame.NavigateToType(pageType, null, new FrameNavigationOptions());
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class ScheduleEditorWindow : WindowEx
{
    private bool closeRequested;

    public ScheduleEditorViewModel ViewModel
    {
        get;
    } = App.GetService<ScheduleEditorViewModel>();

    public ScheduleEditorWindow()
    {
        InitializeComponent();
        ((FrameworkElement)Content).DataContext = ViewModel;
        Closed += OnClosed;

        ScheduleSelectorBar.SelectedItem = BackupTimesItem;
        ShowSelectedPanel();
    }

    public async Task<bool> ShowDialogAsync()
    {
        await ViewModel.InitializeAsync();

        Activate();
        this.CenterOnMainWindow();
        var result = await ViewModel.TaskCompletionSource.Task;

        // Save/Cancel complete the TCS while the window is still open; chrome close
        // (title-bar X / Alt+F4) already closed the window via the Closed handler.
        if (!closeRequested)
        {
            Close();
        }

        return result;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        closeRequested = true;
        ViewModel.TaskCompletionSource.TrySetResult(false);
    }

    private void ScheduleSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        ShowSelectedPanel();
    }

    private void ShowSelectedPanel()
    {
        BackupTimesPanel.Visibility = ReferenceEquals(ScheduleSelectorBar.SelectedItem, BackupTimesItem)
            ? Visibility.Visible
            : Visibility.Collapsed;
        RetentionPanel.Visibility = ReferenceEquals(ScheduleSelectorBar.SelectedItem, RetentionItem)
            ? Visibility.Visible
            : Visibility.Collapsed;
        FullBackupsPanel.Visibility = ReferenceEquals(ScheduleSelectorBar.SelectedItem, FullBackupsItem)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class ScheduleEditorWindow : WindowEx
{
    private readonly ModalCloseHandler closeHandler = new();

    public ScheduleEditorViewModel ViewModel
    {
        get;
    } = App.GetService<ScheduleEditorViewModel>();

    public ScheduleEditorWindow()
    {
        InitializeComponent();
        ((FrameworkElement)Content).DataContext = ViewModel;
        Closed += OnClosed;
    }

    public static InfoBarSeverity ToSeverity(bool hasWarning)
    {
        return hasWarning ? InfoBarSeverity.Warning : InfoBarSeverity.Informational;
    }

    public async Task<bool> ShowDialogAsync()
    {
        await ViewModel.InitializeAsync();

        Activate();
        this.CenterOnMainWindow();
        return await closeHandler.AwaitThenCloseAsync(ViewModel.TaskCompletionSource.Task, Close);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        closeHandler.HandleClosed(() => ViewModel.TaskCompletionSource.TrySetResult(false));
    }
}

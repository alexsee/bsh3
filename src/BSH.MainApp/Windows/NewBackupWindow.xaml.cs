// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.Windows;

public sealed partial class NewBackupWindow : WinUIEx.WindowEx
{
    private readonly ModalCloseHandler closeHandler = new();

    public NewBackupViewModel ViewModel { get; } = new NewBackupViewModel();

    public NewBackupWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public Task<bool> ShowDialogAsync()
    {
        Activate();
        this.CenterOnMainWindow();
        return closeHandler.AwaitThenCloseAsync(ViewModel.TaskCompletionSource.Task, Close);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        closeHandler.HandleClosed(ViewModel.OnWindowClosed);
    }
}

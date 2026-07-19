// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class SwitchStorageWindow : WindowEx
{
    private bool closeRequested;

    public SwitchStorageViewModel ViewModel { get; } = App.GetService<SwitchStorageViewModel>();

    public SwitchStorageWindow()
    {
        InitializeComponent();
        ((FrameworkElement)Content).DataContext = ViewModel;
        Closed += OnClosed;
    }

    public async Task<bool> ShowDialogAsync(string databaseFile)
    {
        ViewModel.Initialize(databaseFile);

        Activate();
        this.CenterOnScreen();
        var result = await ViewModel.TaskCompletionSource.Task;

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
}

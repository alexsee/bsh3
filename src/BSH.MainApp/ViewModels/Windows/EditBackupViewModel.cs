// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class EditBackupViewModel : ObservableObject
{
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string? description;

    public void OnWindowClosed()
    {
        TaskCompletionSource.TrySetResult(false);
    }

    [RelayCommand]
    private void Save()
    {
        TaskCompletionSource.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnWindowClosed();
    }
}

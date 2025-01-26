// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class EditBackupViewModel : ObservableObject
{
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string? description;

    [RelayCommand]
    private void Save()
    {
        TaskCompletionSource.SetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.SetResult(false);
    }
}

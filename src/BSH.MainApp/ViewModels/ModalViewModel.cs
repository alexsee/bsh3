// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels;

public abstract partial class ModalViewModel : ObservableObject
{
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

    public abstract string Title
    {
        get;
    }

    public abstract Task InitializeAsync();

    public abstract Task SaveConfigurationAsync();

    [RelayCommand]
    private async Task Save()
    {
        await SaveConfigurationAsync();
        TaskCompletionSource.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(false);
    }
}

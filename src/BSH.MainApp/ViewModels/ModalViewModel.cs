// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels;

public abstract partial class ModalViewModel : ObservableObject
{
    public TaskCompletionSource<(ModalViewModel, bool)> TaskCompletionSource { get; } = new TaskCompletionSource<(ModalViewModel, bool)>();

    public abstract string Title
    {
        get;
    }

    public abstract int Width
    {
        get;
    }

    public abstract int Height
    {
        get;
    }

    public abstract Task InitializeAsync();

    public async virtual Task SaveConfigurationAsync()
    {
        // do nothing per default
    }

    [RelayCommand]
    private async Task Save()
    {
        await SaveConfigurationAsync();
        TaskCompletionSource.TrySetResult((this, true));
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult((this, false));
    }
}

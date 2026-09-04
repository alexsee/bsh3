// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class RequestFileOverwriteViewModel : ObservableObject
{
    public TaskCompletionSource<RequestOverwriteResult> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private double destinationFileSize;

    [ObservableProperty]
    private DateTime destinationLastModified;

    [ObservableProperty]
    private double sourceFileSize;

    [ObservableProperty]
    private DateTime sourceLastModified;

    [ObservableProperty]
    private bool applyToAll = false;

    [RelayCommand]
    private void OverwriteFile()
    {
        if (!ApplyToAll)
        {
            TaskCompletionSource.TrySetResult(RequestOverwriteResult.Overwrite);
        }
        else
        {
            TaskCompletionSource.TrySetResult(RequestOverwriteResult.OverwriteAll);
        }
    }

    [RelayCommand]
    private void SkipFile()
    {
        if (!ApplyToAll)
        {
            TaskCompletionSource.TrySetResult(RequestOverwriteResult.NoOverwrite);
        }
        else
        {
            TaskCompletionSource.TrySetResult(RequestOverwriteResult.NoOverwriteAll);
        }
    }

    public void OnWindowClosed()
    {
        TaskCompletionSource.TrySetResult(RequestOverwriteResult.None);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnWindowClosed();
    }
}

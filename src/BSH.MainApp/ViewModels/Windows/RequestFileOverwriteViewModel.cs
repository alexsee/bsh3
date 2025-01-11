// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;
public partial class RequestFileOverwriteViewModel : ObservableRecipient
{
    public TaskCompletionSource<RequestOverwriteResult> TaskCompletionSource
    {
        get; set;
    }

    [ObservableProperty]
    private string fileName;

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
            TaskCompletionSource.SetResult(RequestOverwriteResult.Overwrite);
        }
        else
        {
            TaskCompletionSource.SetResult(RequestOverwriteResult.OverwriteAll);
        }
    }

    [RelayCommand]
    private void SkipFile()
    {
        if (!ApplyToAll)
        {
            TaskCompletionSource.SetResult(RequestOverwriteResult.NoOverwrite);
        }
        else
        {
            TaskCompletionSource.SetResult(RequestOverwriteResult.NoOverwriteAll);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.SetResult(RequestOverwriteResult.None);
    }
}

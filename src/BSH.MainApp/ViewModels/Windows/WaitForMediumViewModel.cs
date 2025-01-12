// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class WaitForMediumViewModel : ObservableObject
{
    public event OnCancelRequestedEventHandler OnCancelRequested;

    public delegate void OnCancelRequestedEventHandler();

    [RelayCommand]
    private void Cancel()
    {
        OnCancelRequested?.Invoke();
    }
}

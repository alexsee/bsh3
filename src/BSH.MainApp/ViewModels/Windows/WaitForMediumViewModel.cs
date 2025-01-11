// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class WaitForMediumViewModel
{
    public event EventHandler? OnCancelRequested;

    [RelayCommand]
    private void Cancel()
    {
        OnCancelRequested?.Invoke(this, EventArgs.Empty);
    }
}

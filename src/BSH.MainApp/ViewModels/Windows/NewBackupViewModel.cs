// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;

namespace BSH.MainApp.ViewModels.Windows;
public partial class NewBackupViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private bool? isFullBackup;

    [ObservableProperty]
    private bool? isShutdownPc;
}

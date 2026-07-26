// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;

namespace BSH.MainApp.Windows;

public sealed partial class EditBackupWindow : WinUIEx.WindowEx
{
    public EditBackupViewModel ViewModel { get; set; } = new EditBackupViewModel();

    public EditBackupWindow()
    {
        InitializeComponent();
    }

    public async Task<bool> ShowDialogAsync()
    {
        Activate();
        this.CenterOnMainWindow();
        var result = await ViewModel.TaskCompletionSource.Task;

        Close();
        return result;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class NewBackupWindow : WinUIEx.WindowEx
{
    public NewBackupViewModel ViewModel { get; } = new NewBackupViewModel();

    public NewBackupWindow()
    {
        InitializeComponent();
    }

    public async Task<bool> ShowDialogAsync()
    {
        Activate();
        this.CenterOnScreen();
        var result = await ViewModel.TaskCompletionSource.Task;

        Close();
        return result;
    }
}

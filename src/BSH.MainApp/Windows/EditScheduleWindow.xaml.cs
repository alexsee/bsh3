// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class EditScheduleWindow : WinUIEx.WindowEx
{
    public EditScheduleViewModel ViewModel { get; set; } = App.GetService<EditScheduleViewModel>();

    public EditScheduleWindow()
    {
        InitializeComponent();
    }

    public async Task<bool> ShowDialogAsync()
    {
        await ViewModel.InitializeAsync();

        Activate();
        this.CenterOnScreen();
        var result = await ViewModel.TaskCompletionSource.Task;

        Close();
        return result;
    }
}

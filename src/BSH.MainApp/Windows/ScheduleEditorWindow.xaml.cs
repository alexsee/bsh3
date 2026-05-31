// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class ScheduleEditorWindow : WindowEx
{
    public ScheduleEditorViewModel ViewModel
    {
        get;
    } = App.GetService<ScheduleEditorViewModel>();

    public ScheduleEditorWindow()
    {
        InitializeComponent();
        ((Microsoft.UI.Xaml.FrameworkElement)Content).DataContext = ViewModel;
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

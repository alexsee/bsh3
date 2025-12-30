// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using BSH.MainApp.ViewModels.Windows;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class FilterWindow : WindowEx
{
    public FilterViewModel ViewModel { get; } = new FilterViewModel();

    public FilterWindow()
    {
        InitializeComponent();
        ViewModel.WindowHandle = this.GetWindowHandle();
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

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class CompressionExclusionsWindow : WindowEx
{
    private const int GwlpHwndParent = -8;

    public CompressionExclusionsViewModel ViewModel { get; } = App.GetService<CompressionExclusionsViewModel>();

    public CompressionExclusionsWindow()
    {
        InitializeComponent();
        SetWindowLongPtr(this.GetWindowHandle(), GwlpHwndParent, App.MainWindow.GetWindowHandle());

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsModal = true;
        }

        Closed += (_, _) => ViewModel.CancelCommand.Execute(null);
    }

    public async Task<bool> ShowDialogAsync()
    {
        Activate();
        this.CenterOnScreen();
        var result = await ViewModel.TaskCompletionSource.Task;

        Close();
        return result;
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint windowHandle, int index, nint newValue);
}

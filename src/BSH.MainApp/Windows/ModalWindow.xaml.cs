// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSH.MainApp.Windows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ModalWindow : WindowEx
{
    public ModalViewModel ViewModel
    {
        get; set;
    }

    public ModalWindow(ModalViewModel viewModel)
    {
        this.ViewModel = viewModel;
        InitializeComponent();
    }

    public static async Task<(T, bool)> ShowDialogAsync<T>(Type pageType) where T : ModalViewModel
    {
        var viewModel = App.GetService<T>();
        await viewModel.InitializeAsync();

        var window = new ModalWindow(viewModel);
        window.ModalFramePage.DataContext = viewModel;
        window.ModalFramePage.Navigate(pageType, viewModel);

        window.Title = viewModel.Title;
        window.Width = viewModel.Width;
        window.Height = viewModel.Height;

        window.Activate();
        window.CenterOnScreen();
        var result = await viewModel.TaskCompletionSource.Task;

        window.Close();
        return ((T, bool))result;
    }
}

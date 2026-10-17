// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class FilterWindow : WindowEx
{
    private readonly ModalCloseHandler closeHandler = new();
    private bool isValidationDialogOpen;

    public FilterViewModel ViewModel { get; } = App.GetService<FilterViewModel>();

    public FilterWindow()
    {
        InitializeComponent();
        ViewModel.ValidationFailed += ViewModel_ValidationFailed;
        ViewModel.ParentWindowId = this.AppWindow.Id;
        ((FrameworkElement)Content).DataContext = ViewModel;
        Closed += OnClosed;
    }

    public Task<bool> ShowDialogAsync()
    {
        Activate();
        this.CenterOnMainWindow();
        return closeHandler.AwaitThenCloseAsync(ViewModel.TaskCompletionSource.Task, Close);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        closeHandler.HandleClosed(ViewModel.OnWindowClosed);
    }

    private async void ViewModel_ValidationFailed(string message)
    {
        if (isValidationDialogOpen || Content.XamlRoot == null)
        {
            return;
        }

        isValidationDialogOpen = true;

        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Filter_ValidationError_Title".GetLocalized(),
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                },
                PrimaryButtonText = "MsgBox_OK".GetLocalized(),
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();
        }
        finally
        {
            isValidationDialogOpen = false;
        }
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels.Windows;
using BSH.MainApp.Windows;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public class PresentationService : IPresentationService
{
    public async Task ShowStatusWindow()
    {
    }

    public TaskCompleteAction CloseStatusWindow()
    {
        return TaskCompleteAction.NoAction;
    }

    public async Task ShowMainWindow()
    {
        App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.MainViewModel");
        await App.GetService<IActivationService>().ActivateAsync(null);
    }

    public async Task CloseMainWindow()
    {
    }

    public async Task ShowBackupBrowserWindow()
    {
        await App.GetService<IActivationService>().ActivateAsync(null);
        App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.BrowserViewModel");
    }

    public async Task CloseBackupBrowserWindow()
    {
    }

    public async Task ShowAboutWindow()
    {
    }

    public async Task<(string? password, bool persist)> RequestPassword()
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var requestPasswordWindow = new RequestPasswordWindow();
            requestPasswordWindow.XamlRoot = App.MainWindow.Content.XamlRoot;

            var result = await requestPasswordWindow.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return (requestPasswordWindow.ViewModel.Password, requestPasswordWindow.ViewModel.Persist ?? false);
            }

            return (null, false);
        });
    }

    public async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new RequestFileOverwriteWindow();
            return await dialog.ShowDialogAsync(localFile, remoteFile);
        });
    }

    public async Task ShowErrorInsufficientDiskSpace()
    {
    }

    public async Task<(bool, NewBackupViewModel)> ShowCreateBackupWindow()
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new NewBackupWindow();
            return (await dialog.ShowDialogAsync(), dialog.ViewModel);
        });
    }

    public async Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            if (commands != null && commands.Count > 3)
            {
                throw new InvalidOperationException("A maximum of 3 commands can be specified");
            }

            IUICommand defaultCommand = new UICommand("OK");
            IUICommand? secondaryCommand = null;
            IUICommand? cancelCommand = null;
            if (commands != null)
            {
                defaultCommand = commands.Count > defaultCommandIndex ? commands[(int)defaultCommandIndex] : commands.FirstOrDefault() ?? defaultCommand;
                cancelCommand = commands.Count > cancelCommandIndex ? commands[(int)cancelCommandIndex] : null;
                secondaryCommand = commands.FirstOrDefault(c => c != defaultCommand && c != cancelCommand);
            }
            var dialog = new ContentDialog();
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            dialog.Content = new TextBlock() { Text = content };
            dialog.Title = title;
            dialog.PrimaryButtonText = defaultCommand.Label;
            if (secondaryCommand != null)
            {
                dialog.SecondaryButtonText = secondaryCommand.Label;
            }
            if (cancelCommand != null)
            {
                dialog.CloseButtonText = cancelCommand.Label;
            }
            return await dialog.ShowAsync();
        });
    }
}

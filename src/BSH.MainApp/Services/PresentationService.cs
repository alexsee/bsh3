// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Windows;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public class PresentationService : IPresentationService
{
    public void ShowStatusWindow()
    {
    }

    public TaskCompleteAction CloseStatusWindow()
    {
        return TaskCompleteAction.NoAction;
    }

    public void ShowMainWindow()
    {
    }

    public void CloseMainWindow()
    {
    }

    public void ShowBackupBrowserWindow()
    {
    }

    public void CloseBackupBrowserWindow()
    {
    }


    public void ShowAboutWindow()
    {
    }

    public async Task<(string? password, bool persist)> RequestPassword()
    {
        var requestPasswordWindow = new RequestPasswordWindow();
        requestPasswordWindow.XamlRoot = App.MainWindow.Content.XamlRoot;

        var result = await requestPasswordWindow.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return (requestPasswordWindow.ViewModel.Password, requestPasswordWindow.ViewModel.Persist ?? false);
        }

        return (null, false);
    }

    public async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        var dialog = new RequestFileOverwriteWindow();
        return await dialog.ShowDialogAsync(localFile, remoteFile);
    }

    public void ShowErrorInsufficientDiskSpace()
    {
    }

    public async Task ShowCreateBackupWindow()
    {
        var newBackupWindow = new NewBackupWindow();
        newBackupWindow.AppWindow.Show();
    }

    public async Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
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
    }
}

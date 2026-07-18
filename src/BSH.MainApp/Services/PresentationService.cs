// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using System.Reflection;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels.Windows;
using BSH.MainApp.Windows;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;
using WinUIEx;

namespace BSH.MainApp.Services;

public class PresentationService : IPresentationService
{
    private StatusWindow? statusWindow;

    public async Task ShowStatusWindowAsync()
    {
        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            statusWindow = new StatusWindow();
            statusWindow.Activate();
            statusWindow.CenterOnScreen();
        });
    }

    public async Task<TaskCompleteAction> CloseStatusWindowAsync()
    {
        var action = TaskCompleteAction.NoAction;
        if (statusWindow != null)
        {
            await statusWindow.DispatcherQueue.EnqueueAsync(() =>
            {
                action = statusWindow.ViewModel.SelectedCompletionAction;
                statusWindow.Close();
            });

            App.GetService<IStatusService>().RemoveObserver(statusWindow.ViewModel);
            statusWindow = null;
        }
        return action;
    }

    public async Task ShowMainWindowAsync()
    {
        App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.MainViewModel");
        await App.GetService<IActivationService>().ActivateAsync(null);
    }

    public async Task CloseMainWindowAsync()
    {
        await App.MainWindow.DispatcherQueue.EnqueueAsync(() =>
        {
            App.MainWindow.Close();
        });
    }

    public async Task ShowBackupBrowserWindowAsync()
    {
        await App.GetService<IActivationService>().ActivateAsync(null);
        App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.BrowserViewModel");
    }

    public async Task CloseBackupBrowserWindowAsync()
    {
        await ShowMainWindowAsync();
    }

    public async Task ShowAboutWindowAsync()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        await ShowMessageBoxAsync(
            "About Backup Service Home 3",
            $"Backup Service Home 3{Environment.NewLine}Version {version}",
            [new UICommand("OK")]);
    }

    public Task OpenHelpSupportAsync()
    {
        OpenShellTarget("https://www.brightbits.de/?pk_campaign=software_link&pk_kwd=menu_help&pk_source=bsh-3");
        return Task.CompletedTask;
    }

    public Task OpenCurrentEventLogAsync()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var logFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Alexosoft",
            "Backup Service Home 3",
            $"log{date}.txt");

        OpenShellTarget(logFile);
        return Task.CompletedTask;
    }

    public async Task ResetConfigurationAsync()
    {
        var result = await ShowMessageBoxAsync(
            "Reset Configuration",
            "This deletes the current configuration and backup database. Do you want to continue?",
            [new UICommand("Yes"), new UICommand("No")],
            defaultCommandIndex: 0,
            cancelCommandIndex: 1);

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        DbClientFactory.ClosePool();
        DeleteDatabaseFiles(App.DatabaseFile);

        await App.GetService<IDbClientFactory>().InitializeAsync(App.DatabaseFile);
        await App.GetService<IConfigurationManager>().InitializeAsync();
        App.GetService<IStatusService>().Initialize();
        App.GetService<SetupRouting>().NavigateToSetup();
    }

    public async Task<(string? password, bool persist)> RequestPasswordAsync()
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

    public async Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new RequestFileOverwriteWindow();
            return await dialog.ShowDialogAsync(localFile, remoteFile);
        });
    }

    public async Task ShowErrorInsufficientDiskSpaceAsync()
    {
        await ShowMessageBoxAsync(
            "Insufficient Storage Space on Backup Medium",
            "The backup medium does not have enough free space for this operation.",
            [new UICommand("OK")]);
    }

    public async Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files)
    {
        if (files.Count == 0)
        {
            return;
        }

        await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var list = new ListView
            {
                MaxHeight = 360,
                ItemsSource = files.Select(entry => $"{entry.File.FileNamePath()}{Environment.NewLine}{GetExceptionMessage(entry)}").ToList()
            };

            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Files could not be copied",
                Content = list,
                PrimaryButtonText = "OK"
            };

            await dialog.ShowAsync();
        });
    }

    public async Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync()
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new NewBackupWindow();
            return (await dialog.ShowDialogAsync(), dialog.ViewModel);
        });
    }

    public async Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new EditBackupWindow();
            dialog.ViewModel = backupViewModel;
            return (await dialog.ShowDialogAsync(), dialog.ViewModel);
        });
    }

    public async Task<bool> ShowDeleteBackupWindowAsync()
    {
        var messageBoxResult = await ShowMessageBoxAsync("Delete Backup", "Are you sure you want to delete this backup?", new List<IUICommand> { new UICommand("Yes"), new UICommand("No") });
        return messageBoxResult == ContentDialogResult.Primary;
    }

    public async Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            ValidateCommands(commands);
            var (defaultCommand, secondaryCommand, cancelCommand) = ResolveCommands(commands, defaultCommandIndex, cancelCommandIndex);
            var dialog = BuildDialog(title, content, defaultCommand, secondaryCommand, cancelCommand);
            var result = await dialog.ShowAsync();
            InvokeCommandForResult(result, defaultCommand, secondaryCommand, cancelCommand);
            return result;
        });
    }

    private static void ValidateCommands(IList<IUICommand>? commands)
    {
        if (commands != null && commands.Count > 3)
        {
            throw new InvalidOperationException("A maximum of 3 commands can be specified");
        }
    }

    private static (IUICommand defaultCommand, IUICommand? secondaryCommand, IUICommand? cancelCommand) ResolveCommands(
        IList<IUICommand>? commands,
        uint defaultCommandIndex,
        uint cancelCommandIndex)
    {
        IUICommand defaultCommand = new UICommand("OK");
        IUICommand? cancelCommand = null;

        if (commands == null)
        {
            return (defaultCommand, null, cancelCommand);
        }

        defaultCommand = commands.Count > defaultCommandIndex
            ? commands[(int)defaultCommandIndex]
            : commands.FirstOrDefault() ?? defaultCommand;

        cancelCommand = commands.Count > cancelCommandIndex
            ? commands[(int)cancelCommandIndex]
            : null;

        var secondaryCommand = commands.FirstOrDefault(c => c != defaultCommand && c != cancelCommand);
        return (defaultCommand, secondaryCommand, cancelCommand);
    }

    private static ContentDialog BuildDialog(
        string title,
        string content,
        IUICommand defaultCommand,
        IUICommand? secondaryCommand,
        IUICommand? cancelCommand)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Content = new TextBlock { Text = content },
            Title = title,
            PrimaryButtonText = defaultCommand.Label
        };

        if (secondaryCommand != null)
        {
            dialog.SecondaryButtonText = secondaryCommand.Label;
        }

        if (cancelCommand != null)
        {
            dialog.CloseButtonText = cancelCommand.Label;
        }

        return dialog;
    }

    private static void InvokeCommandForResult(
        ContentDialogResult result,
        IUICommand defaultCommand,
        IUICommand? secondaryCommand,
        IUICommand? cancelCommand)
    {
        var command = result switch
        {
            ContentDialogResult.Primary => defaultCommand,
            ContentDialogResult.Secondary => secondaryCommand,
            ContentDialogResult.None => cancelCommand,
            _ => null
        };

        command?.Invoked?.Invoke(command);
    }

    private static string GetExceptionMessage(FileExceptionEntry entry)
    {
        if (entry.Exception is FileNotProcessedException && entry.Exception.InnerException != null)
        {
            return entry.Exception.InnerException.Message;
        }

        return entry.Exception.Message;
    }

    private static void OpenShellTarget(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        }
        catch
        {
            // Opening support/log targets is best-effort, matching legacy behavior.
        }
    }

    private static void DeleteDatabaseFiles(string databaseFile)
    {
        foreach (var path in new[] { databaseFile, databaseFile + "-wal", databaseFile + "-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public async Task ShowExcludeFileFolderWindowAsync()
    {
        await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new FilterWindow();
            await dialog.ShowDialogAsync();
        });
    }

    public async Task ShowScheduleEditorWindowAsync()
    {
        await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new ScheduleEditorWindow();
            await dialog.ShowDialogAsync();
        });
    }
}

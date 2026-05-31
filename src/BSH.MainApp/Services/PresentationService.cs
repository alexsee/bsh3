// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
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
        if (statusWindow != null)
        {
            await statusWindow.DispatcherQueue.EnqueueAsync(() =>
            {
                statusWindow.Close();
            });

            App.GetService<IStatusService>().RemoveObserver(statusWindow.ViewModel);
            statusWindow = null;
        }
        return TaskCompleteAction.NoAction;
    }

    public async Task ShowMainWindowAsync()
    {
        App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.MainViewModel");
        await App.GetService<IActivationService>().ActivateAsync(null);
    }

    public async Task CloseMainWindowAsync()
    {
    }

    public async Task ShowBackupBrowserWindowAsync()
    {
        await App.GetService<IActivationService>().ActivateAsync(null);
        App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.BrowserViewModel");
    }

    public async Task CloseBackupBrowserWindowAsync()
    {
    }

    public async Task ShowAboutWindowAsync()
    {
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

    public async Task<IReadOnlyList<string>> ShowDeleteBackupsWindowAsync(IReadOnlyList<VersionDetails> versions)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var versionList = new ListView
            {
                SelectionMode = ListViewSelectionMode.Multiple,
                ItemsSource = versions,
                DisplayMemberPath = nameof(VersionDetails.CreationDate),
                MaxHeight = 360
            };

            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Delete backups",
                Content = versionList,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return Array.Empty<string>();
            }

            var selected = versionList.SelectedItems.Cast<VersionDetails>().Select(x => x.Id).ToArray();
            if (selected.Length == 0)
            {
                return Array.Empty<string>();
            }

            var confirmDialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Delete backups",
                Content = new TextBlock { Text = "Are you sure you want to delete the selected backups?" },
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            return await confirmDialog.ShowAsync() == ContentDialogResult.Primary ? selected : Array.Empty<string>();
        });
    }

    public async Task<bool> ShowDeleteSelectedContentWindowAsync(FileOrFolderItem item)
    {
        var itemType = item.IsFile ? "file" : "folder";
        var messageBoxResult = await ShowMessageBoxAsync(
            "Delete from all backups",
            $"Are you sure you want to delete this {itemType} from all backups?",
            new List<IUICommand> { new UICommand("Yes"), new UICommand("No") });
        return messageBoxResult == ContentDialogResult.Primary;
    }

    public async Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var nameBox = new TextBox
            {
                Text = favorite.Name,
                Header = "Name",
                MinWidth = 320
            };

            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Rename favorite",
                Content = nameBox,
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary ? nameBox.Text : null;
        });
    }

    public async Task ShowFileDetailsAsync(FileDetails fileDetails)
    {
        await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = fileDetails.Name,
                CloseButtonText = "Close",
                Content = BuildFileDetailsContent(fileDetails)
            };

            await dialog.ShowAsync();
        });
    }

    public async Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            ValidateCommands(commands);
            var (defaultCommand, secondaryCommand, cancelCommand) = ResolveCommands(commands, defaultCommandIndex, cancelCommandIndex);
            var dialog = BuildDialog(title, content, defaultCommand, secondaryCommand, cancelCommand);
            return await dialog.ShowAsync();
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

    private static Grid BuildFileDetailsContent(FileDetails fileDetails)
    {
        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 16,
            MaxWidth = 560
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        AddDetailsRow(grid, "Restore path", fileDetails.RestorePath);
        AddDetailsRow(grid, "Type", fileDetails.Type);
        AddDetailsRow(grid, "Size", fileDetails.Size.ToString("N0") + " bytes");
        AddDetailsRow(grid, "Created", fileDetails.Created.ToString("g"));
        AddDetailsRow(grid, "Modified", fileDetails.Modified.ToString("g"));
        AddDetailsRow(grid, "Available versions", string.Join(Environment.NewLine, fileDetails.AvailableVersions.Select(x => $"{x.Id} - {x.CreationDate:g}")));

        return grid;
    }

    private static void AddDetailsRow(Grid grid, string label, string value)
    {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var labelBlock = new TextBlock
        {
            Text = label,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };
        Grid.SetRow(labelBlock, row);
        Grid.SetColumn(labelBlock, 0);

        var valueBlock = new TextBlock
        {
            Text = value,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
        };
        Grid.SetRow(valueBlock, row);
        Grid.SetColumn(valueBlock, 1);

        grid.Children.Add(labelBlock);
        grid.Children.Add(valueBlock);
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

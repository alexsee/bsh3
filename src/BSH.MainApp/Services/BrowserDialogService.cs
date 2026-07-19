// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public class BrowserDialogService : IBrowserDialogService
{
    private readonly IPresentationService presentationService;

    public BrowserDialogService(IPresentationService presentationService)
    {
        this.presentationService = presentationService;
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
                Title = "Browser_DeleteBackups_Title".GetLocalized(),
                Content = versionList,
                PrimaryButtonText = "MsgBox_Delete".GetLocalized(),
                CloseButtonText = "MsgBox_Cancel".GetLocalized(),
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
                Title = "Browser_DeleteBackups_Title".GetLocalized(),
                Content = new TextBlock { Text = "Browser_DeleteBackups_Confirm".GetLocalized() },
                PrimaryButtonText = "MsgBox_Yes".GetLocalized(),
                CloseButtonText = "MsgBox_No".GetLocalized()
            };

            return await confirmDialog.ShowAsync() == ContentDialogResult.Primary ? selected : Array.Empty<string>();
        });
    }

    public async Task<bool> ShowDeleteSelectedContentWindowAsync(FileOrFolderItem item)
    {
        var itemType = item.IsFile ? "Browser_DeleteFromAll_File".GetLocalized() : "Browser_DeleteFromAll_Folder".GetLocalized();
        var messageBoxResult = await presentationService.ShowMessageBoxAsync(
            "Browser_DeleteFromAll_Title".GetLocalized(),
            string.Format("Browser_DeleteFromAll_Confirm".GetLocalized() ?? "Browser_DeleteFromAll_Confirm", itemType),
            new List<IUICommand> { new UICommand("MsgBox_Yes".GetLocalized()), new UICommand("MsgBox_No".GetLocalized()) });
        return messageBoxResult == ContentDialogResult.Primary;
    }

    public async Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var nameBox = new TextBox
            {
                Text = favorite.Name,
                Header = "Browser_Column_Name".GetLocalized(),
                MinWidth = 320
            };

            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Browser_RenameFavorite_Title".GetLocalized(),
                Content = nameBox,
                PrimaryButtonText = "MsgBox_Rename".GetLocalized(),
                CloseButtonText = "MsgBox_Cancel".GetLocalized(),
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
                CloseButtonText = "MsgBox_Close".GetLocalized(),
                Content = BuildFileDetailsContent(fileDetails)
            };

            await dialog.ShowAsync();
        });
    }

    public async Task<string?> PickRestoreDestinationFolderAsync()
    {
        // Cannot show dialogs or pickers without a live XamlRoot; treat as cancel.
        if (App.MainWindow?.Content == null)
        {
            return null;
        }

        try
        {
            return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
            {
                var folderPicker = new FolderPicker(App.MainWindow.AppWindow.Id)
                {
                    SuggestedStartLocation = PickerLocationId.ComputerFolder
                };

                var folder = await folderPicker.PickSingleFolderAsync();
                return folder?.Path;
            });
        }
        catch (Exception ex)
        {
            if (App.MainWindow?.Content != null)
            {
                await presentationService.ShowMessageBoxAsync(
                    "Browser_RestoreDestination_Title".GetLocalized(),
                    string.Format("Browser_RestoreDestination_PickerFailed".GetLocalized() ?? "Browser_RestoreDestination_PickerFailed", ex.Message),
                    [new UICommand("MsgBox_OK".GetLocalized())]);
            }

            return null;
        }
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

        AddDetailsRow(grid, "Browser_FileDetails_RestorePath".GetLocalized(), fileDetails.RestorePath);
        AddDetailsRow(grid, "Browser_FileDetails_Type".GetLocalized(), fileDetails.Type);
        AddDetailsRow(grid, "Browser_Column_Size".GetLocalized(), string.Format("Browser_FileDetails_SizeBytes".GetLocalized() ?? "Browser_FileDetails_SizeBytes", fileDetails.Size.ToString("N0")));
        AddDetailsRow(grid, "Browser_FileDetails_Created".GetLocalized(), fileDetails.Created.ToString("g"));
        AddDetailsRow(grid, "Browser_FileDetails_Modified".GetLocalized(), fileDetails.Modified.ToString("g"));
        AddDetailsRow(grid, "Browser_FileDetails_AvailableVersions".GetLocalized(), string.Join(Environment.NewLine, fileDetails.AvailableVersions.Select(x => $"{x.Id} - {x.CreationDate:g}")));

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
}

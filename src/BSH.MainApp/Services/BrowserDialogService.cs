// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
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
                Title = ResourceExtensions.GetLocalized("Browser_DeleteBackups_Title"),
                Content = versionList,
                PrimaryButtonText = ResourceExtensions.GetLocalized("MsgBox_Delete"),
                CloseButtonText = ResourceExtensions.GetLocalized("MsgBox_Cancel"),
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
                Title = ResourceExtensions.GetLocalized("Browser_DeleteBackups_Title"),
                Content = new TextBlock { Text = ResourceExtensions.GetLocalized("Browser_DeleteBackups_Confirm") },
                PrimaryButtonText = ResourceExtensions.GetLocalized("MsgBox_Yes"),
                CloseButtonText = ResourceExtensions.GetLocalized("MsgBox_No")
            };

            return await confirmDialog.ShowAsync() == ContentDialogResult.Primary ? selected : Array.Empty<string>();
        });
    }

    public async Task<bool> ShowDeleteSelectedContentWindowAsync(FileOrFolderItem item)
    {
        var itemType = item.IsFile ? ResourceExtensions.GetLocalized("Browser_DeleteFromAll_File") : ResourceExtensions.GetLocalized("Browser_DeleteFromAll_Folder");
        var messageBoxResult = await presentationService.ShowMessageBoxAsync(
            ResourceExtensions.GetLocalized("Browser_DeleteFromAll_Title"),
            string.Format(ResourceExtensions.GetLocalized("Browser_DeleteFromAll_Confirm"), itemType),
            new List<IUICommand> { new UICommand(ResourceExtensions.GetLocalized("MsgBox_Yes")), new UICommand(ResourceExtensions.GetLocalized("MsgBox_No")) });
        return messageBoxResult == ContentDialogResult.Primary;
    }

    public async Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var nameBox = new TextBox
            {
                Text = favorite.Name,
                Header = ResourceExtensions.GetLocalized("Browser_Column_Name"),
                MinWidth = 320
            };

            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = ResourceExtensions.GetLocalized("Browser_RenameFavorite_Title"),
                Content = nameBox,
                PrimaryButtonText = ResourceExtensions.GetLocalized("MsgBox_Rename"),
                CloseButtonText = ResourceExtensions.GetLocalized("MsgBox_Cancel"),
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
                CloseButtonText = ResourceExtensions.GetLocalized("MsgBox_Close"),
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
                    ResourceExtensions.GetLocalized("Browser_RestoreDestination_Title"),
                    string.Format(ResourceExtensions.GetLocalized("Browser_RestoreDestination_PickerFailed"), ex.Message),
                    [new UICommand(ResourceExtensions.GetLocalized("MsgBox_OK"))]);
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

        AddDetailsRow(grid, ResourceExtensions.GetLocalized("Browser_FileDetails_RestorePath"), fileDetails.RestorePath);
        AddDetailsRow(grid, ResourceExtensions.GetLocalized("Browser_FileDetails_Type"), fileDetails.Type);
        AddDetailsRow(grid, ResourceExtensions.GetLocalized("Browser_Column_Size"), string.Format(ResourceExtensions.GetLocalized("Browser_FileDetails_SizeBytes"), fileDetails.Size.ToString("N0")));
        AddDetailsRow(grid, ResourceExtensions.GetLocalized("Browser_FileDetails_Created"), fileDetails.Created.ToString("g"));
        AddDetailsRow(grid, ResourceExtensions.GetLocalized("Browser_FileDetails_Modified"), fileDetails.Modified.ToString("g"));
        AddDetailsRow(grid, ResourceExtensions.GetLocalized("Browser_FileDetails_AvailableVersions"), string.Join(Environment.NewLine, fileDetails.AvailableVersions.Select(x => $"{x.Id} - {x.CreationDate:g}")));

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

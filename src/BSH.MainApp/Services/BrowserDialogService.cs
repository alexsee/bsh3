// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics.CodeAnalysis;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

[ExcludeFromCodeCoverage]
public class BrowserDialogService : IBrowserDialogService
{
    private const string DeleteScopeGroupName = "DeleteScope";

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

    public async Task<DeleteSelectedContentResult> ShowDeleteSelectedContentWindowAsync(
        FileOrFolderItem item,
        IReadOnlyList<VersionDetails> versions)
    {
        return await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var itemType = item.IsFile
                ? "Browser_DeleteFromAll_File".GetLocalized()
                : "Browser_DeleteFromAll_Folder".GetLocalized();

            var radios = CreateScopeRadios();
            var lastNBox = CreateNumberBox("Browser_DeleteFromRange_LastN_Value".GetLocalized(), Math.Min(3, Math.Max(1, versions.Count)), Math.Max(1, versions.Count));
            var lastDaysBox = CreateNumberBox("Browser_DeleteFromRange_LastDays_Value".GetLocalized(), 30, 3650);
            var versionList = new ListView
            {
                SelectionMode = ListViewSelectionMode.Multiple,
                ItemsSource = versions,
                DisplayMemberPath = nameof(VersionDetails.CreationDate),
                MaxHeight = 220,
                IsEnabled = false
            };

            void UpdateEnabledState()
            {
                lastNBox.IsEnabled = radios.LastN.IsChecked == true;
                lastDaysBox.IsEnabled = radios.LastDays.IsChecked == true;
                versionList.IsEnabled = radios.Selected.IsChecked == true;
            }

            radios.All.Checked += (_, _) => UpdateEnabledState();
            radios.LastN.Checked += (_, _) => UpdateEnabledState();
            radios.LastDays.Checked += (_, _) => UpdateEnabledState();
            radios.Selected.Checked += (_, _) => UpdateEnabledState();

            var panel = new StackPanel { Spacing = 8, MinWidth = 420 };
            panel.Children.Add(new TextBlock
            {
                Text = string.Format("Browser_DeleteFromRange_Intro".GetLocalized() ?? "Browser_DeleteFromRange_Intro", itemType),
                TextWrapping = TextWrapping.Wrap
            });
            panel.Children.Add(radios.All);
            panel.Children.Add(radios.LastN);
            panel.Children.Add(lastNBox);
            panel.Children.Add(radios.LastDays);
            panel.Children.Add(lastDaysBox);
            panel.Children.Add(radios.Selected);
            panel.Children.Add(versionList);

            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Browser_DeleteFromRange_Title".GetLocalized(),
                Content = panel,
                PrimaryButtonText = "MsgBox_Delete".GetLocalized(),
                CloseButtonText = "MsgBox_Cancel".GetLocalized(),
                DefaultButton = ContentDialogButton.Close
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return DeleteSelectedContentResult.Cancelled;
            }

            var scope = DeleteSingleScope.Resolve(
                GetSelectedMode(radios),
                versions,
                (int)Math.Max(1, lastNBox.Value),
                (int)Math.Max(1, lastDaysBox.Value),
                versionList.SelectedItems.Cast<VersionDetails>().Select(x => x.Id).ToArray());

            if (!scope.HasTargetVersions)
            {
                await ShowNoMatchingVersionsAsync();
                return DeleteSelectedContentResult.Cancelled;
            }

            var scopeResult = ToUiResult(scope);
            var confirmMessage = scopeResult.DeleteFromAllVersions
                ? string.Format("Browser_DeleteFromAll_Confirm".GetLocalized() ?? "Browser_DeleteFromAll_Confirm", itemType)
                : string.Format("Browser_DeleteFromRange_Confirm".GetLocalized() ?? "Browser_DeleteFromRange_Confirm", itemType, scopeResult.VersionIds.Count);

            var confirmDialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Browser_DeleteFromRange_Title".GetLocalized(),
                Content = new TextBlock { Text = confirmMessage, TextWrapping = TextWrapping.Wrap },
                PrimaryButtonText = "MsgBox_Yes".GetLocalized(),
                CloseButtonText = "MsgBox_No".GetLocalized()
            };

            return await confirmDialog.ShowAsync() == ContentDialogResult.Primary
                ? scopeResult
                : DeleteSelectedContentResult.Cancelled;
        });
    }

    private static (RadioButton All, RadioButton LastN, RadioButton LastDays, RadioButton Selected) CreateScopeRadios()
    {
        return (
            new RadioButton { Content = "Browser_DeleteFromRange_All".GetLocalized(), IsChecked = true, GroupName = DeleteScopeGroupName },
            new RadioButton { Content = "Browser_DeleteFromRange_LastN".GetLocalized(), GroupName = DeleteScopeGroupName },
            new RadioButton { Content = "Browser_DeleteFromRange_LastDays".GetLocalized(), GroupName = DeleteScopeGroupName },
            new RadioButton { Content = "Browser_DeleteFromRange_Selected".GetLocalized(), GroupName = DeleteScopeGroupName });
    }

    private static NumberBox CreateNumberBox(string header, double value, double maximum)
    {
        return new NumberBox
        {
            Header = header,
            Value = value,
            Minimum = 1,
            Maximum = maximum,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            IsEnabled = false
        };
    }

    private static DeleteSingleScopeMode GetSelectedMode(
        (RadioButton All, RadioButton LastN, RadioButton LastDays, RadioButton Selected) radios)
    {
        if (radios.LastN.IsChecked == true)
        {
            return DeleteSingleScopeMode.LastN;
        }

        if (radios.LastDays.IsChecked == true)
        {
            return DeleteSingleScopeMode.LastDays;
        }

        if (radios.Selected.IsChecked == true)
        {
            return DeleteSingleScopeMode.SelectedVersions;
        }

        return DeleteSingleScopeMode.AllVersions;
    }

    private static DeleteSelectedContentResult ToUiResult(DeleteSingleScopeResult scope)
    {
        if (scope.DeleteFromAllVersions)
        {
            return DeleteSelectedContentResult.AllVersions;
        }

        return DeleteSelectedContentResult.FromVersions(scope.VersionIds.Select(id => id.ToString()).ToArray());
    }

    private static async Task ShowNoMatchingVersionsAsync()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Title = "Browser_DeleteFromRange_Title".GetLocalized(),
            Content = new TextBlock
            {
                Text = "Browser_DeleteFromRange_NoVersions".GetLocalized(),
                TextWrapping = TextWrapping.Wrap
            },
            CloseButtonText = "MsgBox_OK".GetLocalized()
        };
        await dialog.ShowAsync();
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
            if (App.MainWindow.Content != null)
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

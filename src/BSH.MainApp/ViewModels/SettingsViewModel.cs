// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Contracts;
using BSH.MainApp.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinUIEx;

namespace BSH.MainApp.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationManager configurationManager;

    [ObservableProperty]
    private string selectedSource;

    public ObservableCollection<string> Sources { get; } = new();

    public SettingsViewModel(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    [RelayCommand]
    private async Task AddSourceFolder()
    {
        var folderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        var hwnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            if (this.Sources.Contains(folder.Path))
            {
                return;
            }

            this.Sources.Add(folder.Path);
            this.configurationManager.SourceFolder = string.Join("|", this.Sources);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSourceFolder))]
    private void DeleteSourceFolder()
    {
        if (string.IsNullOrEmpty(this.SelectedSource))
        {
            return;
        }

        this.Sources.Remove(this.SelectedSource);
        this.configurationManager.SourceFolder = string.Join("|", this.Sources);
    }

    private bool CanDeleteSourceFolder() => !string.IsNullOrEmpty(SelectedSource);

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        Array.ForEach(this.configurationManager.SourceFolder.Split("|"), this.Sources.Add);
    }
}

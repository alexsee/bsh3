// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BSH.MainApp.ViewModels.Windows;

public partial class FilterViewModel : ObservableObject
{
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

    public ObservableCollection<string> ExcludeFolders { get; } = [];

    public ObservableCollection<string> ExcludeFiles { get; } = [];

    public ObservableCollection<string> ExcludeFileTypes { get; } = [];

    public ObservableCollection<string> RegexPatterns { get; } = [];

    public IRelayCommand AddFolderCommand { get; }

    public IRelayCommand RemoveFolderCommand { get; }

    public IRelayCommand AddFileCommand { get; }

    public IRelayCommand RemoveFileCommand { get; }

    public IRelayCommand AddFileTypeCommand { get; }

    public IRelayCommand RemoveFileTypeCommand { get; }

    public IRelayCommand AddRegexCommand { get; }

    public IRelayCommand RemoveRegexCommand { get; }

    public IRelayCommand SaveCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IAsyncRelayCommand BrowseFolderCommand { get; }

    public IAsyncRelayCommand BrowseFileCommand { get; }

    public nint WindowHandle { get; set; }

    public FilterViewModel()
    {
        AddFolderCommand = new RelayCommand<string?>(AddFolder);
        RemoveFolderCommand = new RelayCommand(RemoveFolder, CanRemoveFolder);
        AddFileCommand = new RelayCommand<string?>(AddFile);
        RemoveFileCommand = new RelayCommand(RemoveFile, CanRemoveFile);
        AddFileTypeCommand = new RelayCommand<string?>(AddFileType);
        RemoveFileTypeCommand = new RelayCommand(RemoveFileType, CanRemoveFileType);
        AddRegexCommand = new RelayCommand<string?>(AddRegex);
        RemoveRegexCommand = new RelayCommand(RemoveRegex, CanRemoveRegex);
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        BrowseFolderCommand = new AsyncRelayCommand(BrowseFolderAsync);
        BrowseFileCommand = new AsyncRelayCommand(BrowseFileAsync);
    }

    private string? selectedFolder;
    public string? SelectedFolder
    {
        get => selectedFolder;
        set
        {
            if (SetProperty(ref selectedFolder, value))
            {
                RemoveFolderCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string? selectedFile;
    public string? SelectedFile
    {
        get => selectedFile;
        set
        {
            if (SetProperty(ref selectedFile, value))
            {
                RemoveFileCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string? selectedFileType;
    public string? SelectedFileType
    {
        get => selectedFileType;
        set
        {
            if (SetProperty(ref selectedFileType, value))
            {
                RemoveFileTypeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string? selectedRegexPattern;
    public string? SelectedRegexPattern
    {
        get => selectedRegexPattern;
        set
        {
            if (SetProperty(ref selectedRegexPattern, value))
            {
                RemoveRegexCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private void AddFolder(string? folder)
    {
        var trimmed = folder?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (ExcludeFolders.Contains(trimmed))
        {
            return;
        }

        ExcludeFolders.Add(trimmed);
    }

    private void RemoveFolder()
    {
        if (SelectedFolder == null)
        {
            return;
        }

        ExcludeFolders.Remove(SelectedFolder);
        SelectedFolder = null;
    }

    private bool CanRemoveFolder() => !string.IsNullOrEmpty(SelectedFolder);

    private void AddFile(string? file)
    {
        var trimmed = file?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (ExcludeFiles.Contains(trimmed))
        {
            return;
        }

        ExcludeFiles.Add(trimmed);
    }

    private void RemoveFile()
    {
        if (SelectedFile == null)
        {
            return;
        }

        ExcludeFiles.Remove(SelectedFile);
        SelectedFile = null;
    }

    private bool CanRemoveFile() => !string.IsNullOrEmpty(SelectedFile);

    private void AddFileType(string? fileType)
    {
        var trimmed = fileType?.Trim().TrimStart('*').TrimStart('.');
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (!Regex.IsMatch(trimmed, "^[A-Za-z0-9]+$"))
        {
            return;
        }

        if (ExcludeFileTypes.Contains(trimmed))
        {
            return;
        }

        ExcludeFileTypes.Add(trimmed);
    }

    private void RemoveFileType()
    {
        if (SelectedFileType == null)
        {
            return;
        }

        ExcludeFileTypes.Remove(SelectedFileType);
        SelectedFileType = null;
    }

    private bool CanRemoveFileType() => !string.IsNullOrEmpty(SelectedFileType);

    private void AddRegex(string? regex)
    {
        if (string.IsNullOrWhiteSpace(regex))
        {
            return;
        }

        var lines = regex.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)))
        {
            if (RegexPatterns.Contains(line))
            {
                continue;
            }

            try
            {
                _ = new Regex(line);
            }
            catch (ArgumentException)
            {
                continue;
            }

            RegexPatterns.Add(line);
        }
    }

    private void RemoveRegex()
    {
        if (SelectedRegexPattern == null)
        {
            return;
        }

        RegexPatterns.Remove(SelectedRegexPattern);
        SelectedRegexPattern = null;
    }

    private bool CanRemoveRegex() => !string.IsNullOrEmpty(SelectedRegexPattern);

    private void Save()
    {
        TaskCompletionSource.SetResult(true);
    }

    private void Cancel()
    {
        TaskCompletionSource.SetResult(false);
    }

    private async Task BrowseFolderAsync()
    {
        if (WindowHandle == 0)
        {
            return;
        }

        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add("*");

        InitializeWithWindow.Initialize(picker, WindowHandle);
        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            AddFolder(folder.Path);
        }
    }

    private async Task BrowseFileAsync()
    {
        if (WindowHandle == 0)
        {
            return;
        }

        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add("*");

        InitializeWithWindow.Initialize(picker, WindowHandle);
        var files = await picker.PickMultipleFilesAsync();
        if (files == null || files.Count == 0)
        {
            return;
        }

        foreach (var file in files)
        {
            AddFile(file.Path);
        }
    }
}

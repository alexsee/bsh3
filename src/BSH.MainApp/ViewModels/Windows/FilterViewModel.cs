// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Brightbits.BSH.Engine.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BSH.MainApp.ViewModels.Windows;

public partial class FilterViewModel : ObservableObject
{
    private readonly IConfigurationManager configurationManager;

    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

    private string? validationErrorMessage;
    public string? ValidationErrorMessage
    {
        get => validationErrorMessage;
        set => SetProperty(ref validationErrorMessage, value);
    }

    public ObservableCollection<string> ExcludeFolders { get; } = [];

    public ObservableCollection<string> ExcludeFiles { get; } = [];

    public ObservableCollection<string> ExcludeFileTypes { get; } = [];

    public ObservableCollection<string> RegexPatterns { get; } = [];

    private string? fileTypeInputText;
    public string? FileTypeInputText
    {
        get => fileTypeInputText;
        set => SetProperty(ref fileTypeInputText, value);
    }

    private string? regexInputText;
    public string? RegexInputText
    {
        get => regexInputText;
        set => SetProperty(ref regexInputText, value);
    }

    public IRelayCommand AddFolderCommand
    {
        get;
    }

    public IRelayCommand RemoveFolderCommand
    {
        get;
    }

    public IRelayCommand AddFileCommand
    {
        get;
    }

    public IRelayCommand RemoveFileCommand
    {
        get;
    }

    public IRelayCommand AddFileTypeCommand
    {
        get;
    }

    public IRelayCommand RemoveFileTypeCommand
    {
        get;
    }

    public IRelayCommand AddRegexCommand
    {
        get;
    }

    public IRelayCommand RemoveRegexCommand
    {
        get;
    }

    public IRelayCommand SaveCommand
    {
        get;
    }

    public IRelayCommand CancelCommand
    {
        get;
    }

    public IAsyncRelayCommand BrowseFolderCommand
    {
        get;
    }

    public IAsyncRelayCommand BrowseFileCommand
    {
        get;
    }

    public nint WindowHandle
    {
        get; set;
    }

    public FilterViewModel(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;

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

        LoadFromConfiguration();
    }

    public void LoadFromConfiguration()
    {
        ExcludeFolders.Clear();
        foreach (var entry in (configurationManager.ExcludeFolder ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ExcludeFolders.Add(entry);
        }

        ExcludeFiles.Clear();
        foreach (var entry in (configurationManager.ExcludeFile ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ExcludeFiles.Add(entry);
        }

        ExcludeFileTypes.Clear();
        foreach (var entry in (configurationManager.ExcludeFileTypes ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().TrimStart('*').TrimStart('.'))
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ExcludeFileTypes.Add(entry);
        }

        RegexPatterns.Clear();
        foreach (var entry in (configurationManager.ExcludeMask ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.Ordinal))
        {
            RegexPatterns.Add(entry);
        }
    }

    public void SaveToConfiguration()
    {
        configurationManager.ExcludeFolder = string.Join("|", ExcludeFolders.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        configurationManager.ExcludeFile = string.Join("|", ExcludeFiles.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        configurationManager.ExcludeFileTypes = string.Join("|", ExcludeFileTypes.Select(x => x.Trim().TrimStart('*').TrimStart('.')).Where(x => !string.IsNullOrEmpty(x)));
        configurationManager.ExcludeMask = string.Join("|", RegexPatterns.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
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

    private IReadOnlyList<string> GetSourceFolders()
    {
        return (configurationManager.SourceFolder ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
    }

    private static string NormalizeDirectoryPath(string path)
    {
        var full = Path.GetFullPath(path);
        full = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return full + Path.DirectorySeparatorChar;
    }

    private bool IsPathUnderSources(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return false;
        }

        var sources = GetSourceFolders();
        if (sources.Count == 0)
        {
            return true;
        }

        try
        {
            var candidate = NormalizeDirectoryPath(fullPath);
            foreach (var source in sources)
            {
                if (string.IsNullOrWhiteSpace(source))
                {
                    continue;
                }

                var normalizedSource = NormalizeDirectoryPath(source);
                if (candidate.StartsWith(normalizedSource, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private void AddFolder(string? folder)
    {
        ValidationErrorMessage = null;

        var trimmed = folder?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (!IsPathUnderSources(trimmed))
        {
            ValidationErrorMessage = "Selected folder is not within the configured source folders.";
            return;
        }

        if (ExcludeFolders.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
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
        ValidationErrorMessage = null;

        var trimmed = file?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (!IsPathUnderSources(trimmed))
        {
            ValidationErrorMessage = "Selected file is not within the configured source folders.";
            return;
        }

        if (ExcludeFiles.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
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

        if (!Regex.IsMatch(trimmed, "^[A-Za-z0-9]+$", RegexOptions.None, TimeSpan.FromSeconds(10)))
        {
            return;
        }

        if (ExcludeFileTypes.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        ExcludeFileTypes.Add(trimmed);

        FileTypeInputText = null;
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
                _ = new Regex(line, RegexOptions.None, TimeSpan.FromSeconds(10));
            }
            catch (ArgumentException)
            {
                continue;
            }

            RegexPatterns.Add(line);
        }

        RegexInputText = null;
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
        SaveToConfiguration();
        TaskCompletionSource.TrySetResult(true);
    }

    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(false);
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

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Brightbits.BSH.Engine.Contracts;
using BSH.MainApp.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class CompressionExclusionsViewModel : ObservableObject
{
    private readonly IConfigurationManager configurationManager;

    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ObservableCollection<string> ExcludeFileTypes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddFileTypeCommand))]
    private string? fileTypeInputText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveFileTypeCommand))]
    private string? selectedFileType;

    public CompressionExclusionsViewModel(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;

        foreach (var entry in CompressionExclusionFormatter.Parse(configurationManager.ExcludeCompression))
        {
            ExcludeFileTypes.Add(entry);
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddFileType))]
    private void AddFileType(string? extension)
    {
        if (!CanAddFileType(extension))
        {
            return;
        }

        var normalized = CompressionExclusionFormatter.NormalizeExtension(extension);
        ExcludeFileTypes.Add(normalized);
        FileTypeInputText = null;
    }

    private bool CanAddFileType(string? extension)
    {
        var normalized = CompressionExclusionFormatter.NormalizeExtension(extension);
        return Regex.IsMatch(normalized, "^\\.[A-Za-z0-9]+$", RegexOptions.None, TimeSpan.FromSeconds(10))
            && !ExcludeFileTypes.Contains(normalized, StringComparer.OrdinalIgnoreCase);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveFileType))]
    private void RemoveFileType()
    {
        if (string.IsNullOrEmpty(SelectedFileType))
        {
            return;
        }

        ExcludeFileTypes.Remove(SelectedFileType);
        SelectedFileType = null;
    }

    private bool CanRemoveFileType() => !string.IsNullOrEmpty(SelectedFileType);

    [RelayCommand]
    private void Save()
    {
        configurationManager.ExcludeCompression = CompressionExclusionFormatter.Format(ExcludeFileTypes);
        TaskCompletionSource.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(false);
    }
}

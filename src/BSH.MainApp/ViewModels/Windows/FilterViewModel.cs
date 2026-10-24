// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Brightbits.BSH.Engine.Contracts;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;

using MaxFileSizeSetting = Brightbits.BSH.Engine.Services.FileCollector.MaxFileSize;

namespace BSH.MainApp.ViewModels.Windows;

public partial class FilterViewModel : ObservableObject
{
    private readonly IConfigurationManager configurationManager;

    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public event Action<string>? ValidationFailed;

    public ObservableCollection<string> ExcludeFolders { get; } = [];

    public ObservableCollection<string> ExcludeFiles { get; } = [];

    public ObservableCollection<string> ExcludeFileTypes { get; } = [];

    public ObservableCollection<string> RegexPatterns { get; } = [];

    public ObservableCollection<ExclusionRuleViewModel> Rules { get; } = [];

    public bool HasNoRules => Rules.Count == 0;

    private bool isAddingFileType;
    public bool IsAddingFileType
    {
        get => isAddingFileType;
        private set => SetProperty(ref isAddingFileType, value);
    }

    private bool isAddingPattern;
    public bool IsAddingPattern
    {
        get => isAddingPattern;
        private set => SetProperty(ref isAddingPattern, value);
    }

    private bool excludeFilesBigger;
    public bool ExcludeFilesBigger
    {
        get => excludeFilesBigger;
        set => SetProperty(ref excludeFilesBigger, value);
    }

    private bool includeSystemFolders;
    public bool IncludeSystemFolders
    {
        get => includeSystemFolders;
        set => SetProperty(ref includeSystemFolders, value);
    }

    public IReadOnlyList<FileSizeUnit> MaxFileSizeUnits { get; } = [FileSizeUnit.MB, FileSizeUnit.GB];

    private double maxFileSize;
    public double MaxFileSize
    {
        get => maxFileSize;
        set => SetProperty(ref maxFileSize, double.IsNaN(value) ? 0 : value);
    }

    private FileSizeUnit maxFileSizeUnit = FileSizeUnit.MB;
    public FileSizeUnit MaxFileSizeUnit
    {
        get => maxFileSizeUnit;
        set => SetProperty(ref maxFileSizeUnit, value);
    }

    private string? fileTypeInputText;
    public string? FileTypeInputText
    {
        get => fileTypeInputText;
        set
        {
            if (SetProperty(ref fileTypeInputText, value))
            {
                AddFileTypeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string? regexInputText;
    public string? RegexInputText
    {
        get => regexInputText;
        set
        {
            if (SetProperty(ref regexInputText, value))
            {
                AddRegexCommand.NotifyCanExecuteChanged();
                UpdateRegexTestResult();
            }
        }
    }

    private string? regexTestInputText;
    public string? RegexTestInputText
    {
        get => regexTestInputText;
        set
        {
            if (SetProperty(ref regexTestInputText, value))
            {
                UpdateRegexTestResult();
            }
        }
    }

    private bool isRegexTestMatch;
    public bool IsRegexTestMatch
    {
        get => isRegexTestMatch;
        private set => SetProperty(ref isRegexTestMatch, value);
    }

    private bool isRegexTestNoMatch;
    public bool IsRegexTestNoMatch
    {
        get => isRegexTestNoMatch;
        private set => SetProperty(ref isRegexTestNoMatch, value);
    }

    public IRelayCommand AddFileTypeCommand { get; }

    public IRelayCommand AddRegexCommand { get; }

    public IRelayCommand ShowAddFileTypeCommand { get; }

    public IRelayCommand ShowAddPatternCommand { get; }

    public IRelayCommand CancelAddCommand { get; }

    public IRelayCommand SaveCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IAsyncRelayCommand BrowseFolderCommand { get; }

    public IAsyncRelayCommand BrowseFileCommand { get; }

    public WindowId ParentWindowId
    {
        get; set;
    }

    public FilterViewModel(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;

        AddFileTypeCommand = new RelayCommand<string?>(AddFileType, CanAddFileType);
        AddRegexCommand = new RelayCommand<string?>(AddRegex, CanAddRegex);
        ShowAddFileTypeCommand = new RelayCommand(() => ShowAddPanel(fileType: true));
        ShowAddPatternCommand = new RelayCommand(() => ShowAddPanel(fileType: false));
        CancelAddCommand = new RelayCommand(CloseAddPanels);
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(OnWindowClosed);
        BrowseFolderCommand = new AsyncRelayCommand(BrowseFolderAsync);
        BrowseFileCommand = new AsyncRelayCommand(BrowseFileAsync);

        ExcludeFolders.CollectionChanged += (_, _) => RebuildRules();
        ExcludeFiles.CollectionChanged += (_, _) => RebuildRules();
        ExcludeFileTypes.CollectionChanged += (_, _) => RebuildRules();
        RegexPatterns.CollectionChanged += (_, _) => RebuildRules();

        LoadFromConfiguration();
    }

    private void RebuildRules()
    {
        Rules.Clear();
        AddRules(ExclusionRuleKind.Folder, ExcludeFolders);
        AddRules(ExclusionRuleKind.File, ExcludeFiles);
        AddRules(ExclusionRuleKind.FileType, ExcludeFileTypes);
        AddRules(ExclusionRuleKind.Pattern, RegexPatterns);
        OnPropertyChanged(nameof(HasNoRules));
    }

    private void AddRules(ExclusionRuleKind kind, ObservableCollection<string> source)
    {
        foreach (var value in source)
        {
            Rules.Add(new ExclusionRuleViewModel(kind, value, new RelayCommand(() => RemoveRule(source, value))));
        }
    }

    private void RemoveRule(ObservableCollection<string> source, string value)
    {
        source.Remove(value);
        if (ReferenceEquals(source, ExcludeFileTypes))
        {
            AddFileTypeCommand.NotifyCanExecuteChanged();
        }
    }

    private void ShowAddPanel(bool fileType)
    {
        IsAddingFileType = fileType;
        IsAddingPattern = !fileType;
    }

    private void CloseAddPanels()
    {
        IsAddingFileType = false;
        IsAddingPattern = false;
        FileTypeInputText = null;
        RegexInputText = null;
        RegexTestInputText = null;
    }

    public void LoadFromConfiguration()
    {
        ExcludeFolders.Clear();
        foreach (var entry in SplitExclusions(configurationManager.ExcludeFolder)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ExcludeFolders.Add(entry);
        }

        ExcludeFiles.Clear();
        foreach (var entry in SplitExclusions(configurationManager.ExcludeFile)
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
        foreach (var entry in SplitExclusions(configurationManager.ExcludeMask)
            .Distinct(StringComparer.Ordinal))
        {
            RegexPatterns.Add(entry);
        }

        ExcludeFilesBigger = MaxFileSizeSetting.TryGetMegabytes(configurationManager.ExcludeFileBigger, out var megabytes);
        if (ExcludeFilesBigger && megabytes >= 1024 && megabytes % 1024 == 0)
        {
            MaxFileSizeUnit = FileSizeUnit.GB;
            MaxFileSize = (double)(megabytes / 1024);
        }
        else
        {
            MaxFileSizeUnit = FileSizeUnit.MB;
            MaxFileSize = ExcludeFilesBigger ? (double)megabytes : 0;
        }
        IncludeSystemFolders = configurationManager.IncludeSystemFolders == "1";
    }

    public void SaveToConfiguration()
    {
        configurationManager.ExcludeFolder = string.Join("|", ExcludeFolders.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        configurationManager.ExcludeFile = string.Join("|", ExcludeFiles.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        configurationManager.ExcludeFileTypes = string.Join("|", ExcludeFileTypes.Select(x => x.Trim().TrimStart('*').TrimStart('.')).Where(x => !string.IsNullOrEmpty(x)));
        configurationManager.ExcludeMask = string.Join("|", RegexPatterns.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        var configuredMegabytes = (decimal)MaxFileSize * (MaxFileSizeUnit == FileSizeUnit.GB ? 1024 : 1);
        configurationManager.ExcludeFileBigger = ExcludeFilesBigger
            ? MaxFileSizeSetting.FromMegabytes(configuredMegabytes)
            : string.Empty;
        configurationManager.IncludeSystemFolders = IncludeSystemFolders ? "1" : "0";
    }

    private static IEnumerable<string> SplitExclusions(string? value)
    {
        return (value ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(entry => entry.Trim())
            .Where(entry => !string.IsNullOrEmpty(entry));
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
        var trimmed = folder?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (!IsPathUnderSources(trimmed))
        {
            ReportValidationError("Filter_FolderNotInSources");
            return;
        }

        if (ExcludeFolders.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        ExcludeFolders.Add(trimmed);
    }

    private void AddFile(string? file)
    {
        var trimmed = file?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        if (!IsPathUnderSources(trimmed))
        {
            ReportValidationError("Filter_FileNotInSources");
            return;
        }

        if (ExcludeFiles.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        ExcludeFiles.Add(trimmed);
    }

    private void AddFileType(string? fileType)
    {
        var trimmed = NormalizeFileType(fileType);
        if (!IsValidFileType(trimmed))
        {
            return;
        }

        if (ExcludeFileTypes.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        ExcludeFileTypes.Add(trimmed);

        CloseAddPanels();
    }

    private bool CanAddFileType(string? fileType)
    {
        var trimmed = NormalizeFileType(fileType);
        return IsValidFileType(trimmed)
            && !ExcludeFileTypes.Contains(trimmed!, StringComparer.OrdinalIgnoreCase);
    }

    private static string? NormalizeFileType(string? fileType)
    {
        return fileType?.Trim().TrimStart('*').TrimStart('.');
    }

    private static bool IsValidFileType(string? fileType)
    {
        return !string.IsNullOrEmpty(fileType)
            && Regex.IsMatch(fileType, "^[A-Za-z0-9]+$", RegexOptions.None, TimeSpan.FromSeconds(10));
    }

    private void AddRegex(string? regex)
    {
        if (!TryCreateRegexes(regex, out var regexes))
        {
            return;
        }

        foreach (var item in regexes)
        {
            var line = item.ToString();
            if (RegexPatterns.Contains(line))
            {
                continue;
            }

            RegexPatterns.Add(line);
        }

        CloseAddPanels();
    }

    private static bool CanAddRegex(string? regex)
    {
        return TryCreateRegexes(regex, out _);
    }

    private static bool TryCreateRegexes(string? input, out IReadOnlyList<Regex> regexes)
    {
        regexes = [];
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var parsedRegexes = new List<Regex>();
        var lines = input.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);

        try
        {
            foreach (var line in lines.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)))
            {
                parsedRegexes.Add(new Regex(line, RegexOptions.None, TimeSpan.FromSeconds(10)));
            }
        }
        catch (ArgumentException)
        {
            return false;
        }

        regexes = parsedRegexes;
        return parsedRegexes.Count > 0;
    }

    private void UpdateRegexTestResult()
    {
        if (RegexTestInputText == null || !TryCreateRegexes(RegexInputText, out var regexes))
        {
            IsRegexTestMatch = false;
            IsRegexTestNoMatch = false;
            return;
        }

        bool isMatch;
        try
        {
            isMatch = regexes.Any(regex => regex.IsMatch(RegexTestInputText));
        }
        catch (RegexMatchTimeoutException)
        {
            isMatch = false;
        }

        IsRegexTestMatch = isMatch;
        IsRegexTestNoMatch = !isMatch;
    }

    private void ReportValidationError(string resourceName)
    {
        ValidationFailed?.Invoke(resourceName.GetLocalized() ?? resourceName);
    }

    private void Save()
    {
        SaveToConfiguration();
        TaskCompletionSource.TrySetResult(true);
    }

    public void OnWindowClosed()
    {
        TaskCompletionSource.TrySetResult(false);
    }

    private async Task BrowseFolderAsync()
    {
        if (ParentWindowId.Value == 0)
        {
            return;
        }

        var picker = new FolderPicker(ParentWindowId)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            AddFolder(folder.Path);
        }
    }

    private async Task BrowseFileAsync()
    {
        if (ParentWindowId.Value == 0)
        {
            return;
        }

        var picker = new FileOpenPicker(ParentWindowId)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

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

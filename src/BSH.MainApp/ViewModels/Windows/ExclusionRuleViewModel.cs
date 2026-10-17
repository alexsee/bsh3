// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows.Input;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.ViewModels.Windows;

public enum ExclusionRuleKind
{
    Folder,
    File,
    FileType,
    Pattern,
}

public sealed class ExclusionRuleViewModel
{
    public ExclusionRuleViewModel(ExclusionRuleKind kind, string value, ICommand removeCommand)
    {
        Kind = kind;
        Value = value;
        RemoveCommand = removeCommand;
    }

    public ExclusionRuleKind Kind { get; }

    public string Value { get; }

    public string DisplayValue => Kind == ExclusionRuleKind.FileType ? "." + Value : Value;

    public string KindText => $"Filter_Kind_{Kind}".GetLocalized() ?? Kind.ToString();

    public string Glyph => Kind switch
    {
        ExclusionRuleKind.Folder => "\uE8B7",
        ExclusionRuleKind.File => "\uE8A5",
        ExclusionRuleKind.FileType => "\uE8EC",
        _ => "\uE943",
    };

    public string AccessibleText => $"{KindText}: {DisplayValue}";

    public ICommand RemoveCommand { get; }
}

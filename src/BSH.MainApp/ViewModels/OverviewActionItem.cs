// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows.Input;
using BSH.MainApp.Models;

namespace BSH.MainApp.ViewModels;

public sealed class OverviewActionItem
{
    public OverviewActionItem(OverviewIssueKind kind, OverviewSeverity severity, string text, string buttonText, ICommand command)
    {
        Kind = kind;
        Severity = severity;
        Text = text;
        ButtonText = buttonText;
        Command = command;
    }

    public OverviewIssueKind Kind { get; }

    public OverviewSeverity Severity { get; }

    public string Text { get; }

    public string ButtonText { get; }

    public ICommand Command { get; }
}

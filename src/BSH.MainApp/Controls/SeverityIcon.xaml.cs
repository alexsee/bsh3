// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Controls;

public sealed partial class SeverityIcon : UserControl
{
    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity),
            typeof(OverviewSeverity),
            typeof(SeverityIcon),
            new PropertyMetadata(OverviewSeverity.Protected, (d, _) => ((SeverityIcon)d).UpdateState()));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(SeverityIcon), new PropertyMetadata(16d));

    public SeverityIcon()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateState();
    }

    public OverviewSeverity Severity
    {
        get => (OverviewSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    private void UpdateState()
    {
        VisualStateManager.GoToState(this, Severity.ToString(), false);
    }
}

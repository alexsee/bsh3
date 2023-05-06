// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views.SettingsPages;

public sealed partial class SourcesSettingsPage : Page
{
    public SourcesSettingsPage()
    {
        this.InitializeComponent();

        this.lvSources.ItemsSource = new List<string>
        {
            "D:\\Meine Dokumente",
            "D:\\Desktop"
        };
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using BSH.MainApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

using Brightbits.BSH.Engine.Types;
namespace BSH.MainApp.Views.SettingsPages;

public sealed partial class TargetSettingsPage : Page
{
    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public TargetSettingsPage()
    {
        this.InitializeComponent();
    }

    private async void MediaTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel ||
            MediaTypeComboBox.SelectedItem is not MediaType mediaType ||
            mediaType == viewModel.SelectedMediaType)
        {
            return;
        }

        await viewModel.ChangeSelectedMediaTypeAsync(mediaType);
        MediaTypeComboBox.SelectedItem = viewModel.SelectedMediaType;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using BSH.MainApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

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

    private async void FtpEnforceUnencryptedCheckBox_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        var requested = FtpEnforceUnencryptedCheckBox.IsChecked == true;
        if (requested == viewModel.FtpRemoteEnforceUnencrypted)
        {
            return;
        }

        await viewModel.ChangeFtpRemoteEnforceUnencryptedAsync(requested);
        FtpEnforceUnencryptedCheckBox.IsChecked = viewModel.FtpRemoteEnforceUnencrypted;
    }
}

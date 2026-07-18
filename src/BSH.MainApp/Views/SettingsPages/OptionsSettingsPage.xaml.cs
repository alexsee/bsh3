// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views.SettingsPages;

public sealed partial class OptionsSettingsPage : Page
{
    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
    private bool isSyncingModeType;

    public OptionsSettingsPage()
    {
        this.InitializeComponent();
    }

    private async void ModeTypeRadioButton_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (isSyncingModeType || DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        var requestedModeType = sender switch
        {
            var radioButton when ReferenceEquals(radioButton, CompressionRadioButton) => ModeType.Compression,
            var radioButton when ReferenceEquals(radioButton, EncryptionRadioButton) => ModeType.Encryption,
            _ => ModeType.RegularCopy
        };

        await viewModel.ChangeModeTypeAsync(requestedModeType);
        SyncModeTypeSelection(viewModel.ModeType);
    }

    private void SyncModeTypeSelection(ModeType modeType)
    {
        isSyncingModeType = true;
        try
        {
            RegularCopyRadioButton.IsChecked = modeType == ModeType.RegularCopy;
            CompressionRadioButton.IsChecked = modeType == ModeType.Compression;
            EncryptionRadioButton.IsChecked = modeType == ModeType.Encryption;
        }
        finally
        {
            isSyncingModeType = false;
        }
    }
}

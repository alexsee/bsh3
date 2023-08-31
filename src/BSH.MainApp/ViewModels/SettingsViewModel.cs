// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using WinUIEx;

namespace BSH.MainApp.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationManager configurationManager;

    #region Sources Settings

    [ObservableProperty]
    private string? selectedSource = null;

    public ObservableCollection<string> Sources { get; } = new();

    private void InitSourcesSettings()
    {
        Array.ForEach(this.configurationManager.SourceFolder.Split("|"), this.Sources.Add);
    }

    [RelayCommand]
    private async Task AddSourceFolder()
    {
        var folderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        var hwnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            if (this.Sources.Contains(folder.Path))
            {
                return;
            }

            this.Sources.Add(folder.Path);
            this.configurationManager.SourceFolder = string.Join("|", this.Sources);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSourceFolder))]
    private void DeleteSourceFolder()
    {
        if (string.IsNullOrEmpty(this.SelectedSource))
        {
            return;
        }

        this.Sources.Remove(this.SelectedSource);
        this.configurationManager.SourceFolder = string.Join("|", this.Sources);
    }

    private bool CanDeleteSourceFolder() => !string.IsNullOrEmpty(SelectedSource);

    #endregion

    #region Target Settings

    public IDictionary<MediaType, string> MediaTypes
    {
        get => Enum.GetValues(typeof(MediaType)).Cast<MediaType>().ToDictionary(x => x, this.GetMediaTypeDisplayName);
    }

    private MediaType selectedMediaType;
    public MediaType SelectedMediaType
    {
        get => selectedMediaType;
        set
        {
            if (value == MediaType.LocalDevice)
            {
                this.FtpRemoteVisibility = Visibility.Collapsed;
                this.LocalDeviceVisibility = Visibility.Visible;
            }
            else
            {
                this.FtpRemoteVisibility = Visibility.Visible;
                this.LocalDeviceVisibility = Visibility.Collapsed;
            }

            SetProperty(ref selectedMediaType, value);
        }
    }

    [ObservableProperty]
    private Visibility localDeviceVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility ftpRemoteVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private string localDevicePath;

    [ObservableProperty]
    private string ftpRemoteHost;

    [ObservableProperty]
    private int ftpRemotePort;

    [ObservableProperty]
    private string ftpRemoteUser;

    [ObservableProperty]
    private string ftpRemotePassword;

    [ObservableProperty]
    private string ftpRemotePath;

    [ObservableProperty]
    private string ftpRemoteEncoding;

    [ObservableProperty]
    private bool ftpRemoteEnforceUnencrypted;

    private void InitTargetSettings()
    {
        this.SelectedMediaType = this.configurationManager.MediumType;

        // local device
        this.LocalDevicePath = this.configurationManager.BackupFolder;

        // ftp remote
        this.FtpRemoteHost = this.configurationManager.FtpHost;
        this.FtpRemotePort = int.Parse(this.configurationManager.FtpPort);
        this.FtpRemoteUser = this.configurationManager.FtpUser;
        this.FtpRemotePassword = this.configurationManager.FtpPass;
        this.FtpRemotePath = this.configurationManager.FtpFolder;
        this.FtpRemoteEncoding = this.configurationManager.FtpCoding;

        if (this.configurationManager.FtpEncryptionMode == "3")
        {
            this.FtpRemoteEnforceUnencrypted = true;
        }
        else
        {
            this.FtpRemoteEnforceUnencrypted = false;
        }
    }

    public string GetMediaTypeDisplayName(MediaType mediaType)
    {
        if (mediaType == MediaType.LocalDevice)
        {
            return "MediaType_LocalDevice".GetLocalized();
        }
        else
        {
            return "MediaType_FileTransferServer".GetLocalized();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCheckFtpRemote))]
    public async Task CheckFtpRemote()
    {
        // check FTP
        var profile = FTPStorage.CheckConnection(FtpRemoteHost, FtpRemotePort, FtpRemoteUser, FtpRemotePassword, FtpRemotePath, FtpRemoteEncoding);

        if (profile)
        {
            var messageBoxDlg = new MessageDialog("MsgBox_Ftp_Successful_Text".GetLocalized(), "MsgBox_Ftp_Successful_Title".GetLocalized());
            messageBoxDlg.Commands.Add(new UICommand("OK"));

            var hwnd = App.MainWindow.GetWindowHandle();
            WinRT.Interop.InitializeWithWindow.Initialize(messageBoxDlg, hwnd);

            await messageBoxDlg.ShowAsync();
        }
        else
        {
            var messageBoxDlg = new MessageDialog("MsgBox_Ftp_Unuccessful_Text".GetLocalized(), "MsgBox_Ftp_Unuccessful_Title".GetLocalized());
            messageBoxDlg.Commands.Add(new UICommand("OK"));

            var hwnd = App.MainWindow.GetWindowHandle();
            WinRT.Interop.InitializeWithWindow.Initialize(messageBoxDlg, hwnd);

            await messageBoxDlg.ShowAsync();
        }
    }

    private bool CanExecuteCheckFtpRemote()
    {
        return !string.IsNullOrEmpty(this.FtpRemoteHost) && !string.IsNullOrEmpty(this.FtpRemoteUser) && !string.IsNullOrEmpty(this.FtpRemotePassword);
    }

    #endregion

    public SettingsViewModel(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        this.InitSourcesSettings();
        this.InitTargetSettings();
    }
}

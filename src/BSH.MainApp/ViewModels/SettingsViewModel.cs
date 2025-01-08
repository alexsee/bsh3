// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using WinUIEx;

namespace BSH.MainApp.ViewModels;

public enum ModeType
{
    RegularCopy = 0,
    Compression = 1,
    Encryption = 2,
}

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationManager configurationManager;
    private readonly IPresentationService presentationController;
    private readonly IJobService jobService;
    private readonly IQueryManager queryManager;

    #region Sources Settings

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteSourceFolderCommand))]
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

    public IList<MediaType> MediaTypes
    {
        get => new List<MediaType>() { MediaType.LocalDevice, MediaType.FileTransferServer };
    }

    [ObservableProperty]
    private MediaType selectedMediaType;

    [ObservableProperty]
    private Visibility localDeviceVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility ftpRemoteVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private string localDevicePath;

    [ObservableProperty]
    private string localUNCUser;

    [ObservableProperty]
    private string localUNCPassword;

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
        // selected media type
        this.SelectedMediaType = this.configurationManager.MediumType;

        if (SelectedMediaType == MediaType.LocalDevice)
        {
            this.FtpRemoteVisibility = Visibility.Collapsed;
            this.LocalDeviceVisibility = Visibility.Visible;
        }
        else
        {
            this.FtpRemoteVisibility = Visibility.Visible;
            this.LocalDeviceVisibility = Visibility.Collapsed;
        }

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

    public static string GetMediaTypeDisplayName(MediaType mediaType)
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
        var profile = FtpStorage.CheckConnection(FtpRemoteHost, FtpRemotePort, FtpRemoteUser, FtpRemotePassword, FtpRemotePath, FtpRemoteEncoding);

        if (profile)
        {
            await presentationController.ShowMessageBoxAsync("MsgBox_Ftp_Successful_Title".GetLocalized(), "MsgBox_Ftp_Successful_Text".GetLocalized(), new List<IUICommand> { new UICommand("OK") });
        }
        else
        {
            await presentationController.ShowMessageBoxAsync("MsgBox_Ftp_Unuccessful_Title".GetLocalized(), "MsgBox_Ftp_Unuccessful_Text".GetLocalized(), new List<IUICommand> { new UICommand("OK") });
        }
    }

    private bool CanExecuteCheckFtpRemote()
    {
        return !string.IsNullOrEmpty(this.FtpRemoteHost) && !string.IsNullOrEmpty(this.FtpRemoteUser) && !string.IsNullOrEmpty(this.FtpRemotePassword);
    }

    [RelayCommand]
    public async Task ChangeLocalPath()
    {
        var folderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder == null)
        {
            return;
        }

        await this.presentationController.ShowMessageBoxAsync(
            "MsgBox_LocalPath_Change_Text".GetLocalized(),
            "MsgBox_LocalPath_Change_Title".GetLocalized(),
            [
                new UICommand("MsgBox_LocalPath_Change_Use".GetLocalized(), (x) =>
                {
                    this.LocalDevicePath = folder.Path;
                    this.configurationManager.BackupFolder = folder.Path;

                    // update media serial (if local path)
                    if (folder.Path.StartsWith(@"\\"))
                    {
                        return;
                    }

                    this.LocalUNCUser = "";
                    this.LocalUNCPassword = "";

                    this.configurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(folder.Path.Substring(0, 3));
                    if (this.configurationManager.MediaVolumeSerial == null || this.configurationManager.MediaVolumeSerial == "0")
                    {
                        this.configurationManager.MediaVolumeSerial = "";
                    }
                }),
                new UICommand("MsgBox_LocalPath_Change_Move".GetLocalized(), (x) =>
                {
                    this.LocalDevicePath = folder.Path;

                    // TODO: add move logic
                }),
                new UICommand("MsgBox_Cancel".GetLocalized())
            ]
        );
    }

    async partial void OnSelectedMediaTypeChanging(MediaType oldValue, MediaType newValue)
    {
        if (oldValue == MediaType.Unset) return;
        if (oldValue == newValue) return;

        var result = await this.presentationController.ShowMessageBoxAsync(
            "MsgBox_MediaType_Change_Title".GetLocalized(),
            "MsgBox_MediaType_Change_Text".GetLocalized(),
            [
                new UICommand("MsgBox_Yes".GetLocalized()),
                new UICommand("MsgBox_No".GetLocalized())
            ]
        );

        // run the task
        if (result == ContentDialogResult.Primary)
        {
            // add remove all backups logic
            var versions = this.queryManager.GetVersions().Select(x => x.Id).ToList();
            await this.jobService.DeleteBackupsAsync(versions);

            // update UI
            if (newValue == MediaType.LocalDevice)
            {
                this.FtpRemoteVisibility = Visibility.Collapsed;
                this.LocalDeviceVisibility = Visibility.Visible;
            }
            else
            {
                this.FtpRemoteVisibility = Visibility.Visible;
                this.LocalDeviceVisibility = Visibility.Collapsed;
            }
        }
        else
        {
            SelectedMediaType = oldValue;
        }

    }

    partial void OnLocalUNCUserChanged(string value)
    {
        this.configurationManager.UNCUsername = value;
    }

    partial void OnLocalUNCPasswordChanged(string value)
    {
        this.configurationManager.UNCPassword = value;
    }

    #endregion

    #region Options Settings

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DisableEncryptionCommand))]
    private ModeType modeType;

    [ObservableProperty]
    private bool waitForDevice;

    private void InitOptionsSettings()
    {
        if (this.configurationManager.Compression == 1)
        {
            this.ModeType = ModeType.Compression;
        }
        else if (this.configurationManager.Encrypt == 1)
        {
            this.ModeType = ModeType.Encryption;
        }
        else
        {
            this.ModeType = ModeType.RegularCopy;
        }

        this.WaitForDevice = this.configurationManager.ShowWaitOnMediaAutoBackups == "1";
    }

    partial void OnWaitForDeviceChanged(bool value)
    {
        this.configurationManager.ShowWaitOnMediaAutoBackups = value ? "1" : "0";
    }

    async partial void OnModeTypeChanged(ModeType oldValue, ModeType newValue)
    {
        if (newValue == ModeType.Compression)
        {
            this.configurationManager.Compression = 1;
            this.configurationManager.Encrypt = 0;
        }
        else if (newValue == ModeType.Encryption)
        {
            // setup encryption
            var (password, _) = await presentationController.RequestPassword();
            if (password == null)
            {
                ModeType = oldValue;
                return;
            }

            this.configurationManager.EncryptPassMD5 = Hash.GetMD5Hash(password);
            this.configurationManager.Encrypt = 1;
        }
        else
        {
            this.configurationManager.Compression = 0;
            this.configurationManager.Encrypt = 0;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisableEncryption))]
    private async Task DisableEncryption()
    {
        if (!await this.jobService.CheckMediaAsync(ActionType.Delete))
        {
            return;
        }

        if (!await this.jobService.RequestPassword())
        {
            return;
        }

        // disable encryption (need to decrypt everything)
        var task = this.jobService.ModifyBackupAsync();
        await task.ConfigureAwait(true);

        InitOptionsSettings();
    }

    private bool CanDisableEncryption() => ModeType == ModeType.Encryption;

    #endregion

    #region Mode Settings

    [ObservableProperty]
    private TaskType taskType;

    [ObservableProperty]
    private bool stopBackupWhenBatteryMode;

    private void InitModeSettings()
    {
        this.TaskType = this.configurationManager.TaskType;
        this.StopBackupWhenBatteryMode = this.configurationManager.DeativateAutoBackupsWhenAkku == "1";
    }

    partial void OnStopBackupWhenBatteryModeChanged(bool value)
    {
        this.configurationManager.DeativateAutoBackupsWhenAkku = value ? "1" : "0";
    }

    #endregion

    #region Enhanced Settings

    [ObservableProperty]
    private bool enableNotificationWhenDiskspaceLow;

    [ObservableProperty]
    private int notificationWhenDiskspaceLow;

    [ObservableProperty]
    private bool enableDirectoryLocalization;

    [ObservableProperty]
    private bool enableNotificationWhenBackupFinished;

    [ObservableProperty]
    private bool enableNotificationWhenBackupDeviceNotReady;

    [ObservableProperty]
    private bool enableNotificationWhenBackupOutdated;

    [ObservableProperty]
    private int notificationWhenBackupOutdated;

    void InitEnhancedSettings()
    {
        this.EnableNotificationWhenDiskspaceLow = !string.IsNullOrEmpty(this.configurationManager.RemindSpace);
        this.NotificationWhenDiskspaceLow = int.Parse(this.configurationManager.RemindSpace);

        this.EnableDirectoryLocalization = this.configurationManager.ShowLocalizedPath == "1";
        this.EnableNotificationWhenBackupFinished = this.configurationManager.InfoBackupDone == "1";
        this.EnableNotificationWhenBackupDeviceNotReady = this.configurationManager.Medium == "1";

        this.EnableNotificationWhenBackupOutdated = !string.IsNullOrEmpty(this.configurationManager.RemindAfterDays);
        this.NotificationWhenBackupOutdated = int.Parse(this.configurationManager.RemindAfterDays);
    }

    partial void OnEnableNotificationWhenDiskspaceLowChanged(bool value)
    {
        if (value)
        {
            this.configurationManager.RemindSpace = this.NotificationWhenDiskspaceLow.ToString();
        }
        else
        {
            this.configurationManager.RemindSpace = string.Empty;
        }
    }

    partial void OnNotificationWhenDiskspaceLowChanged(int value)
    {
        if (value == 0) return;
        this.configurationManager.RemindSpace = value.ToString();
    }

    partial void OnEnableDirectoryLocalizationChanged(bool value)
    {
        this.configurationManager.ShowLocalizedPath = value ? "1" : "0";
    }

    partial void OnEnableNotificationWhenBackupFinishedChanged(bool value)
    {
        this.configurationManager.InfoBackupDone = value ? "1" : "0";
    }

    partial void OnEnableNotificationWhenBackupDeviceNotReadyChanged(bool value)
    {
        this.configurationManager.Medium = value ? "1" : "0";
    }

    partial void OnEnableNotificationWhenBackupOutdatedChanged(bool value)
    {
        if (value)
        {
            this.configurationManager.RemindAfterDays = this.NotificationWhenBackupOutdated.ToString();
        }
        else
        {
            this.configurationManager.RemindAfterDays = string.Empty;
        }
    }

    partial void OnNotificationWhenBackupOutdatedChanged(int value)
    {
        if (value == 0) return;
        this.configurationManager.RemindAfterDays = value.ToString();
    }

    #endregion

    public SettingsViewModel(IConfigurationManager configurationManager, IPresentationService presentationService, IJobService jobService, IQueryManager queryManager)
    {
        this.configurationManager = configurationManager;
        this.presentationController = presentationService;
        this.jobService = jobService;
        this.queryManager = queryManager;
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        this.InitSourcesSettings();
        this.InitTargetSettings();
        this.InitModeSettings();
        this.InitOptionsSettings();
        this.InitEnhancedSettings();
    }
}

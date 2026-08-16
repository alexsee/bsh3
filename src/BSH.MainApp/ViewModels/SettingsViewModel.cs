// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace BSH.MainApp.ViewModels;

public enum ModeType
{
    Unset = -1,
    RegularCopy = 0,
    Compression = 1,
    Encryption = 2,
}

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly IConfigurationManager configurationManager;
    private readonly IPresentationService presentationController;
    private readonly IJobService jobService;
    private readonly IQueryManager queryManager;
    private readonly IBackupTargetService backupTargetService;
    private readonly ISwitchStorageService switchStorageService;
    private readonly IOrchestrationService orchestrationService;
    private readonly IStartupLaunchAdapter startupLaunchAdapter;
    private readonly IUpdateService updateService;
    private bool suppressEnhancedSettingsPersistence;
    private bool suppressTargetSettingsPersistence;
    private const string RemindSpaceOffSentinel = "-1";

    #region Sources Settings

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteSourceFolderCommand))]
    private string? selectedSource = null;

    public ObservableCollection<string> Sources { get; } = new();

    private string? sourceValidationErrorMessage;
    public string? SourceValidationErrorMessage
    {
        get => sourceValidationErrorMessage;
        set => SetProperty(ref sourceValidationErrorMessage, value);
    }

    private void InitSourcesSettings()
    {
        this.Sources.Clear();
        Array.ForEach(
            this.configurationManager.SourceFolder.Split("|", StringSplitOptions.RemoveEmptyEntries),
            this.Sources.Add);
    }

    [RelayCommand]
    private async Task AddSourceFolder()
    {
        var folderPicker = new FolderPicker(App.MainWindow.AppWindow.Id)
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            TryAddSourceFolderPath(folder.Path);
        }
    }

    public bool TryAddSourceFolderPath(string folderPath)
    {
        SourceValidationErrorMessage = null;

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return false;
        }

        if (!PathRules.TryNormalizeFolderPath(folderPath, out var fullPath))
        {
            SourceValidationErrorMessage = "Settings_Sources_InvalidPath".GetLocalized();
            return false;
        }

        if (PathRules.IsDriveRoot(fullPath))
        {
            SourceValidationErrorMessage = "Settings_Sources_DriveRootRisky".GetLocalized();
            return false;
        }

        var folderName = Path.GetFileName(fullPath);
        if (this.Sources.Any(source =>
            string.Equals(Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)), folderName, StringComparison.OrdinalIgnoreCase)))
        {
            SourceValidationErrorMessage = "Settings_Sources_SameName".GetLocalized();
            return false;
        }

        this.Sources.Add(fullPath);
        this.configurationManager.SourceFolder = string.Join("|", this.Sources);
        return true;
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

    [RelayCommand]
    private async Task ShowExcludeFileFolderWindow()
    {
        await this.presentationController.ShowExcludeFileFolderWindowAsync();
    }

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
    private string localDevicePath = string.Empty;

    [ObservableProperty]
    private string localUNCUser = string.Empty;

    [ObservableProperty]
    private string localUNCPassword = string.Empty;

    [ObservableProperty]
    private string ftpRemoteHost = string.Empty;

    [ObservableProperty]
    private int ftpRemotePort;

    [ObservableProperty]
    private string ftpRemoteUser = string.Empty;

    [ObservableProperty]
    private string ftpRemotePassword = string.Empty;

    [ObservableProperty]
    private string ftpRemotePath = string.Empty;

    [ObservableProperty]
    private string ftpRemoteEncoding = string.Empty;

    [ObservableProperty]
    private bool ftpRemoteEnforceUnencrypted;

    [ObservableProperty]
    private bool isMovingBackupTarget;

    [ObservableProperty]
    private bool isBackupTargetChangeEnabled = true;

    [ObservableProperty]
    private Visibility backupTargetMoveProgressVisibility = Visibility.Collapsed;

    private void InitTargetSettings()
    {
        suppressTargetSettingsPersistence = true;
        try
        {
            // selected media type
            this.SelectedMediaType = this.configurationManager.MediumType;
            UpdateTargetVisibility();

            // local device
            this.LocalDevicePath = this.configurationManager.BackupFolder;
            this.LocalUNCUser = this.configurationManager.UNCUsername;
            this.LocalUNCPassword = DecryptConfigurationPassword(this.configurationManager.UNCPassword);

            // ftp remote
            this.FtpRemoteHost = this.configurationManager.FtpHost;
            this.FtpRemotePort = string.IsNullOrEmpty(this.configurationManager.FtpPort) ? 21 : int.Parse(this.configurationManager.FtpPort);
            this.FtpRemoteUser = this.configurationManager.FtpUser;
            this.FtpRemotePassword = this.configurationManager.FtpPass;
            this.FtpRemotePath = this.configurationManager.FtpFolder;
            this.FtpRemoteEncoding = this.configurationManager.FtpCoding;

            // FtpStorage treats mode "3" as encrypted (AutoConnect); anything else is plain FTP.
            this.FtpRemoteEnforceUnencrypted = this.configurationManager.FtpEncryptionMode != "3";
        }
        finally
        {
            suppressTargetSettingsPersistence = false;
        }
    }

    private void UpdateTargetVisibility()
    {
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
    }

    public (string Host, int Port, string User, string Password, string Path, string Encoding) GetDisplayedFtpConnectionValues()
    {
        return (FtpRemoteHost, FtpRemotePort, FtpRemoteUser, FtpRemotePassword, FtpRemotePath, FtpRemoteEncoding);
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
        var probe = GetDisplayedFtpConnectionValues();
        var profile = FtpStorage.CheckConnection(probe.Host, probe.Port, probe.User, probe.Password, probe.Path, probe.Encoding);

        if (profile)
        {
            await presentationController.ShowMessageBoxAsync("MsgBox_Ftp_Successful_Title".GetLocalized(), "MsgBox_Ftp_Successful_Text".GetLocalized(), new List<IUICommand> { new UICommand("MsgBox_OK".GetLocalized()) });
        }
        else
        {
            await presentationController.ShowMessageBoxAsync("MsgBox_Ftp_Unuccessful_Title".GetLocalized(), "MsgBox_Ftp_Unuccessful_Text".GetLocalized(), new List<IUICommand> { new UICommand("MsgBox_OK".GetLocalized()) });
        }
    }

    private bool CanExecuteCheckFtpRemote()
    {
        return !string.IsNullOrEmpty(this.FtpRemoteHost) && !string.IsNullOrEmpty(this.FtpRemoteUser) && !string.IsNullOrEmpty(this.FtpRemotePassword);
    }

    [RelayCommand]
    public async Task ChangeLocalPath()
    {
        var folderPicker = new FolderPicker(App.MainWindow.AppWindow.Id)
        {
            ViewMode = PickerViewMode.List,
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
                    UseLocalPath(folder.Path);
                }),
                new UICommand("MsgBox_LocalPath_Change_Move".GetLocalized(), async (x) =>
                {
                    await MoveExistingLocalBackupDataAsync(folder.Path);
                }),
                new UICommand("MsgBox_Cancel".GetLocalized())
            ]
        );
    }

    public async Task ChangeSelectedMediaTypeAsync(MediaType newValue)
    {
        var oldValue = SelectedMediaType;
        if (oldValue == MediaType.Unset || oldValue == newValue)
        {
            return;
        }

        var result = await this.presentationController.ShowMessageBoxAsync(
            "MsgBox_MediaType_Change_Title".GetLocalized(),
            "MsgBox_MediaType_Change_Text".GetLocalized(),
            [
                new UICommand("MsgBox_Yes".GetLocalized()),
                new UICommand("MsgBox_No".GetLocalized())
            ]
        );

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var versions = this.queryManager.GetVersions().Select(x => x.Id).ToList();
        await this.jobService.DeleteBackupsAsync(versions);

        SelectedMediaType = newValue;
        ApplyCurrentBackupTarget();
        UpdateTargetVisibility();
    }

    partial void OnLocalUNCUserChanged(string? oldValue, string newValue)
    {
        if (oldValue == null) return;
        if (oldValue == newValue) return;

        this.configurationManager.UNCUsername = newValue;
    }

    partial void OnLocalUNCPasswordChanged(string? oldValue, string newValue)
    {
        if (oldValue == null) return;
        if (oldValue == newValue) return;

        this.configurationManager.UNCPassword = string.IsNullOrEmpty(newValue)
            ? string.Empty
            : Crypto.EncryptString(newValue, System.Security.Cryptography.DataProtectionScope.LocalMachine);
    }

    partial void OnFtpRemoteHostChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        PersistFtpTargetIfSelected();
        CheckFtpRemoteCommand.NotifyCanExecuteChanged();
    }

    partial void OnFtpRemotePortChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        PersistFtpTargetIfSelected();
    }

    partial void OnFtpRemoteUserChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        PersistFtpTargetIfSelected();
        CheckFtpRemoteCommand.NotifyCanExecuteChanged();
    }

    partial void OnFtpRemotePasswordChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        PersistFtpTargetIfSelected();
        CheckFtpRemoteCommand.NotifyCanExecuteChanged();
    }

    partial void OnFtpRemotePathChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        PersistFtpTargetIfSelected();
    }

    partial void OnFtpRemoteEncodingChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        PersistFtpTargetIfSelected();
    }

    partial void OnFtpRemoteEnforceUnencryptedChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;

        // Disabling FTP encryption (enforce unencrypted) must delete backups first.
        if (newValue)
        {
            return;
        }

        PersistFtpTargetIfSelected();
    }

    public async Task ChangeFtpRemoteEnforceUnencryptedAsync(bool enforceUnencrypted)
    {
        var currentlyUnencrypted = this.configurationManager.FtpEncryptionMode != "3";
        if (enforceUnencrypted == currentlyUnencrypted && FtpRemoteEnforceUnencrypted == enforceUnencrypted)
        {
            return;
        }

        if (enforceUnencrypted)
        {
            if (!await TryDeleteAllBackupsWithMediaCheckAsync())
            {
                RevertFtpRemoteEnforceUnencrypted(false);
                return;
            }

            FtpRemoteEnforceUnencrypted = true;
            PersistFtpTargetIfSelected();
            return;
        }

        FtpRemoteEnforceUnencrypted = false;
        PersistFtpTargetIfSelected();
    }

    private void RevertFtpRemoteEnforceUnencrypted(bool enforceUnencrypted)
    {
        suppressTargetSettingsPersistence = true;
        try
        {
            FtpRemoteEnforceUnencrypted = enforceUnencrypted;
        }
        finally
        {
            suppressTargetSettingsPersistence = false;
        }
    }

    private async Task<bool> TryDeleteAllBackupsWithMediaCheckAsync()
    {
        if (!await this.jobService.CheckMediaAsync(ActionType.Delete))
        {
            return false;
        }

        var versions = this.queryManager.GetVersions().Select(x => x.Id).ToList();
        await this.jobService.DeleteBackupsAsync(versions);
        return true;
    }

    private void PersistFtpTargetIfSelected()
    {
        if (suppressTargetSettingsPersistence || SelectedMediaType != MediaType.FileTransferServer)
        {
            return;
        }

        ApplyFtpTargetFromPage();
    }

    private void ApplyCurrentBackupTarget()
    {
        if (suppressTargetSettingsPersistence || SelectedMediaType == MediaType.Unset)
        {
            return;
        }

        if (SelectedMediaType == MediaType.FileTransferServer)
        {
            ApplyFtpTargetFromPage();
            return;
        }

        ApplyLocalTargetFromPage();
    }

    private void ApplyFtpTargetFromPage()
    {
        MediaTargetApplier.ApplyFtpTarget(
            this.configurationManager,
            FtpRemoteHost,
            FtpRemotePort <= 0 ? "21" : FtpRemotePort.ToString(),
            FtpRemoteUser,
            FtpRemotePassword,
            NormalizeFtpFolder(FtpRemotePath),
            FtpRemoteEncoding,
            encryptionMode: FtpRemoteEnforceUnencrypted ? "0" : "3",
            sslProtocols: "0");
    }

    private void ApplyLocalTargetFromPage()
    {
        if (MediaTargetApplier.IsUncPath(LocalDevicePath))
        {
            MediaTargetApplier.ApplyUncTarget(
                this.configurationManager,
                LocalDevicePath,
                LocalUNCUser,
                LocalUNCPassword);
            return;
        }

        if (string.IsNullOrWhiteSpace(LocalDevicePath))
        {
            MediaTargetApplier.ApplyEmptyLocalTarget(this.configurationManager);
            return;
        }

        MediaTargetApplier.ApplyLocalTarget(
            this.configurationManager,
            LocalDevicePath,
            MediaTargetApplier.ResolveVolumeSerial(LocalDevicePath));
    }

    private static string NormalizeFtpFolder(string? path)
    {
        return string.IsNullOrEmpty(path) ? "" : FtpStorage.GetFtpPath(path);
    }

    public void UseLocalPath(string folderPath)
    {
        this.LocalDevicePath = folderPath;

        if (MediaTargetApplier.IsUncPath(folderPath))
        {
            MediaTargetApplier.ApplyUncTarget(
                this.configurationManager,
                folderPath,
                this.LocalUNCUser,
                this.LocalUNCPassword);
            return;
        }

        this.LocalUNCUser = "";
        this.LocalUNCPassword = "";
        MediaTargetApplier.ApplyLocalTarget(
            this.configurationManager,
            folderPath,
            MediaTargetApplier.ResolveVolumeSerial(folderPath));
    }

    public async Task MoveExistingLocalBackupDataAsync(string newFolderPath)
    {
        try
        {
            IsMovingBackupTarget = true;
            IsBackupTargetChangeEnabled = false;
            BackupTargetMoveProgressVisibility = Visibility.Visible;

            var oldFolderPath = this.configurationManager.BackupFolder;
            var result = await this.backupTargetService.MoveExistingBackupDataAsync(oldFolderPath, newFolderPath);

            if (result.Success)
            {
                UseLocalPath(newFolderPath);
                return;
            }

            await this.presentationController.ShowMessageBoxAsync(
                "Settings_Target_MoveFailed_Title".GetLocalized(),
                result.ErrorMessage ?? "Settings_Target_MoveFailed_Text".GetLocalized(),
                [new UICommand("MsgBox_OK".GetLocalized())]);
        }
        finally
        {
            IsMovingBackupTarget = false;
            IsBackupTargetChangeEnabled = true;
            BackupTargetMoveProgressVisibility = Visibility.Collapsed;
        }
    }

    [RelayCommand]
    public async Task SwitchStorageAsync()
    {
        if (!await this.jobService.CheckMediaAsync(ActionType.Modify, true))
        {
            return;
        }

        this.switchStorageService.SyncDatabaseToCurrentMedium(App.DatabaseFile);

        if (!await this.presentationController.ShowSwitchStorageWindowAsync())
        {
            return;
        }

        InitTargetSettings();
    }

    #endregion

    #region Options Settings

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DisableEncryptionCommand))]
    [NotifyPropertyChangedFor(nameof(CanSelectNonEncryptionMode))]
    private ModeType modeType = ModeType.Unset;

    [ObservableProperty]
    private bool waitForDevice;

    public bool CanSelectNonEncryptionMode => ModeType != ModeType.Encryption;

    private void InitOptionsSettings()
    {
        if (this.configurationManager.Encrypt == 1)
        {
            this.ModeType = ModeType.Encryption;
        }
        else if (this.configurationManager.Compression == 1)
        {
            this.ModeType = ModeType.Compression;
        }
        else
        {
            this.ModeType = ModeType.RegularCopy;
        }

        this.WaitForDevice = this.configurationManager.ShowWaitOnMediaAutoBackups == "1";
    }

    [RelayCommand]
    private async Task ShowCompressionExclusionsWindow()
    {
        await this.presentationController.ShowCompressionExclusionsWindowAsync();
    }

    partial void OnWaitForDeviceChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;

        this.configurationManager.ShowWaitOnMediaAutoBackups = newValue ? "1" : "0";
    }

    [RelayCommand]
    public async Task ChangeModeTypeAsync(ModeType newValue)
    {
        var oldValue = ModeType;
        if (oldValue == ModeType.Unset || oldValue == newValue)
        {
            return;
        }

        if (oldValue == ModeType.Encryption && newValue != ModeType.Encryption)
        {
            await DisableEncryption();
            if (this.configurationManager.Encrypt == 1)
            {
                return;
            }
        }

        if (newValue == ModeType.Compression)
        {
            ModeType = newValue;
            this.configurationManager.Compression = 1;
            this.configurationManager.Encrypt = 0;
        }
        else if (newValue == ModeType.Encryption)
        {
            // setup encryption
            var (password, _) = await presentationController.RequestPasswordAsync();
            if (password == null)
            {
                return;
            }

            ModeType = newValue;
            this.configurationManager.EncryptPassMD5 = Hash.GetMD5Hash(password);
            this.configurationManager.Encrypt = 1;
            this.configurationManager.Compression = 0;
        }
        else
        {
            ModeType = newValue;
            this.configurationManager.Compression = 0;
            this.configurationManager.Encrypt = 0;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisableEncryption))]
    private async Task DisableEncryption()
    {
        if (this.configurationManager.MediumType == MediaType.FileTransferServer)
        {
            if (!await TryDeleteAllBackupsWithMediaCheckAsync())
            {
                return;
            }

            this.configurationManager.Encrypt = 0;
            this.configurationManager.EncryptPassMD5 = "";
            InitOptionsSettings();
            return;
        }

        if (!await this.jobService.CheckMediaAsync(ActionType.Restore))
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
    private TaskType taskType = TaskType.Unset;

    [ObservableProperty]
    private bool stopBackupWhenBatteryMode;

    private void InitModeSettings()
    {
        this.TaskType = this.configurationManager.TaskType;
        this.StopBackupWhenBatteryMode = this.configurationManager.DeativateAutoBackupsWhenAkku == "1";
    }

    partial void OnStopBackupWhenBatteryModeChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;

        var value = newValue ? "1" : "0";
        if (configurationManager.DeativateAutoBackupsWhenAkku == value)
        {
            return;
        }

        configurationManager.DeativateAutoBackupsWhenAkku = value;
        _ = orchestrationService.RefreshAutomationAsync();
    }

    partial void OnTaskTypeChanged(TaskType oldValue, TaskType newValue)
    {
        if (oldValue == TaskType.Unset) return;
        if (oldValue == newValue) return;

        this.configurationManager.TaskType = newValue;
        _ = orchestrationService.RefreshAutomationAsync();
    }

    [RelayCommand]
    private async Task ShowScheduleEditorWindow()
    {
        await this.presentationController.ShowScheduleEditorWindowAsync();
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

    [ObservableProperty]
    private bool launchAtWindowsStartup;

    [ObservableProperty]
    private bool automaticallyCheckForUpdates = true;

    [ObservableProperty]
    private bool downloadBetaUpdates;

    void InitEnhancedSettings()
    {
        suppressEnhancedSettingsPersistence = true;
        try
        {
            var remindSpace = this.configurationManager.RemindSpace;
            var reminderEnabled = int.TryParse(remindSpace, out var diskSpace) && diskSpace >= 0;
            this.NotificationWhenDiskspaceLow = reminderEnabled ? diskSpace : 0;
            this.EnableNotificationWhenDiskspaceLow = reminderEnabled;

            this.EnableDirectoryLocalization = this.configurationManager.ShowLocalizedPath == "1";
            this.EnableNotificationWhenBackupFinished = this.configurationManager.InfoBackupDone == "1";
            this.EnableNotificationWhenBackupDeviceNotReady = this.configurationManager.Medium == "1";

            this.EnableNotificationWhenBackupOutdated = !string.IsNullOrEmpty(this.configurationManager.RemindAfterDays);
            this.NotificationWhenBackupOutdated = int.TryParse(this.configurationManager.RemindAfterDays, out var days) ? days : 0;

            LaunchAtWindowsStartup = startupLaunchAdapter.IsEnabled();
        }
        finally
        {
            suppressEnhancedSettingsPersistence = false;
        }

        _ = LoadUpdateSettingsAsync();
    }

    private async Task LoadUpdateSettingsAsync()
    {
        var autoSearch = await updateService.GetAutoSearchEnabledAsync();
        var downloadBeta = await updateService.GetDownloadBetaAsync();

        suppressEnhancedSettingsPersistence = true;
        try
        {
            AutomaticallyCheckForUpdates = autoSearch;
            DownloadBetaUpdates = downloadBeta;
        }
        finally
        {
            suppressEnhancedSettingsPersistence = false;
        }
    }

    partial void OnEnableNotificationWhenDiskspaceLowChanged(bool oldValue, bool newValue)
    {
        if (suppressEnhancedSettingsPersistence || oldValue == newValue) return;

        if (newValue)
        {
            this.configurationManager.RemindSpace = this.NotificationWhenDiskspaceLow.ToString();
        }
        else
        {
            this.configurationManager.RemindSpace = RemindSpaceOffSentinel;
        }
    }

    partial void OnNotificationWhenDiskspaceLowChanged(int oldValue, int newValue)
    {
        if (suppressEnhancedSettingsPersistence || oldValue == newValue) return;
        if (newValue == 0) return;
        this.configurationManager.RemindSpace = newValue.ToString();
    }

    partial void OnEnableDirectoryLocalizationChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        this.configurationManager.ShowLocalizedPath = newValue ? "1" : "0";
    }

    partial void OnEnableNotificationWhenBackupFinishedChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        this.configurationManager.InfoBackupDone = newValue ? "1" : "0";
    }

    partial void OnEnableNotificationWhenBackupDeviceNotReadyChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        this.configurationManager.Medium = newValue ? "1" : "0";
    }

    partial void OnEnableNotificationWhenBackupOutdatedChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue)
        {
            this.configurationManager.RemindAfterDays = this.NotificationWhenBackupOutdated.ToString();
        }
        else
        {
            this.configurationManager.RemindAfterDays = string.Empty;
        }
    }

    partial void OnNotificationWhenBackupOutdatedChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        if (newValue == 0) return;
        this.configurationManager.RemindAfterDays = newValue.ToString();
    }

    partial void OnLaunchAtWindowsStartupChanged(bool oldValue, bool newValue)
    {
        if (suppressEnhancedSettingsPersistence || oldValue == newValue)
        {
            return;
        }

        if (startupLaunchAdapter.TrySetEnabled(newValue))
        {
            return;
        }

        suppressEnhancedSettingsPersistence = true;
        try
        {
            LaunchAtWindowsStartup = oldValue;
        }
        finally
        {
            suppressEnhancedSettingsPersistence = false;
        }

        _ = presentationController.ShowMessageBoxAsync(
            "Settings_Enhanced_AccessDenied_Title".GetLocalized(),
            "Settings_Enhanced_AccessDenied_Text".GetLocalized(),
            [new UICommand("MsgBox_OK".GetLocalized())]);
    }

    partial void OnAutomaticallyCheckForUpdatesChanged(bool oldValue, bool newValue)
    {
        if (suppressEnhancedSettingsPersistence || oldValue == newValue)
        {
            return;
        }

        _ = updateService.SetAutoSearchEnabledAsync(newValue);
    }

    partial void OnDownloadBetaUpdatesChanged(bool oldValue, bool newValue)
    {
        if (suppressEnhancedSettingsPersistence || oldValue == newValue)
        {
            return;
        }

        _ = updateService.SetDownloadBetaAsync(newValue);
    }

    #endregion

    public SettingsViewModel(
        IConfigurationManager configurationManager,
        IPresentationService presentationService,
        IJobService jobService,
        IQueryManager queryManager,
        IBackupTargetService backupTargetService,
        ISwitchStorageService switchStorageService,
        IOrchestrationService orchestrationService,
        IStartupLaunchAdapter startupLaunchAdapter,
        IUpdateService updateService)
    {
        this.configurationManager = configurationManager;
        this.presentationController = presentationService;
        this.jobService = jobService;
        this.queryManager = queryManager;
        this.backupTargetService = backupTargetService;
        this.switchStorageService = switchStorageService;
        this.orchestrationService = orchestrationService;
        this.startupLaunchAdapter = startupLaunchAdapter;
        this.updateService = updateService;
    }

    private static string DecryptConfigurationPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return string.Empty;
        }

        try
        {
            return Crypto.DecryptString(password, System.Security.Cryptography.DataProtectionScope.LocalMachine);
        }
        catch
        {
            return string.Empty;
        }
    }

    public void OnNavigatedFrom()
    {
        ApplyCurrentBackupTarget();
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

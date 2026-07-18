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
    private readonly IOrchestrationService orchestrationService;
    private readonly IStartupLaunchAdapter startupLaunchAdapter;
    private readonly IUpdateService updateService;

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
            SourceValidationErrorMessage = "Selected source folder path is invalid.";
            return false;
        }

        if (PathRules.IsDriveRoot(fullPath))
        {
            SourceValidationErrorMessage = "Selecting a drive root is risky. Choose a specific folder instead.";
            return false;
        }

        var folderName = Path.GetFileName(fullPath);
        if (this.Sources.Any(source =>
            string.Equals(Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)), folderName, StringComparison.OrdinalIgnoreCase)))
        {
            SourceValidationErrorMessage = "A source folder with the same name is already configured.";
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
        get => new List<MediaType>() { MediaType.LocalDevice, MediaType.FileTransferServer, MediaType.WebDav };
    }

    [ObservableProperty]
    private MediaType selectedMediaType;

    [ObservableProperty]
    private Visibility localDeviceVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility ftpRemoteVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility webDavRemoteVisibility = Visibility.Collapsed;

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

    [ObservableProperty]
    private bool isMovingBackupTarget;

    [ObservableProperty]
    private bool isBackupTargetChangeEnabled = true;

    [ObservableProperty]
    private Visibility backupTargetMoveProgressVisibility = Visibility.Collapsed;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCompressionExclusionCommand))]
    private string? selectedCompressionExclusion;

    [ObservableProperty]
    private string? compressionExclusionInputText;

    public ObservableCollection<string> CompressionExclusions { get; } = new();

    private void InitTargetSettings()
    {
        // selected media type
        this.SelectedMediaType = this.configurationManager.MediumType;
        UpdateTargetPanelVisibility(SelectedMediaType);

        // local device
        this.LocalDevicePath = this.configurationManager.BackupFolder;
        this.LocalUNCUser = this.configurationManager.UNCUsername;
        this.LocalUNCPassword = DecryptConfigurationPassword(this.configurationManager.UNCPassword);

        // remote (FTP / WebDAV share Ftp* configuration fields)
        this.FtpRemoteHost = this.configurationManager.FtpHost;
        this.FtpRemotePort = ResolveRemotePort(SelectedMediaType, this.configurationManager.FtpPort);
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

    private static int ResolveRemotePort(MediaType mediaType, string? configuredPort)
    {
        if (!string.IsNullOrEmpty(configuredPort) && int.TryParse(configuredPort, out var port))
        {
            return port;
        }

        return mediaType == MediaType.WebDav ? 443 : 21;
    }

    private void UpdateTargetPanelVisibility(MediaType mediaType)
    {
        LocalDeviceVisibility = mediaType == MediaType.LocalDevice ? Visibility.Visible : Visibility.Collapsed;
        FtpRemoteVisibility = mediaType == MediaType.FileTransferServer ? Visibility.Visible : Visibility.Collapsed;
        WebDavRemoteVisibility = mediaType == MediaType.WebDav ? Visibility.Visible : Visibility.Collapsed;
    }

    public static string GetMediaTypeDisplayName(MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.LocalDevice => "MediaType_LocalDevice".GetLocalized(),
            MediaType.WebDav => "MediaType_WebDav".GetLocalized(),
            _ => "MediaType_FileTransferServer".GetLocalized()
        };
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCheckRemoteConnection))]
    public async Task CheckFtpRemote()
    {
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

    [RelayCommand(CanExecute = nameof(CanExecuteCheckRemoteConnection))]
    public async Task CheckWebDavRemote()
    {
        var ok = WebDavStorage.CheckConnection(FtpRemoteHost, FtpRemotePort, FtpRemoteUser, FtpRemotePassword, FtpRemotePath);

        if (ok)
        {
            await presentationController.ShowMessageBoxAsync("MsgBox_WebDav_Successful_Title".GetLocalized(), "MsgBox_WebDav_Successful_Text".GetLocalized(), new List<IUICommand> { new UICommand("OK") });
        }
        else
        {
            await presentationController.ShowMessageBoxAsync("MsgBox_WebDav_Unuccessful_Title".GetLocalized(), "MsgBox_WebDav_Unuccessful_Text".GetLocalized(), new List<IUICommand> { new UICommand("OK") });
        }
    }

    private bool CanExecuteCheckRemoteConnection()
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
        this.configurationManager.MediumType = newValue;
        UpdateTargetPanelVisibility(newValue);

        if (newValue == MediaType.WebDav && FtpRemotePort == 21)
        {
            FtpRemotePort = 443;
        }
        else if (newValue == MediaType.FileTransferServer && FtpRemotePort == 443)
        {
            FtpRemotePort = 21;
        }
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
        this.configurationManager.FtpHost = newValue;
        CheckFtpRemoteCommand.NotifyCanExecuteChanged();
        CheckWebDavRemoteCommand.NotifyCanExecuteChanged();
    }

    partial void OnFtpRemotePortChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        this.configurationManager.FtpPort = newValue.ToString();
    }

    partial void OnFtpRemoteUserChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        this.configurationManager.FtpUser = newValue;
        CheckFtpRemoteCommand.NotifyCanExecuteChanged();
        CheckWebDavRemoteCommand.NotifyCanExecuteChanged();
    }

    partial void OnFtpRemotePasswordChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        this.configurationManager.FtpPass = newValue;
        CheckFtpRemoteCommand.NotifyCanExecuteChanged();
        CheckWebDavRemoteCommand.NotifyCanExecuteChanged();
    }

    partial void OnFtpRemotePathChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        this.configurationManager.FtpFolder = newValue;
    }

    partial void OnFtpRemoteEncodingChanged(string? oldValue, string newValue)
    {
        if (oldValue == null || oldValue == newValue) return;
        this.configurationManager.FtpCoding = newValue;
    }

    partial void OnFtpRemoteEnforceUnencryptedChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        this.configurationManager.FtpEncryptionMode = newValue ? "3" : "0";
    }

    public void UseLocalPath(string folderPath)
    {
        this.LocalDevicePath = folderPath;
        this.configurationManager.BackupFolder = folderPath;

        if (folderPath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return;
        }

        this.LocalUNCUser = "";
        this.LocalUNCPassword = "";

        if (folderPath.Length >= 3)
        {
            this.configurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(folderPath[..3]);
            if (this.configurationManager.MediaVolumeSerial == null || this.configurationManager.MediaVolumeSerial == "0")
            {
                this.configurationManager.MediaVolumeSerial = "";
            }
        }
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
                "Could not move backup data",
                result.ErrorMessage ?? "The backup data could not be moved.",
                [new UICommand("OK")]);
        }
        finally
        {
            IsMovingBackupTarget = false;
            IsBackupTargetChangeEnabled = true;
            BackupTargetMoveProgressVisibility = Visibility.Collapsed;
        }
    }

    #endregion

    #region Options Settings

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DisableEncryptionCommand))]
    private ModeType modeType = ModeType.Unset;

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

        this.CompressionExclusions.Clear();
        foreach (var entry in CompressionExclusionFormatter.Parse(this.configurationManager.ExcludeCompression))
        {
            this.CompressionExclusions.Add(entry);
        }
    }

    [RelayCommand]
    public void AddCompressionExclusion(string? extension)
    {
        var normalized = CompressionExclusionFormatter.NormalizeExtension(extension);
        if (string.IsNullOrEmpty(normalized) || this.CompressionExclusions.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        this.CompressionExclusions.Add(normalized);
        CompressionExclusionInputText = null;
        SaveCompressionExclusions();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveCompressionExclusion))]
    private void RemoveCompressionExclusion()
    {
        if (string.IsNullOrEmpty(SelectedCompressionExclusion))
        {
            return;
        }

        this.CompressionExclusions.Remove(SelectedCompressionExclusion);
        SelectedCompressionExclusion = null;
        SaveCompressionExclusions();
    }

    private bool CanRemoveCompressionExclusion() => !string.IsNullOrEmpty(SelectedCompressionExclusion);

    private void SaveCompressionExclusions()
    {
        this.configurationManager.ExcludeCompression = CompressionExclusionFormatter.Format(this.CompressionExclusions);
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
        this.EnableNotificationWhenDiskspaceLow = !string.IsNullOrEmpty(this.configurationManager.RemindSpace);
        this.NotificationWhenDiskspaceLow = int.TryParse(this.configurationManager.RemindSpace, out var diskSpace) ? diskSpace : 0;

        this.EnableDirectoryLocalization = this.configurationManager.ShowLocalizedPath == "1";
        this.EnableNotificationWhenBackupFinished = this.configurationManager.InfoBackupDone == "1";
        this.EnableNotificationWhenBackupDeviceNotReady = this.configurationManager.Medium == "1";

        this.EnableNotificationWhenBackupOutdated = !string.IsNullOrEmpty(this.configurationManager.RemindAfterDays);
        this.NotificationWhenBackupOutdated = int.TryParse(this.configurationManager.RemindAfterDays, out var days) ? days : 0;

        // Set backing fields so load does not re-enter the persistence handlers.
        launchAtWindowsStartup = startupLaunchAdapter.IsEnabled();
        OnPropertyChanged(nameof(LaunchAtWindowsStartup));
        _ = LoadUpdateSettingsAsync();
    }

    private async Task LoadUpdateSettingsAsync()
    {
        automaticallyCheckForUpdates = await updateService.GetAutoSearchEnabledAsync();
        downloadBetaUpdates = await updateService.GetDownloadBetaAsync();
        OnPropertyChanged(nameof(AutomaticallyCheckForUpdates));
        OnPropertyChanged(nameof(DownloadBetaUpdates));
    }

    partial void OnEnableNotificationWhenDiskspaceLowChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;

        if (newValue)
        {
            this.configurationManager.RemindSpace = this.NotificationWhenDiskspaceLow.ToString();
        }
        else
        {
            this.configurationManager.RemindSpace = string.Empty;
        }
    }

    partial void OnNotificationWhenDiskspaceLowChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
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
        if (oldValue == newValue)
        {
            return;
        }

        if (startupLaunchAdapter.TrySetEnabled(newValue))
        {
            return;
        }

        launchAtWindowsStartup = oldValue;
        OnPropertyChanged(nameof(LaunchAtWindowsStartup));
        _ = presentationController.ShowMessageBoxAsync(
            "Access denied",
            "Windows startup could not be changed. Check your permissions and try again.",
            [new UICommand("OK")]);
    }

    partial void OnAutomaticallyCheckForUpdatesChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        _ = updateService.SetAutoSearchEnabledAsync(newValue);
    }

    partial void OnDownloadBetaUpdatesChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue)
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
        IOrchestrationService orchestrationService,
        IStartupLaunchAdapter startupLaunchAdapter,
        IUpdateService updateService)
    {
        this.configurationManager = configurationManager;
        this.presentationController = presentationService;
        this.jobService = jobService;
        this.queryManager = queryManager;
        this.backupTargetService = backupTargetService;
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

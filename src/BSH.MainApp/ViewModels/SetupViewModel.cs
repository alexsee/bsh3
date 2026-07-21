// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage.Pickers;
using Windows.UI.Popups;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly IConfigurationManager configurationManager;
    private readonly ISetupService setupService;
    private readonly IOrchestrationService orchestrationService;
    private readonly IJobService jobService;
    private readonly INavigationService navigationService;
    private readonly IPresentationService presentationService;
    private readonly IStatusService? statusService;
    private readonly IDbClientFactory? dbClientFactory;
    private string? databaseFileOverride;

    public SetupViewModel(
        IConfigurationManager configurationManager,
        ISetupService setupService,
        IOrchestrationService orchestrationService,
        IJobService jobService,
        INavigationService navigationService,
        IPresentationService presentationService,
        IStatusService? statusService = null,
        IDbClientFactory? dbClientFactory = null)
    {
        this.configurationManager = configurationManager;
        this.setupService = setupService;
        this.orchestrationService = orchestrationService;
        this.jobService = jobService;
        this.navigationService = navigationService;
        this.presentationService = presentationService;
        this.statusService = statusService;
        this.dbClientFactory = dbClientFactory;

        FtpPort = "21";
        FtpEncoding = "UTF8";
        ImportFtpPort = "21";
        ImportFtpEncoding = "UTF8";
        SelectedTaskType = TaskType.Auto;
        SelectedTargetKind = MediaTargetKind.LocalDrive;
        SelectedImportSourceKind = SetupImportSourceKind.LocalMedia;
        CurrentStep = SetupWizardStep.Welcome;
    }

    /// <summary>
    /// Test helper to avoid touching the live app database path.
    /// </summary>
    public void UseDatabaseFile(string databaseFile)
    {
        databaseFileOverride = databaseFile;
    }

    private string DatabaseFile => databaseFileOverride ?? App.DatabaseFile;

    public ObservableCollection<string> Sources { get; } = new();

    public ObservableCollection<SetupDriveItem> AvailableDrives { get; } = new();

    public ObservableCollection<DiscoveredBackup> DiscoveredBackups { get; } = new();

    public ObservableCollection<SourceRemap> SourceRemaps { get; } = new();

    [ObservableProperty]
    private SetupWizardStep currentStep;

    [ObservableProperty]
    private string title = "Setup_Welcome_Title".GetLocalized();

    [ObservableProperty]
    private string description = "Setup_Welcome_Description".GetLocalized();

    [ObservableProperty]
    private string? validationErrorMessage;

    [ObservableProperty]
    private string? selectedSource;

    [ObservableProperty]
    private MediaTargetKind selectedTargetKind;

    [ObservableProperty]
    private SetupDriveItem? selectedDrive;

    [ObservableProperty]
    private string? selectedDriveRoot;

    [ObservableProperty]
    private string uncPath = "";

    [ObservableProperty]
    private string uncUsername = "";

    [ObservableProperty]
    private string uncPassword = "";

    [ObservableProperty]
    private string ftpHost = "";

    [ObservableProperty]
    private string ftpPort = "21";

    [ObservableProperty]
    private string ftpUser = "";

    [ObservableProperty]
    private string ftpPassword = "";

    [ObservableProperty]
    private string ftpFolder = "";

    [ObservableProperty]
    private string ftpEncoding = "UTF8";

    [ObservableProperty]
    private bool ftpEnforceUnencrypted;

    [ObservableProperty]
    private TaskType selectedTaskType = TaskType.Auto;

    [ObservableProperty]
    private SetupImportSourceKind selectedImportSourceKind;

    [ObservableProperty]
    private SetupDriveItem? selectedImportDrive;

    [ObservableProperty]
    private DiscoveredBackup? selectedDiscoveredBackup;

    [ObservableProperty]
    private string explicitImportPath = "";

    [ObservableProperty]
    private string importFtpHost = "";

    [ObservableProperty]
    private string importFtpPort = "21";

    [ObservableProperty]
    private string importFtpUser = "";

    [ObservableProperty]
    private string importFtpPassword = "";

    [ObservableProperty]
    private string importFtpFolder = "";

    [ObservableProperty]
    private string importFtpEncoding = "UTF8";

    [ObservableProperty]
    private bool importFtpEnforceUnencrypted;

    [ObservableProperty]
    private SourceRemap? selectedSourceRemap;

    [ObservableProperty]
    private Visibility welcomeVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility wizardChromeVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility sourcesVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility targetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility modeVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility importMediaVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility importSelectVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility importRemapVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility progressVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility backButtonVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility nextButtonVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility localTargetVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility uncTargetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility ftpTargetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility localImportVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility ftpImportVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility explicitImportVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private bool isBusy;

    partial void OnCurrentStepChanged(SetupWizardStep value)
    {
        UpdateStepPresentation();
    }

    partial void OnSelectedDriveChanged(SetupDriveItem? value)
    {
        SelectedDriveRoot = value?.RootPath;
    }

    partial void OnSelectedTargetKindChanged(MediaTargetKind value)
    {
        UpdateTargetVisibility();
    }

    partial void OnSelectedImportSourceKindChanged(SetupImportSourceKind value)
    {
        UpdateImportSourceVisibility();
    }

    public void BeginNewSetup()
    {
        ValidationErrorMessage = null;
        Sources.Clear();
        var defaultSource = setupService.GetDefaultSourceFolder();
        if (!string.IsNullOrEmpty(defaultSource))
        {
            Sources.Add(defaultSource);
        }

        RefreshDrives();
        CurrentStep = SetupWizardStep.Sources;
    }

    public void BeginImport()
    {
        ValidationErrorMessage = null;
        RefreshDrives();
        DiscoveredBackups.Clear();
        SourceRemaps.Clear();
        CurrentStep = SetupWizardStep.ImportMedia;
    }

    public bool TryAddSourceFolder(string folderPath)
    {
        ValidationErrorMessage = null;
        if (!setupService.TryAddSourceFolder(Sources, folderPath, out var error))
        {
            ValidationErrorMessage = error;
            return false;
        }

        return true;
    }

    public void LoadSourceRemaps(IEnumerable<string> sourceFolders)
    {
        SourceRemaps.Clear();
        foreach (var folder in sourceFolders.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            SourceRemaps.Add(new SourceRemap(folder, folder));
        }
    }

    public bool TryUpdateSourceRemap(int index, string newPath)
    {
        ValidationErrorMessage = null;
        if (index < 0 || index >= SourceRemaps.Count)
        {
            ValidationErrorMessage = "Setup_Validation_SelectSourceToRemap".GetLocalized();
            return false;
        }

        var remap = SourceRemaps[index];
        if (!setupService.CanRemapSourcePath(remap.OriginalPath, newPath, out var error))
        {
            ValidationErrorMessage = error;
            return false;
        }

        remap.CurrentPath = newPath;
        SourceRemaps[index] = remap;
        SelectedSourceRemap = remap;
        return true;
    }

    public async Task<bool> FinishNewSetupAsync()
    {
        ValidationErrorMessage = null;

        if (Sources.Count == 0)
        {
            ValidationErrorMessage = "Setup_Validation_AddAtLeastOneSource".GetLocalized();
            return false;
        }

        var configuration = BuildNewSetupConfiguration();
        if (configuration == null)
        {
            return false;
        }

        if (SelectedTargetKind == MediaTargetKind.LocalDrive)
        {
            if (string.IsNullOrWhiteSpace(configuration.LocalBackupFolder))
            {
                ValidationErrorMessage = "Setup_Validation_SelectTargetDrive".GetLocalized();
                return false;
            }

            // Only reject an existing folder when we have not already persisted this setup.
            if (configurationManager.IsConfigured != "1" &&
                !setupService.IsLocalBackupFolderAvailable(configuration.LocalBackupFolder))
            {
                ValidationErrorMessage = "Setup_Validation_TargetContainsBackup".GetLocalized();
                return false;
            }
        }

        CurrentStep = SetupWizardStep.Progress;
        IsBusy = true;

        try
        {
            if (configurationManager.IsConfigured != "1")
            {
                setupService.ApplyNewConfiguration(configuration);
            }

            if (SelectedTaskType == TaskType.Manual)
            {
                statusService?.SetSystemStatus(SystemStatus.DEACTIVATED);
                navigationService.NavigateTo(typeof(SettingsViewModel).FullName!, clearNavigation: true);
                return true;
            }

            await orchestrationService.StartAsync(true);
            var backupStarted = await jobService.CreateBackupAsync("CreateBackup_Title_First".GetLocalized(), "", statusDialog: false);
            if (!backupStarted)
            {
                ValidationErrorMessage = "Setup_Validation_FirstBackupFailed".GetLocalized();
                CurrentStep = SetupWizardStep.Mode;
                return false;
            }

            navigationService.NavigateTo(typeof(MainViewModel).FullName!, clearNavigation: true);
            return true;
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = ex.Message;
            CurrentStep = SetupWizardStep.Mode;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> FinishImportAsync()
    {
        ValidationErrorMessage = null;
        CurrentStep = SetupWizardStep.Progress;
        IsBusy = true;

        try
        {
            await setupService.RemapSourcesAsync(SourceRemaps.ToList());
            await orchestrationService.StartAsync(true);
            navigationService.NavigateTo(typeof(MainViewModel).FullName!, clearNavigation: true);
            return true;
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = ex.Message;
            CurrentStep = SetupWizardStep.ImportRemap;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectTarget(MediaTargetKind kind)
    {
        SelectedTargetKind = kind;
    }

    [RelayCommand]
    private void SelectImportSource(SetupImportSourceKind kind)
    {
        SelectedImportSourceKind = kind;
    }

    [RelayCommand]
    private void StartNewSetup()
    {
        BeginNewSetup();
    }

    [RelayCommand]
    private void StartImport()
    {
        BeginImport();
    }

    [RelayCommand]
    private async Task AddSourceFolderAsync()
    {
        var folder = await PickFolderAsync();
        if (folder != null)
        {
            TryAddSourceFolder(folder);
        }
    }

    [RelayCommand]
    private void RemoveSourceFolder()
    {
        if (string.IsNullOrEmpty(SelectedSource))
        {
            return;
        }

        Sources.Remove(SelectedSource);
        SelectedSource = null;
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        AvailableDrives.Clear();
        foreach (var drive in DriveInfo.GetDrives().Where(drive => drive.IsReady))
        {
            AvailableDrives.Add(new SetupDriveItem
            {
                DisplayName = $"{drive.Name} ({SafeVolumeLabel(drive)})",
                RootPath = drive.RootDirectory.FullName,
                DriveType = drive.DriveType
            });
        }
    }

    [RelayCommand]
    private async Task BrowseExplicitImportPathAsync()
    {
        ValidationErrorMessage = null;
        var folder = await PickFolderAsync();
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        ExplicitImportPath = folder;
        SelectedImportSourceKind = SetupImportSourceKind.ExplicitPath;
    }

    [RelayCommand]
    private async Task RemapSelectedSourceAsync()
    {
        if (SelectedSourceRemap == null)
        {
            ValidationErrorMessage = "Setup_Validation_SelectSourceToRemap".GetLocalized();
            return;
        }

        var folder = await PickFolderAsync();
        if (folder == null)
        {
            return;
        }

        var index = SourceRemaps.IndexOf(SelectedSourceRemap);
        TryUpdateSourceRemap(index, folder);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        ValidationErrorMessage = null;

        // After a database has been imported, going back would leave a half-finished state.
        if (CurrentStep == SetupWizardStep.ImportRemap)
        {
            return;
        }

        CurrentStep = CurrentStep switch
        {
            SetupWizardStep.Sources => SetupWizardStep.Welcome,
            SetupWizardStep.Target => SetupWizardStep.Sources,
            SetupWizardStep.Mode => SetupWizardStep.Target,
            SetupWizardStep.ImportMedia => SetupWizardStep.Welcome,
            SetupWizardStep.ImportSelect => SetupWizardStep.ImportMedia,
            _ => SetupWizardStep.Welcome
        };

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GoNextAsync()
    {
        ValidationErrorMessage = null;

        switch (CurrentStep)
        {
            case SetupWizardStep.Sources:
                if (Sources.Count == 0)
                {
                    ValidationErrorMessage = "Setup_Validation_AddAtLeastOneSource".GetLocalized();
                    return;
                }

                CurrentStep = SetupWizardStep.Target;
                break;

            case SetupWizardStep.Target:
                if (!await ValidateTargetAsync())
                {
                    return;
                }

                CurrentStep = SetupWizardStep.Mode;
                break;

            case SetupWizardStep.Mode:
                await FinishNewSetupAsync();
                break;

            case SetupWizardStep.ImportMedia:
                await AdvanceImportMediaAsync();
                break;

            case SetupWizardStep.ImportSelect:
                await ImportSelectedLocalBackupAsync();
                break;

            case SetupWizardStep.ImportRemap:
                await FinishImportAsync();
                break;
        }
    }

    private async Task<bool> ValidateTargetAsync()
    {
        switch (SelectedTargetKind)
        {
            case MediaTargetKind.LocalDrive:
                if (string.IsNullOrWhiteSpace(SelectedDriveRoot))
                {
                    ValidationErrorMessage = "Setup_Validation_SelectTargetDrive".GetLocalized();
                    return false;
                }

                var backupFolder = setupService.BuildLocalBackupFolder(SelectedDriveRoot);
                if (!setupService.IsLocalBackupFolderAvailable(backupFolder))
                {
                    ValidationErrorMessage = "Setup_Validation_TargetContainsBackup".GetLocalized();
                    return false;
                }

                return true;

            case MediaTargetKind.Unc:
                {
                    var probe = UncTargetProbe.Probe(UncPath, UncUsername, UncPassword, requireEmptyTarget: false);
                    if (probe.Status == UncProbeStatus.Ok)
                    {
                        UncPath = probe.NormalizedPath;
                        return true;
                    }

                    if (probe.Status == UncProbeStatus.InvalidPath)
                    {
                        ValidationErrorMessage = "Setup_Validation_EnterNetworkPath".GetLocalized();
                        return false;
                    }

                    ValidationErrorMessage = string.IsNullOrEmpty(probe.Detail)
                        ? "Setup_Validation_NetworkUnreachable".GetLocalized()
                        : string.Format(
                            "Setup_Validation_NetworkUnreachableWithError".GetLocalized() ?? "Setup_Validation_NetworkUnreachableWithError",
                            probe.Detail);
                    return false;
                }

            case MediaTargetKind.Ftp:
                try
                {
                    FtpFolder = FtpStorage.GetFtpPath(FtpFolder);
                    if (!int.TryParse(FtpPort, out var port))
                    {
                        ValidationErrorMessage = "Setup_Validation_FtpPortInvalid".GetLocalized();
                        return false;
                    }

                    var ok = FtpStorage.CheckConnection(FtpHost, port, FtpUser, FtpPassword, FtpFolder, FtpEncoding);
                    if (!ok)
                    {
                        ValidationErrorMessage = "Setup_Validation_FtpDirectoryNotFound".GetLocalized();
                        return false;
                    }

                    await presentationService.ShowMessageBoxAsync(
                        "Setup_FtpConnectionSuccessful_Title".GetLocalized(),
                        "Setup_FtpConnectionSuccessful_Text".GetLocalized(),
                        [new UICommand("MsgBox_OK".GetLocalized())]);
                    return true;
                }
                catch (Exception ex)
                {
                    ValidationErrorMessage = string.Format("Setup_Validation_FtpConnectionFailed".GetLocalized() ?? "Setup_Validation_FtpConnectionFailed", ex.Message);
                    return false;
                }

            default:
                ValidationErrorMessage = "Setup_Validation_SelectBackupTarget".GetLocalized();
                return false;
        }
    }

    private async Task AdvanceImportMediaAsync()
    {
        switch (SelectedImportSourceKind)
        {
            case SetupImportSourceKind.LocalMedia:
                if (SelectedImportDrive == null)
                {
                    ValidationErrorMessage = "Setup_Validation_SelectDeviceWithBackups".GetLocalized();
                    return;
                }

                DiscoveredBackups.Clear();
                foreach (var backup in setupService.DiscoverBackupsOnDrive(SelectedImportDrive.RootPath))
                {
                    DiscoveredBackups.Add(backup);
                }

                CurrentStep = SetupWizardStep.ImportSelect;
                break;

            case SetupImportSourceKind.ExplicitPath:
                if (string.IsNullOrWhiteSpace(ExplicitImportPath) ||
                    !setupService.BackupDatabaseExists(ExplicitImportPath))
                {
                    ValidationErrorMessage = "Setup_Validation_NoDatabaseAtPath".GetLocalized();
                    return;
                }

                await ImportFromFolderAsync(ExplicitImportPath, SetupImportSourceKind.ExplicitPath);
                break;

            case SetupImportSourceKind.Ftp:
                await ImportFromFtpAsync();
                break;
        }
    }

    private async Task ImportSelectedLocalBackupAsync()
    {
        if (SelectedDiscoveredBackup == null)
        {
            ValidationErrorMessage = "Setup_Validation_SelectBackupToImport".GetLocalized();
            return;
        }

        await ImportFromFolderAsync(SelectedDiscoveredBackup.FolderPath, SetupImportSourceKind.LocalMedia);
    }

    private async Task ImportFromFolderAsync(string folderPath, SetupImportSourceKind sourceKind)
    {
        var confirmed = await ConfirmDatabaseReplacementAsync();
        if (!confirmed)
        {
            return;
        }

        CurrentStep = SetupWizardStep.Progress;
        IsBusy = true;

        try
        {
            var sourceDatabase = Path.Combine(folderPath, "backup.bshdb");
            setupService.ReplaceDatabaseWithCopy(sourceDatabase, DatabaseFile);
            await ReinitializeAfterImportAsync();

            configurationManager.BackupFolder = folderPath;
            configurationManager.MediumType = MediaType.LocalDevice;

            if (sourceKind == SetupImportSourceKind.LocalMedia)
            {
                await setupService.ConvertFileTypesForLocalImportAsync();
            }

            PrepareSourceRemapStep();
            CurrentStep = SetupWizardStep.ImportRemap;
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = ex.Message;
            CurrentStep = SetupWizardStep.ImportMedia;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ImportFromFtpAsync()
    {
        try
        {
            ImportFtpFolder = FtpStorage.GetFtpPath(ImportFtpFolder);
            if (!int.TryParse(ImportFtpPort, out var port))
            {
                ValidationErrorMessage = "Setup_Validation_FtpPortInvalid".GetLocalized();
                return;
            }

            using (var storage = new FtpStorage(
                ImportFtpHost,
                port,
                ImportFtpUser,
                ImportFtpPassword,
                ImportFtpFolder,
                ImportFtpEncoding,
                !ImportFtpEnforceUnencrypted,
                0))
            {
                storage.Open();
                if (!storage.FileExists("backup.bshdb"))
                {
                    ValidationErrorMessage = "Setup_Validation_NoDatabaseOnFtp".GetLocalized();
                    return;
                }
            }

            var confirmed = await ConfirmDatabaseReplacementAsync();
            if (!confirmed)
            {
                return;
            }

            CurrentStep = SetupWizardStep.Progress;
            IsBusy = true;

            setupService.PrepareDatabaseReplacement(DatabaseFile);
            using (var storage = new FtpStorage(
                ImportFtpHost,
                port,
                ImportFtpUser,
                ImportFtpPassword,
                ImportFtpFolder,
                ImportFtpEncoding,
                !ImportFtpEnforceUnencrypted,
                0))
            {
                storage.Open();
                storage.CopyFileFromStorage(DatabaseFile, "backup.bshdb");
            }

            await ReinitializeAfterImportAsync();

            configurationManager.BackupFolder = "";
            configurationManager.FtpFolder = ImportFtpFolder;
            configurationManager.FtpHost = ImportFtpHost;
            configurationManager.FtpPass = ImportFtpPassword;
            configurationManager.FtpPort = ImportFtpPort;
            configurationManager.FtpUser = ImportFtpUser;
            configurationManager.FtpCoding = ImportFtpEncoding;
            configurationManager.MediumType = MediaType.FileTransferServer;
            await setupService.ConvertFileTypesForFtpImportAsync();

            PrepareSourceRemapStep();
            CurrentStep = SetupWizardStep.ImportRemap;
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = string.Format("Setup_Validation_FtpImportFailed".GetLocalized() ?? "Setup_Validation_FtpImportFailed", ex.Message);
            CurrentStep = SetupWizardStep.ImportMedia;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PrepareSourceRemapStep()
    {
        var sources = configurationManager.SourceFolder
            .Split('|', StringSplitOptions.RemoveEmptyEntries);
        LoadSourceRemaps(sources);
    }

    private async Task ReinitializeAfterImportAsync()
    {
        var factory = dbClientFactory ?? App.GetService<IDbClientFactory>();
        await factory.InitializeAsync(DatabaseFile);
        await configurationManager.InitializeAsync();
    }

    private async Task<bool> ConfirmDatabaseReplacementAsync()
    {
        var result = await presentationService.ShowMessageBoxAsync(
            "Setup_ReplaceDatabase_Title".GetLocalized(),
            "Setup_ReplaceDatabase_Text".GetLocalized(),
            [new UICommand("MsgBox_Yes".GetLocalized()), new UICommand("MsgBox_No".GetLocalized())],
            defaultCommandIndex: 0,
            cancelCommandIndex: 1);

        return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
    }

    private NewSetupConfiguration? BuildNewSetupConfiguration()
    {
        return SelectedTargetKind switch
        {
            MediaTargetKind.LocalDrive => new NewSetupConfiguration
            {
                SourceFolders = Sources.ToList(),
                TargetKind = MediaTargetKind.LocalDrive,
                LocalBackupFolder = string.IsNullOrWhiteSpace(SelectedDriveRoot)
                    ? null
                    : setupService.BuildLocalBackupFolder(SelectedDriveRoot),
                MediaVolumeSerial = ResolveVolumeSerial(SelectedDriveRoot),
                TaskType = SelectedTaskType
            },
            MediaTargetKind.Unc => new NewSetupConfiguration
            {
                SourceFolders = Sources.ToList(),
                TargetKind = MediaTargetKind.Unc,
                UncPath = UncPath,
                UncUsername = UncUsername,
                UncPassword = UncPassword,
                TaskType = SelectedTaskType
            },
            MediaTargetKind.Ftp => new NewSetupConfiguration
            {
                SourceFolders = Sources.ToList(),
                TargetKind = MediaTargetKind.Ftp,
                FtpHost = FtpHost,
                FtpPort = FtpPort,
                FtpUser = FtpUser,
                FtpPassword = FtpPassword,
                FtpFolder = FtpFolder,
                FtpEncoding = FtpEncoding,
                FtpEnforceUnencrypted = FtpEnforceUnencrypted,
                TaskType = SelectedTaskType
            },
            _ => null
        };
    }

    private static string ResolveVolumeSerial(string? driveRoot)
    {
        return MediaTargetApplier.ResolveVolumeSerial(driveRoot);
    }

    private async Task<string?> PickFolderAsync()
    {
        try
        {
            if (App.MainWindow?.Content == null)
            {
                ValidationErrorMessage = "Setup_Validation_WindowNotReady".GetLocalized();
                return null;
            }

            var folderPicker = new FolderPicker(App.MainWindow.AppWindow.Id)
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            var folder = await folderPicker.PickSingleFolderAsync();
            return folder?.Path;
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = string.Format("Setup_Validation_FolderPickerFailed".GetLocalized() ?? "Setup_Validation_FolderPickerFailed", ex.Message);
            return null;
        }
    }

    private void UpdateStepPresentation()
    {
        WelcomeVisibility = CurrentStep == SetupWizardStep.Welcome ? Visibility.Visible : Visibility.Collapsed;
        WizardChromeVisibility = CurrentStep == SetupWizardStep.Welcome ? Visibility.Collapsed : Visibility.Visible;
        SourcesVisibility = CurrentStep == SetupWizardStep.Sources ? Visibility.Visible : Visibility.Collapsed;
        TargetVisibility = CurrentStep == SetupWizardStep.Target ? Visibility.Visible : Visibility.Collapsed;
        ModeVisibility = CurrentStep == SetupWizardStep.Mode ? Visibility.Visible : Visibility.Collapsed;
        ImportMediaVisibility = CurrentStep == SetupWizardStep.ImportMedia ? Visibility.Visible : Visibility.Collapsed;
        ImportSelectVisibility = CurrentStep == SetupWizardStep.ImportSelect ? Visibility.Visible : Visibility.Collapsed;
        ImportRemapVisibility = CurrentStep == SetupWizardStep.ImportRemap ? Visibility.Visible : Visibility.Collapsed;
        ProgressVisibility = CurrentStep == SetupWizardStep.Progress ? Visibility.Visible : Visibility.Collapsed;

        BackButtonVisibility = CurrentStep is SetupWizardStep.Welcome or SetupWizardStep.Progress or SetupWizardStep.ImportRemap
            ? Visibility.Collapsed
            : Visibility.Visible;
        NextButtonVisibility = CurrentStep is SetupWizardStep.Welcome or SetupWizardStep.Progress
            ? Visibility.Collapsed
            : Visibility.Visible;

        (Title, Description) = CurrentStep switch
        {
            SetupWizardStep.Welcome => ("", ""),
            SetupWizardStep.Sources => ("Setup_Step_Sources_Title".GetLocalized(), "Setup_Step_Sources_Description".GetLocalized()),
            SetupWizardStep.Target => ("Setup_Step_Target_Title".GetLocalized(), "Setup_Step_Target_Description".GetLocalized()),
            SetupWizardStep.Mode => ("Setup_Step_Mode_Title".GetLocalized(), "Setup_Step_Mode_Description".GetLocalized()),
            SetupWizardStep.ImportMedia => ("Setup_Step_ImportMedia_Title".GetLocalized(), "Setup_Step_ImportMedia_Description".GetLocalized()),
            SetupWizardStep.ImportSelect => ("Setup_Step_ImportSelect_Title".GetLocalized(), "Setup_Step_ImportSelect_Description".GetLocalized()),
            SetupWizardStep.ImportRemap => ("Setup_Step_ImportRemap_Title".GetLocalized(), "Setup_Step_ImportRemap_Description".GetLocalized()),
            SetupWizardStep.Progress => ("Setup_Step_Progress_Title".GetLocalized(), "Setup_Step_Progress_Description".GetLocalized()),
            _ => ("Setup_Step_Default_Title".GetLocalized(), "")
        };

        UpdateTargetVisibility();
        UpdateImportSourceVisibility();
    }

    private void UpdateTargetVisibility()
    {
        LocalTargetVisibility = SelectedTargetKind == MediaTargetKind.LocalDrive ? Visibility.Visible : Visibility.Collapsed;
        UncTargetVisibility = SelectedTargetKind == MediaTargetKind.Unc ? Visibility.Visible : Visibility.Collapsed;
        FtpTargetVisibility = SelectedTargetKind == MediaTargetKind.Ftp ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateImportSourceVisibility()
    {
        LocalImportVisibility = SelectedImportSourceKind == SetupImportSourceKind.LocalMedia ? Visibility.Visible : Visibility.Collapsed;
        FtpImportVisibility = SelectedImportSourceKind == SetupImportSourceKind.Ftp ? Visibility.Visible : Visibility.Collapsed;
        ExplicitImportVisibility = SelectedImportSourceKind == SetupImportSourceKind.ExplicitPath ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string SafeVolumeLabel(DriveInfo drive)
    {
        try
        {
            return drive.VolumeLabel;
        }
        catch
        {
            return drive.DriveType.ToString();
        }
    }
}

public sealed class SetupDriveItem
{
    public required string DisplayName
    {
        get; init;
    }

    public required string RootPath
    {
        get; init;
    }

    public required DriveType DriveType
    {
        get; init;
    }
}

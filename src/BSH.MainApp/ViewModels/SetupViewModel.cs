// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using WinUIEx;

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
        SelectedTargetKind = SetupTargetKind.LocalDrive;
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
    private string title = "Welcome";

    [ObservableProperty]
    private string description = "Choose how you want to get started.";

    [ObservableProperty]
    private string? validationErrorMessage;

    [ObservableProperty]
    private string? selectedSource;

    [ObservableProperty]
    private SetupTargetKind selectedTargetKind;

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

    partial void OnSelectedTargetKindChanged(SetupTargetKind value)
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
            ValidationErrorMessage = "Select a source folder to remap.";
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
            ValidationErrorMessage = "Add at least one source folder.";
            return false;
        }

        var configuration = BuildNewSetupConfiguration();
        if (configuration == null)
        {
            return false;
        }

        if (SelectedTargetKind == SetupTargetKind.LocalDrive)
        {
            if (string.IsNullOrWhiteSpace(configuration.LocalBackupFolder))
            {
                ValidationErrorMessage = "Select a backup target drive.";
                return false;
            }

            // Only reject an existing folder when we have not already persisted this setup.
            if (configurationManager.IsConfigured != "1" &&
                !setupService.IsLocalBackupFolderAvailable(configuration.LocalBackupFolder))
            {
                ValidationErrorMessage = "The selected target already contains a backup for this computer and user.";
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
            var backupStarted = await jobService.CreateBackupAsync("First backup", "", statusDialog: false);
            if (!backupStarted)
            {
                ValidationErrorMessage = "Setup was saved, but the first backup could not be started.";
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
            ValidationErrorMessage = "Select a source folder to remap.";
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
                    ValidationErrorMessage = "Add at least one source folder.";
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
            case SetupTargetKind.LocalDrive:
                if (string.IsNullOrWhiteSpace(SelectedDriveRoot))
                {
                    ValidationErrorMessage = "Select a backup target drive.";
                    return false;
                }

                var backupFolder = setupService.BuildLocalBackupFolder(SelectedDriveRoot);
                if (!setupService.IsLocalBackupFolderAvailable(backupFolder))
                {
                    ValidationErrorMessage = "The selected target already contains a backup for this computer and user.";
                    return false;
                }

                return true;

            case SetupTargetKind.Unc:
                {
                    var path = UncPath.Replace("//", @"\", StringComparison.Ordinal);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        ValidationErrorMessage = "Enter a network path.";
                        return false;
                    }

                    try
                    {
                        using var connection = new Brightbits.BSH.Engine.Security.NetworkConnection(path, UncUsername, UncPassword);
                        if (!Directory.Exists(path))
                        {
                            ValidationErrorMessage = "The network path could not be reached.";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        ValidationErrorMessage = "The network path could not be reached. " + ex.Message;
                        return false;
                    }

                    return true;
                }

            case SetupTargetKind.Ftp:
                try
                {
                    FtpFolder = FtpStorage.GetFtpPath(FtpFolder);
                    if (!int.TryParse(FtpPort, out var port))
                    {
                        ValidationErrorMessage = "FTP port is invalid.";
                        return false;
                    }

                    var ok = FtpStorage.CheckConnection(FtpHost, port, FtpUser, FtpPassword, FtpFolder, FtpEncoding);
                    if (!ok)
                    {
                        ValidationErrorMessage = "The FTP directory was not found.";
                        return false;
                    }

                    await presentationService.ShowMessageBoxAsync(
                        "FTP connection successful",
                        "The FTP connection was verified successfully.",
                        [new UICommand("OK")]);
                    return true;
                }
                catch (Exception ex)
                {
                    ValidationErrorMessage = "FTP connection failed. " + ex.Message;
                    return false;
                }

            default:
                ValidationErrorMessage = "Select a backup target.";
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
                    ValidationErrorMessage = "Select a device that contains backups.";
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
                    ValidationErrorMessage = "No backup database was found at the selected path.";
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
            ValidationErrorMessage = "Select a backup to import.";
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
                ValidationErrorMessage = "FTP port is invalid.";
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
                    ValidationErrorMessage = "No backup database was found on the FTP server.";
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
            ValidationErrorMessage = "FTP import failed. " + ex.Message;
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
            "Replace current database",
            "Importing replaces the current configuration and backup database. Do you want to continue?",
            [new UICommand("Yes"), new UICommand("No")],
            defaultCommandIndex: 0,
            cancelCommandIndex: 1);

        return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
    }

    private NewSetupConfiguration? BuildNewSetupConfiguration()
    {
        return SelectedTargetKind switch
        {
            SetupTargetKind.LocalDrive => new NewSetupConfiguration
            {
                SourceFolders = Sources.ToList(),
                TargetKind = SetupTargetKind.LocalDrive,
                LocalBackupFolder = string.IsNullOrWhiteSpace(SelectedDriveRoot)
                    ? null
                    : setupService.BuildLocalBackupFolder(SelectedDriveRoot),
                MediaVolumeSerial = ResolveVolumeSerial(SelectedDriveRoot),
                TaskType = SelectedTaskType
            },
            SetupTargetKind.Unc => new NewSetupConfiguration
            {
                SourceFolders = Sources.ToList(),
                TargetKind = SetupTargetKind.Unc,
                UncPath = UncPath,
                UncUsername = UncUsername,
                UncPassword = UncPassword,
                TaskType = SelectedTaskType
            },
            SetupTargetKind.Ftp => new NewSetupConfiguration
            {
                SourceFolders = Sources.ToList(),
                TargetKind = SetupTargetKind.Ftp,
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
        if (string.IsNullOrWhiteSpace(driveRoot) || driveRoot.Length < 1)
        {
            return "";
        }

        try
        {
            var serial = Brightbits.BSH.Engine.Win32Stuff.GetVolumeSerial(driveRoot[..1] + @":\");
            return string.IsNullOrEmpty(serial) || serial == "0" ? "" : serial;
        }
        catch
        {
            return "";
        }
    }

    private async Task<string?> PickFolderAsync()
    {
        try
        {
            if (App.MainWindow?.Content == null)
            {
                ValidationErrorMessage = "The main window is not ready for folder selection.";
                return null;
            }

            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = App.MainWindow.GetWindowHandle();
            if (hwnd == IntPtr.Zero)
            {
                ValidationErrorMessage = "Unable to open the folder picker.";
                return null;
            }

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            return folder?.Path;
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = "Unable to open the folder picker. " + ex.Message;
            return null;
        }
    }

    private void UpdateStepPresentation()
    {
        WelcomeVisibility = CurrentStep == SetupWizardStep.Welcome ? Visibility.Visible : Visibility.Collapsed;
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
            SetupWizardStep.Welcome => ("Welcome", "Create a new backup configuration or import an existing backup database."),
            SetupWizardStep.Sources => ("Select source folders", "Choose the folders that should be included in your backups."),
            SetupWizardStep.Target => ("Select backup target", "Choose a local drive, network path, or FTP server."),
            SetupWizardStep.Mode => ("Choose backup mode", "Run backups automatically or start them manually."),
            SetupWizardStep.ImportMedia => ("Import backup", "Select the media, FTP server, or path that contains your backup database."),
            SetupWizardStep.ImportSelect => ("Select backup", "Choose which discovered backup database to import."),
            SetupWizardStep.ImportRemap => ("Remap source folders", "Update source paths if folders moved after the original backup."),
            SetupWizardStep.Progress => ("Working", "Please wait while the setup is applied."),
            _ => ("Setup", "")
        };

        UpdateTargetVisibility();
        UpdateImportSourceVisibility();
    }

    private void UpdateTargetVisibility()
    {
        LocalTargetVisibility = SelectedTargetKind == SetupTargetKind.LocalDrive ? Visibility.Visible : Visibility.Collapsed;
        UncTargetVisibility = SelectedTargetKind == SetupTargetKind.Unc ? Visibility.Visible : Visibility.Collapsed;
        FtpTargetVisibility = SelectedTargetKind == SetupTargetKind.Ftp ? Visibility.Visible : Visibility.Collapsed;
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
    public required string DisplayName { get; init; }

    public required string RootPath { get; init; }

    public required DriveType DriveType { get; init; }
}

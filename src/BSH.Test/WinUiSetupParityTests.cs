// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using NUnit.Framework;
using Windows.UI.Popups;

namespace BSH.Test;

public class WinUiSetupParityTests
{
    [Test]
    public void DefaultSourceFolderUsesDocumentsWhenAvailable()
    {
        var setupService = CreateSetupService(new TestConfigurationManager());

        var defaultSource = setupService.GetDefaultSourceFolder();

        Assert.That(defaultSource, Is.EqualTo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
    }

    [Test]
    public void TryAddSourceFolderRejectsDuplicateFolderNames()
    {
        var setupService = CreateSetupService(new TestConfigurationManager());
        var sources = new List<string> { @"C:\Users\alex\Documents" };

        var added = setupService.TryAddSourceFolder(sources, @"D:\Archive\Documents", out var error);

        Assert.That(added, Is.False);
        Assert.That(error, Does.Contain("name").IgnoreCase);
        Assert.That(sources, Has.Count.EqualTo(1));
    }

    [Test]
    public void BuildLocalBackupFolderUsesMachineAndUser()
    {
        var setupService = CreateSetupService(new TestConfigurationManager());

        var folder = setupService.BuildLocalBackupFolder(@"E:\");

        Assert.That(folder, Is.EqualTo($@"E:\Backups\{Environment.MachineName}\{Environment.UserName}"));
    }

    [Test]
    public void LocalTargetIsRejectedWhenBackupFolderAlreadyExists()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var backupFolder = Path.Combine(root, "Backups", Environment.MachineName, Environment.UserName);
        Directory.CreateDirectory(backupFolder);

        try
        {
            var setupService = CreateSetupService(new TestConfigurationManager());

            Assert.That(setupService.IsLocalBackupFolderAvailable(backupFolder), Is.False);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public void ApplyNewLocalConfigurationPersistsSourcesTargetAndMode()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var setupService = CreateSetupService(configuration);
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var backupFolder = setupService.BuildLocalBackupFolder(root + Path.DirectorySeparatorChar);
            setupService.ApplyNewConfiguration(new NewSetupConfiguration
            {
                SourceFolders = [@"C:\Users\alex\Documents"],
                TargetKind = SetupTargetKind.LocalDrive,
                LocalBackupFolder = backupFolder,
                MediaVolumeSerial = "ABC123",
                TaskType = TaskType.Auto
            });

            Assert.That(configuration.IsConfigured, Is.EqualTo("1"));
            Assert.That(configuration.SourceFolder, Is.EqualTo(@"C:\Users\alex\Documents"));
            Assert.That(configuration.BackupFolder, Is.EqualTo(backupFolder));
            Assert.That(configuration.MediumType, Is.EqualTo(MediaType.LocalDevice));
            Assert.That(configuration.MediaVolumeSerial, Is.EqualTo("ABC123"));
            Assert.That(configuration.TaskType, Is.EqualTo(TaskType.Auto));
            Assert.That(configuration.Medium, Is.EqualTo("1"));
            Assert.That(Directory.Exists(backupFolder), Is.True);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public void ApplyNewFtpConfigurationPersistsFtpSettings()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var setupService = CreateSetupService(configuration);

        setupService.ApplyNewConfiguration(new NewSetupConfiguration
        {
            SourceFolders = [@"C:\Users\alex\Documents"],
            TargetKind = SetupTargetKind.Ftp,
            FtpHost = "ftp.example.test",
            FtpPort = "2121",
            FtpUser = "ftp-user",
            FtpPassword = "ftp-password",
            FtpFolder = "/backup",
            FtpEncoding = "UTF8",
            FtpEnforceUnencrypted = true,
            TaskType = TaskType.Manual
        });

        Assert.That(configuration.MediumType, Is.EqualTo(MediaType.FileTransferServer));
        Assert.That(configuration.FtpHost, Is.EqualTo("ftp.example.test"));
        Assert.That(configuration.FtpPort, Is.EqualTo("2121"));
        Assert.That(configuration.FtpUser, Is.EqualTo("ftp-user"));
        Assert.That(configuration.FtpPass, Is.EqualTo("ftp-password"));
        Assert.That(configuration.FtpFolder, Is.EqualTo("/backup"));
        Assert.That(configuration.FtpCoding, Is.EqualTo("UTF8"));
        Assert.That(configuration.FtpEncryptionMode, Is.EqualTo("0"));
        Assert.That(configuration.TaskType, Is.EqualTo(TaskType.Manual));
        Assert.That(configuration.IsConfigured, Is.EqualTo("1"));
    }

    [Test]
    public void ApplyNewFtpConfigurationPersistsEncryptedModeByDefault()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var setupService = CreateSetupService(configuration);

        setupService.ApplyNewConfiguration(new NewSetupConfiguration
        {
            SourceFolders = [@"C:\Users\alex\Documents"],
            TargetKind = SetupTargetKind.Ftp,
            FtpHost = "ftp.example.test",
            FtpPort = "21",
            FtpUser = "ftp-user",
            FtpPassword = "ftp-password",
            FtpFolder = "/backup",
            FtpEncoding = "UTF8",
            FtpEnforceUnencrypted = false,
            TaskType = TaskType.Manual
        });

        Assert.That(configuration.FtpEncryptionMode, Is.EqualTo("3"));
    }

    [Test]
    public void DiscoverBackupsFindsDatabaseUnderComputerUserLayout()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var backupFolder = Path.Combine(root, "Backups", "PC-ONE", "alex");
        Directory.CreateDirectory(backupFolder);
        File.WriteAllText(Path.Combine(backupFolder, "backup.bshdb"), "db");

        try
        {
            var setupService = CreateSetupService(new TestConfigurationManager());

            var discovered = setupService.DiscoverBackupsOnDrive(root);

            Assert.That(discovered, Has.Count.EqualTo(1));
            Assert.That(discovered[0].ComputerName, Is.EqualTo("PC-ONE"));
            Assert.That(discovered[0].UserName, Is.EqualTo("alex"));
            Assert.That(discovered[0].FolderPath, Is.EqualTo(backupFolder));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public void ExplicitPathImportDecisionRequiresBackupDatabase()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var setupService = CreateSetupService(new TestConfigurationManager());

            Assert.That(setupService.BackupDatabaseExists(root), Is.False);
            File.WriteAllText(Path.Combine(root, "backup.bshdb"), "db");
            Assert.That(setupService.BackupDatabaseExists(root), Is.True);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public void CanRemapSourceAllowsSameFolderNameOrDriveRootSwap()
    {
        var setupService = CreateSetupService(new TestConfigurationManager());

        Assert.That(setupService.CanRemapSourcePath(@"C:\Users\alex\Documents", @"D:\Users\alex\Documents", out _), Is.True);
        Assert.That(setupService.CanRemapSourcePath(@"C:\", @"D:\", out _), Is.True);
        Assert.That(setupService.CanRemapSourcePath(@"C:\Users\alex\Documents", @"D:\Users\alex\Pictures", out var error), Is.False);
        Assert.That(error, Does.Contain("change").IgnoreCase);
    }

    [Test]
    public void ApplySourceRemapsUpdatesMatchingPaths()
    {
        var remapped = SetupService.ApplySourceRemaps(
            @"C:\Users\alex\Documents|C:\Users\alex\Pictures",
            [
                new SourceRemap(@"C:\Users\alex\Documents", @"D:\Users\alex\Documents"),
                new SourceRemap(@"C:\Users\alex\Pictures", @"D:\Users\alex\Pictures")
            ]);

        Assert.That(remapped, Is.EqualTo(@"D:\Users\alex\Documents|D:\Users\alex\Pictures"));
    }

    [Test]
    public void ReplaceDatabaseDeletesSidecarsAndCopiesNewFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var destination = Path.Combine(root, "backupservicehome.bshdb");
        var source = Path.Combine(root, "import", "backup.bshdb");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        File.WriteAllText(destination, "old");
        File.WriteAllText(destination + "-wal", "wal");
        File.WriteAllText(destination + "-shm", "shm");
        File.WriteAllText(source, "imported");

        try
        {
            var setupService = CreateSetupService(new TestConfigurationManager());

            setupService.ReplaceDatabaseWithCopy(source, destination);

            Assert.That(File.ReadAllText(destination), Is.EqualTo("imported"));
            Assert.That(File.Exists(destination + "-wal"), Is.False);
            Assert.That(File.Exists(destination + "-shm"), Is.False);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public async Task CompletingAutomaticSetupStartsSystemAndCreatesFirstBackup()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var setupService = CreateSetupService(configuration);
        var orchestration = new TestOrchestrationService();
        var jobService = new TestJobService();
        var navigation = new TestNavigationService();
        var databaseFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bshdb");
        var viewModel = new SetupViewModel(
            configuration,
            setupService,
            orchestration,
            jobService,
            navigation,
            new TestPresentationService());
        viewModel.UseDatabaseFile(databaseFile);

        viewModel.BeginNewSetup();
        if (viewModel.Sources.Count == 0)
        {
            Assert.That(viewModel.TryAddSourceFolder(@"C:\Users\alex\Documents"), Is.True);
        }

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            viewModel.SelectedTargetKind = SetupTargetKind.LocalDrive;
            viewModel.SelectedDriveRoot = root + Path.DirectorySeparatorChar;
            viewModel.SelectedTaskType = TaskType.Auto;

            var completed = await viewModel.FinishNewSetupAsync();

            Assert.That(completed, Is.True);
            Assert.That(configuration.IsConfigured, Is.EqualTo("1"));
            Assert.That(orchestration.StartCalls, Is.EqualTo(1));
            Assert.That(orchestration.LastStartTurnOn, Is.True);
            Assert.That(jobService.CreateBackupCalls, Is.EqualTo(1));
            Assert.That(navigation.LastPageKey, Does.Contain("MainViewModel"));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public async Task CompletingManualSetupNavigatesToSettingsWithoutStartingBackup()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var setupService = CreateSetupService(configuration);
        var orchestration = new TestOrchestrationService();
        var jobService = new TestJobService();
        var navigation = new TestNavigationService();
        var databaseFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bshdb");
        var viewModel = new SetupViewModel(
            configuration,
            setupService,
            orchestration,
            jobService,
            navigation,
            new TestPresentationService());
        viewModel.UseDatabaseFile(databaseFile);

        viewModel.BeginNewSetup();
        if (viewModel.Sources.Count == 0)
        {
            Assert.That(viewModel.TryAddSourceFolder(@"C:\Users\alex\Documents"), Is.True);
        }

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            viewModel.SelectedTargetKind = SetupTargetKind.LocalDrive;
            viewModel.SelectedDriveRoot = root + Path.DirectorySeparatorChar;
            viewModel.SelectedTaskType = TaskType.Manual;

            var completed = await viewModel.FinishNewSetupAsync();

            Assert.That(completed, Is.True);
            Assert.That(orchestration.StartCalls, Is.EqualTo(0));
            Assert.That(jobService.CreateBackupCalls, Is.EqualTo(0));
            Assert.That(navigation.LastPageKey, Does.Contain("SettingsViewModel"));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public async Task CompletingImportRemapsSourcesUpdatesConfigurationAndStartsSystem()
    {
        var configuration = new TestConfigurationManager
        {
            IsConfigured = "1",
            SourceFolder = @"C:\Users\old\Documents"
        };
        var queryManager = new TestQueryManager
        {
            Versions =
            [
                new VersionDetails
                {
                    Id = "1",
                    Sources = @"C:\Users\old\Documents"
                }
            ]
        };
        var dbClientFactory = new TestDbClientFactory();
        var setupService = new SetupService(configuration, queryManager, dbClientFactory);
        var orchestration = new TestOrchestrationService();
        var jobService = new TestJobService();
        var navigation = new TestNavigationService();
        var databaseFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bshdb");
        var viewModel = new SetupViewModel(
            configuration,
            setupService,
            orchestration,
            jobService,
            navigation,
            new TestPresentationService(),
            dbClientFactory: dbClientFactory);
        viewModel.UseDatabaseFile(databaseFile);

        viewModel.BeginImport();
        viewModel.LoadSourceRemaps([@"C:\Users\old\Documents"]);
        Assert.That(viewModel.TryUpdateSourceRemap(0, @"D:\Users\alex\Documents"), Is.True);

        var completed = await viewModel.FinishImportAsync();

        Assert.That(completed, Is.True);
        Assert.That(configuration.SourceFolder, Is.EqualTo(@"D:\Users\alex\Documents"));
        Assert.That(configuration.TaskType, Is.EqualTo(TaskType.Manual));
        Assert.That(dbClientFactory.ExecutedQueries.Any(q => q.Contains("versionSources")), Is.True);
        Assert.That(orchestration.StartCalls, Is.EqualTo(1));
        Assert.That(orchestration.LastStartTurnOn, Is.True);
        Assert.That(navigation.LastPageKey, Does.Contain("MainViewModel"));
    }

    [Test]
    public void DefaultActivationRoutesToSetupWhenNotConfigured()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var navigation = new TestNavigationService();
        var routing = new SetupRouting(configuration, navigation);

        routing.NavigateForStartup();

        Assert.That(navigation.LastPageKey, Does.Contain("SetupViewModel"));
        Assert.That(navigation.LastClearNavigation, Is.True);
    }

    [Test]
    public void NavigateToSetupForcesFreshNavigationParameter()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var navigation = new TestNavigationService();
        var routing = new SetupRouting(configuration, navigation);

        routing.NavigateToSetup();
        var firstParameter = navigation.LastParameter;
        routing.NavigateToSetup();

        Assert.That(navigation.LastPageKey, Does.Contain("SetupViewModel"));
        Assert.That(navigation.LastParameter, Is.Not.Null);
        Assert.That(navigation.LastParameter, Is.Not.EqualTo(firstParameter));
    }

    [Test]
    public void BeginNewSetupAddsDocumentsAsDefaultSource()
    {
        var configuration = new TestConfigurationManager { IsConfigured = "0" };
        var setupService = CreateSetupService(configuration);
        var viewModel = new SetupViewModel(
            configuration,
            setupService,
            new TestOrchestrationService(),
            new TestJobService(),
            new TestNavigationService(),
            new TestPresentationService());

        viewModel.BeginNewSetup();

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (documents.Length > 2)
        {
            Assert.That(viewModel.Sources, Does.Contain(documents));
        }
        else
        {
            Assert.That(viewModel.Sources, Is.Empty);
        }
    }

    private static SetupService CreateSetupService(IConfigurationManager configuration)
    {
        return new SetupService(configuration, new TestQueryManager(), new TestDbClientFactory());
    }

    private static void DeleteDirectory(string root)
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }
    }

    private sealed class TestConfigurationManager : IConfigurationManager
    {
        public string AutoBackup { get; set; } = "";
        public string BackupFolder { get; set; } = "";
        public string BackupSize { get; set; } = "";
        public int Compression { get; set; }
        public string DbStatus { get; set; } = "";
        public string DBVersion { get; set; } = "";
        public string DeativateAutoBackupsWhenAkku { get; set; } = "1";
        public string DoPastBackups { get; set; } = "";
        public int Encrypt { get; set; }
        public string EncryptPassMD5 { get; set; } = "";
        public string ExcludeCompression { get; set; } = ".zip|.rar|.7zip";
        public string ExcludeFile { get; set; } = "";
        public string ExcludeFileBigger { get; set; } = "";
        public string ExcludeFileTypes { get; set; } = "";
        public string ExcludeFolder { get; set; } = "";
        public string ExcludeMask { get; set; } = "";
        public string FreeSpace { get; set; } = "";
        public string FtpCoding { get; set; } = "ISO-8859-1";
        public string FtpEncryptionMode { get; set; } = "";
        public string FtpFolder { get; set; } = "";
        public string FtpHost { get; set; } = "";
        public string FtpPass { get; set; } = "";
        public string FtpPort { get; set; } = "21";
        public string FtpSslProtocols { get; set; } = "";
        public string FtpUser { get; set; } = "";
        public string InfoBackupDone { get; set; } = "";
        public string IntervallAutoHourBackups { get; set; } = "";
        public string IntervallDelete { get; set; } = "";
        public string IsConfigured { get; set; } = "";
        public string LastBackupDone { get; set; } = "";
        public string LastVersionDate { get; set; } = "";
        public string MediaVolumeSerial { get; set; } = "";
        public string Medium { get; set; } = "";
        public MediaType MediumType { get; set; } = MediaType.LocalDevice;
        public string OldBackupPrevent { get; set; } = "";
        public string RemindAfterDays { get; set; } = "7";
        public string RemindSpace { get; set; } = "-1";
        public string ScheduleFullBackup { get; set; } = "";
        public string ShowLocalizedPath { get; set; } = "1";
        public string ShowWaitOnMediaAutoBackups { get; set; } = "";
        public string SourceFolder { get; set; } = "";
        public TaskType TaskType { get; set; } = TaskType.Auto;
        public string UNCPassword { get; set; } = "";
        public string UNCUsername { get; set; } = "";

        public Task InitializeAsync() => Task.CompletedTask;
    }

    private sealed class TestQueryManager : IQueryManager
    {
        public List<VersionDetails> Versions { get; set; } = [];

        public List<VersionDetails> GetVersions(bool desc = true) => Versions;

        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public string GetFileNameFromDrive(FileTableRow file) => "";
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult(("", false));
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => throw new NotImplementedException();
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult("");
        public Task<VersionDetails> GetLastBackupAsync() => throw new NotImplementedException();
        public Task<VersionDetails> GetLastFullBackupAsync() => throw new NotImplementedException();
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(0);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => throw new NotImplementedException();
        public Task<VersionDetails> GetVersionByIdAsync(string id) => throw new NotImplementedException();
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }

    private sealed class TestDbClientFactory : IDbClientFactory
    {
        public List<string> ExecutedQueries { get; } = [];

        public string DatabaseFile => "test.bshdb";

        public Brightbits.BSH.Engine.Database.DbClient CreateDbClient() => throw new NotSupportedException();

        public Task InitializeAsync(string databaseFile) => Task.CompletedTask;

        public Task ExecuteNonQueryAsync(string query)
        {
            ExecutedQueries.Add(query);
            return Task.CompletedTask;
        }
    }

    private sealed class TestOrchestrationService : IOrchestrationService
    {
        public int StartCalls { get; private set; }
        public bool LastStartTurnOn { get; private set; }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task StartAsync(bool turnOn = false)
        {
            StartCalls++;
            LastStartTurnOn = turnOn;
            return Task.CompletedTask;
        }

        public Task StopAsync(bool turnOff = false) => Task.CompletedTask;
    }

    private sealed class TestJobService : IJobService
    {
        public int CreateBackupCalls { get; private set; }

        public bool IsCancellationRequested => false;

        public void Cancel()
        {
        }

        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(true);

        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "")
        {
            CreateBackupCalls++;
            return Task.FromResult(true);
        }

        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true) => Task.CompletedTask;
        public CancellationToken GetNewCancellationToken() => CancellationToken.None;
        public Task<bool> RequestPassword() => Task.FromResult(true);
        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
    }

    private sealed class TestNavigationService : INavigationService
    {
        public string? LastPageKey { get; private set; }
        public bool LastClearNavigation { get; private set; }
        public object? LastParameter { get; private set; }

        public Frame? Frame { get; set; }
        public bool CanGoBack => false;

        public event NavigatedEventHandler? Navigated;

        public bool GoBack() => false;

        public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
        {
            LastPageKey = pageKey;
            LastClearNavigation = clearNavigation;
            LastParameter = parameter;
            return true;
        }
    }

    private sealed class TestPresentationService : IPresentationService
    {
        public Task CloseBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task CloseMainWindowAsync() => Task.CompletedTask;
        public Task OpenCurrentEventLogAsync() => Task.CompletedTask;
        public Task OpenHelpSupportAsync() => Task.CompletedTask;
        public Task<TaskCompleteAction> CloseStatusWindowAsync() => Task.FromResult(TaskCompleteAction.NoAction);
        public Task<(string? password, bool persist)> RequestPasswordAsync() => Task.FromResult<(string?, bool)>(("password", false));
        public Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.Overwrite);
        public Task ResetConfigurationAsync() => Task.CompletedTask;
        public Task ShowAboutWindowAsync() => Task.CompletedTask;
        public Task ShowBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => throw new NotImplementedException();
        public Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => throw new NotImplementedException();
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(true);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.Primary);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;
using BSH.MainApp.ViewModels.Windows;
using BSH.Test.Fakes;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Windows.UI.Popups;

namespace BSH.Test;

[TestFixture]
public class SettingsViewModelTests
{
    [Test]
    public void FtpFields_PersistAndReloadAcrossNavigation()
    {
        var configuration = CreateFtpConfiguration();
        var harness = CreateHarness(configuration);
        harness.ViewModel.OnNavigatedTo(null);

        harness.ViewModel.FtpRemoteHost = "ftp.page.example";
        harness.ViewModel.FtpRemotePort = 2121;
        harness.ViewModel.FtpRemoteUser = "page-user";
        harness.ViewModel.FtpRemotePassword = "page-secret";
        harness.ViewModel.FtpRemotePath = @"backups\home";
        harness.ViewModel.OnNavigatedFrom();

        Assert.Multiple(() =>
        {
            Assert.That(configuration.FtpHost, Is.EqualTo("ftp.page.example"));
            Assert.That(configuration.FtpPort, Is.EqualTo("2121"));
            Assert.That(configuration.FtpUser, Is.EqualTo("page-user"));
            Assert.That(configuration.FtpPass, Is.EqualTo("page-secret"));
            Assert.That(configuration.FtpFolder, Is.EqualTo(FtpStorage.GetFtpPath(@"backups\home")));
        });

        var reloaded = CreateHarness(configuration).ViewModel;
        reloaded.OnNavigatedTo(null);

        Assert.Multiple(() =>
        {
            Assert.That(reloaded.FtpRemoteHost, Is.EqualTo("ftp.page.example"));
            Assert.That(reloaded.FtpRemotePort, Is.EqualTo(2121));
            Assert.That(reloaded.FtpRemoteUser, Is.EqualTo("page-user"));
            Assert.That(reloaded.FtpRemotePassword, Is.EqualTo("page-secret"));
            Assert.That(reloaded.FtpRemotePath, Is.EqualTo(FtpStorage.GetFtpPath(@"backups\home")));
        });
    }

    [Test]
    public void FtpTestConnection_UsesDisplayedFields_NotStaleStoredCredentials()
    {
        var configuration = CreateFtpConfiguration();
        var harness = CreateHarness(configuration);
        harness.ViewModel.OnNavigatedTo(null);

        harness.ViewModel.FtpRemoteHost = "ftp.page.example";
        harness.ViewModel.FtpRemotePort = 2121;
        harness.ViewModel.FtpRemoteUser = "page-user";
        harness.ViewModel.FtpRemotePassword = "page-secret";
        harness.ViewModel.FtpRemotePath = "/page-path";
        harness.ViewModel.FtpRemoteEncoding = "ISO-8859-1";

        configuration.FtpHost = "ftp.stale.example";
        configuration.FtpPort = "21";
        configuration.FtpUser = "stale-user";
        configuration.FtpPass = "stale-secret";
        configuration.FtpFolder = "/stale-path";
        configuration.FtpCoding = "UTF8";

        var probe = harness.ViewModel.GetDisplayedFtpConnectionValues();

        Assert.Multiple(() =>
        {
            Assert.That(probe.Host, Is.EqualTo("ftp.page.example"));
            Assert.That(probe.Port, Is.EqualTo(2121));
            Assert.That(probe.User, Is.EqualTo("page-user"));
            Assert.That(probe.Password, Is.EqualTo("page-secret"));
            Assert.That(probe.Path, Is.EqualTo("/page-path"));
            Assert.That(probe.Encoding, Is.EqualTo("ISO-8859-1"));
            Assert.That(configuration.FtpHost, Is.EqualTo("ftp.stale.example"));
        });
    }

    [Test]
    public void Options_WhenEncryptAndCompressionAreBothSet_ShowsEncryption()
    {
        var configuration = new FakeConfigurationManager
        {
            Encrypt = 1,
            Compression = 1
        };
        var harness = CreateHarness(configuration);
        harness.ViewModel.OnNavigatedTo(null);

        Assert.That(harness.ViewModel.ModeType, Is.EqualTo(ModeType.Encryption));
        Assert.That(harness.ViewModel.CanSelectNonEncryptionMode, Is.False);
    }

    [Test]
    public async Task RegularOrCompress_WhileEncrypted_DoesNotClearEncryptWithoutDecryptJob()
    {
        var configuration = new FakeConfigurationManager { Encrypt = 1, Compression = 0 };
        var harness = CreateHarness(configuration);
        harness.JobService.RequestPasswordResult = false;
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.ChangeModeTypeAsync(ModeType.RegularCopy);
        await harness.ViewModel.ChangeModeTypeAsync(ModeType.Compression);

        Assert.Multiple(() =>
        {
            Assert.That(configuration.Encrypt, Is.EqualTo(1));
            Assert.That(configuration.Compression, Is.EqualTo(0));
            Assert.That(harness.ViewModel.ModeType, Is.EqualTo(ModeType.Encryption));
            Assert.That(harness.JobService.ModifyBackupCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task TurningEncryptionOff_OnLocal_RunsMediaCheckPasswordAndDecryptJob()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.LocalDevice,
            Encrypt = 1,
            EncryptPassMD5 = "existing-hash"
        };
        var harness = CreateHarness(configuration);
        harness.JobService.ClearEncryptionOnModify = true;
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.DisableEncryptionCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(harness.JobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Restore }));
            Assert.That(harness.JobService.RequestPasswordCallCount, Is.EqualTo(1));
            Assert.That(harness.JobService.ModifyBackupCalls, Is.EqualTo(1));
            Assert.That(harness.JobService.DeleteBackupsCalls, Is.Empty);
            Assert.That(configuration.Encrypt, Is.EqualTo(0));
            Assert.That(harness.ViewModel.ModeType, Is.EqualTo(ModeType.RegularCopy));
        });
    }

    [Test]
    public async Task TurningEncryptionOff_OnFtp_DeletesAllBackupsWithoutDecryptJob()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.FileTransferServer,
            Encrypt = 1,
            EncryptPassMD5 = "existing-hash",
            FtpHost = "ftp.example.com",
            FtpEncryptionMode = "3"
        };
        var harness = CreateHarness(configuration);
        harness.QueryManager.Versions =
        [
            new VersionDetails { Id = "1" },
            new VersionDetails { Id = "2" }
        ];
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.DisableEncryptionCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(harness.JobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Delete }));
            Assert.That(harness.JobService.RequestPasswordCallCount, Is.EqualTo(0));
            Assert.That(harness.JobService.ModifyBackupCalls, Is.EqualTo(0));
            Assert.That(harness.JobService.DeleteBackupsCalls, Has.Count.EqualTo(1));
            Assert.That(harness.JobService.DeleteBackupsCalls[0], Is.EqualTo(new[] { "1", "2" }));
            Assert.That(configuration.Encrypt, Is.EqualTo(0));
            Assert.That(configuration.EncryptPassMD5, Is.EqualTo(""));
            Assert.That(harness.ViewModel.ModeType, Is.EqualTo(ModeType.RegularCopy));
        });
    }

    [Test]
    public async Task TurningEncryptionOff_OnFtp_WhenMediaCheckFails_KeepsEncryption()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.FileTransferServer,
            Encrypt = 1,
            EncryptPassMD5 = "existing-hash"
        };
        var harness = CreateHarness(configuration);
        harness.QueryManager.Versions = [new VersionDetails { Id = "1" }];
        harness.JobService.CheckMediaResult = false;
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.DisableEncryptionCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(harness.JobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Delete }));
            Assert.That(harness.JobService.DeleteBackupsCalls, Is.Empty);
            Assert.That(harness.JobService.ModifyBackupCalls, Is.EqualTo(0));
            Assert.That(configuration.Encrypt, Is.EqualTo(1));
            Assert.That(configuration.EncryptPassMD5, Is.EqualTo("existing-hash"));
            Assert.That(harness.ViewModel.ModeType, Is.EqualTo(ModeType.Encryption));
        });
    }

    [Test]
    public async Task EnforcingUnencryptedFtp_DeletesAllBackups()
    {
        var configuration = CreateFtpConfiguration();
        var harness = CreateHarness(configuration);
        harness.QueryManager.Versions =
        [
            new VersionDetails { Id = "10" },
            new VersionDetails { Id = "11" }
        ];
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.ChangeFtpRemoteEnforceUnencryptedAsync(true);

        Assert.Multiple(() =>
        {
            Assert.That(harness.JobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Delete }));
            Assert.That(harness.JobService.DeleteBackupsCalls, Has.Count.EqualTo(1));
            Assert.That(harness.JobService.DeleteBackupsCalls[0], Is.EqualTo(new[] { "10", "11" }));
            Assert.That(harness.JobService.ModifyBackupCalls, Is.EqualTo(0));
            Assert.That(configuration.FtpEncryptionMode, Is.EqualTo("0"));
            Assert.That(harness.ViewModel.FtpRemoteEnforceUnencrypted, Is.True);
        });
    }

    [Test]
    public async Task EnforcingUnencryptedFtp_WhenMediaCheckFails_DoesNotDeleteOrPersist()
    {
        var configuration = CreateFtpConfiguration();
        var harness = CreateHarness(configuration);
        harness.QueryManager.Versions = [new VersionDetails { Id = "10" }];
        harness.JobService.CheckMediaResult = false;
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.ChangeFtpRemoteEnforceUnencryptedAsync(true);

        Assert.Multiple(() =>
        {
            Assert.That(harness.JobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Delete }));
            Assert.That(harness.JobService.DeleteBackupsCalls, Is.Empty);
            Assert.That(configuration.FtpEncryptionMode, Is.EqualTo("3"));
            Assert.That(harness.ViewModel.FtpRemoteEnforceUnencrypted, Is.False);
        });
    }

    [Test]
    public async Task TurningEncryptionOn_ClearsCompression_AndShowsEncryption()
    {
        var configuration = new FakeConfigurationManager { Compression = 1, Encrypt = 0 };
        var harness = CreateHarness(configuration);
        harness.PresentationService.Password = "secret";
        harness.ViewModel.OnNavigatedTo(null);

        await harness.ViewModel.ChangeModeTypeAsync(ModeType.Encryption);

        Assert.Multiple(() =>
        {
            Assert.That(configuration.Encrypt, Is.EqualTo(1));
            Assert.That(configuration.Compression, Is.EqualTo(0));
            Assert.That(configuration.EncryptPassMD5, Is.EqualTo(Hash.GetMD5Hash("secret")));
            Assert.That(harness.ViewModel.ModeType, Is.EqualTo(ModeType.Encryption));
        });
    }

    [Test]
    public async Task SwitchingLocalToFtp_AppliesFtpTargetAndClearsLocalPath()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.LocalDevice,
            BackupFolder = @"D:\Backups",
            MediaVolumeSerial = "ABCDEF12",
            FtpHost = "",
            FtpPort = "",
            FtpUser = "",
            FtpPass = "",
            FtpFolder = ""
        };
        var harness = CreateHarness(configuration);
        harness.QueryManager.Versions =
        [
            new VersionDetails { Id = "1" },
            new VersionDetails { Id = "2" }
        ];
        harness.ViewModel.OnNavigatedTo(null);

        harness.ViewModel.FtpRemoteHost = "ftp.example.com";
        harness.ViewModel.FtpRemotePort = 21;
        harness.ViewModel.FtpRemoteUser = "ftp-user";
        harness.ViewModel.FtpRemotePassword = "ftp-pass";
        harness.ViewModel.FtpRemotePath = "/backups";

        await harness.ViewModel.ChangeSelectedMediaTypeAsync(MediaType.FileTransferServer);

        Assert.Multiple(() =>
        {
            Assert.That(configuration.MediumType, Is.EqualTo(MediaType.FileTransferServer));
            Assert.That(configuration.BackupFolder, Is.EqualTo(""));
            Assert.That(configuration.MediaVolumeSerial, Is.EqualTo(""));
            Assert.That(configuration.FtpHost, Is.EqualTo("ftp.example.com"));
            Assert.That(configuration.FtpUser, Is.EqualTo("ftp-user"));
            Assert.That(harness.JobService.DeleteBackupsCalls, Has.Count.EqualTo(1));
            Assert.That(harness.JobService.DeleteBackupsCalls[0], Is.EqualTo(new[] { "1", "2" }));
        });
    }

    [Test]
    public async Task SwitchingFtpToLocal_AppliesLocalTargetAndClearsFtpFields()
    {
        var configuration = new FakeConfigurationManager
        {
            MediumType = MediaType.FileTransferServer,
            BackupFolder = "",
            FtpHost = "ftp.old",
            FtpPort = "21",
            FtpUser = "ftp-user",
            FtpPass = "ftp-pass",
            FtpFolder = "/old",
            FtpCoding = "UTF8",
            FtpEncryptionMode = "3",
            FtpSslProtocols = "0"
        };
        var harness = CreateHarness(configuration);
        harness.ViewModel.OnNavigatedTo(null);
        harness.ViewModel.LocalDevicePath = @"D:\Backups";

        await harness.ViewModel.ChangeSelectedMediaTypeAsync(MediaType.LocalDevice);

        Assert.Multiple(() =>
        {
            Assert.That(configuration.MediumType, Is.EqualTo(MediaType.LocalDevice));
            Assert.That(configuration.BackupFolder, Is.EqualTo(@"D:\Backups"));
            Assert.That(configuration.FtpHost, Is.EqualTo(""));
            Assert.That(configuration.FtpUser, Is.EqualTo(""));
            Assert.That(configuration.FtpPass, Is.EqualTo(""));
            Assert.That(configuration.FtpFolder, Is.EqualTo(""));
            Assert.That(configuration.FtpCoding, Is.EqualTo(""));
            Assert.That(configuration.FtpEncryptionMode, Is.EqualTo(""));
        });
    }

    [Test]
    public void OnNavigatedFrom_WhenFtpTarget_ClearsLeftoverLocalPath()
    {
        var configuration = CreateFtpConfiguration();
        configuration.BackupFolder = @"D:\Leftover";
        configuration.MediaVolumeSerial = "ABCDEF12";
        var harness = CreateHarness(configuration);
        harness.ViewModel.OnNavigatedTo(null);
        harness.ViewModel.OnNavigatedFrom();

        Assert.Multiple(() =>
        {
            Assert.That(configuration.MediumType, Is.EqualTo(MediaType.FileTransferServer));
            Assert.That(configuration.BackupFolder, Is.EqualTo(""));
            Assert.That(configuration.MediaVolumeSerial, Is.EqualTo(""));
            Assert.That(configuration.FtpHost, Is.EqualTo("ftp.stored.example"));
        });
    }

    private static FakeConfigurationManager CreateFtpConfiguration() => new()
    {
        MediumType = MediaType.FileTransferServer,
        FtpHost = "ftp.stored.example",
        FtpPort = "21",
        FtpUser = "stored-user",
        FtpPass = "stored-secret",
        FtpFolder = "/stored",
        FtpCoding = "UTF8",
        FtpEncryptionMode = "3"
    };

    private static SettingsHarness CreateHarness(FakeConfigurationManager configuration)
    {
        var presentation = new SettingsPresentationService();
        var jobService = new SettingsJobService(configuration);
        var queryManager = new SettingsQueryManager();
        var viewModel = new SettingsViewModel(
            configuration,
            presentation,
            jobService,
            queryManager,
            new SettingsBackupTargetService(),
            new SettingsSwitchStorageService(),
            new SettingsOrchestrationService(),
            new SettingsStartupLaunchAdapter(),
            new SettingsUpdateService());

        return new SettingsHarness(viewModel, presentation, jobService, queryManager);
    }

    private sealed record SettingsHarness(
        SettingsViewModel ViewModel,
        SettingsPresentationService PresentationService,
        SettingsJobService JobService,
        SettingsQueryManager QueryManager);

    private sealed class SettingsPresentationService : IPresentationService
    {
        public ContentDialogResult MessageBoxResult { get; set; } = ContentDialogResult.Primary;
        public string? Password { get; set; }

        public Task CloseBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task CloseMainWindowAsync() => Task.CompletedTask;
        public Task<TaskCompleteAction> CloseStatusWindowAsync() => Task.FromResult(TaskCompleteAction.NoAction);
        public Task OpenCurrentEventLogAsync() => Task.CompletedTask;
        public Task OpenHelpSupportAsync() => Task.CompletedTask;
        public Task<(string? password, bool persist)> RequestPasswordAsync() => Task.FromResult((Password, false));
        public Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);
        public Task ResetConfigurationAsync() => Task.CompletedTask;
        public Task ShowAboutWindowAsync() => Task.CompletedTask;
        public Task ShowBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task ShowCompressionExclusionsWindowAsync() => Task.CompletedTask;
        public Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new NewBackupViewModel()));
        public Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(MessageBoxResult);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
        public Task<bool> ShowSwitchStorageWindowAsync() => Task.FromResult(false);
    }

    private sealed class SettingsJobService : IJobService
    {
        private readonly FakeConfigurationManager configuration;

        public SettingsJobService(FakeConfigurationManager configuration)
        {
            this.configuration = configuration;
        }

        public List<ActionType> CheckMediaCalls { get; } = [];
        public List<List<string>> DeleteBackupsCalls { get; } = [];
        public int RequestPasswordCallCount { get; private set; }
        public int ModifyBackupCalls { get; private set; }
        public bool CheckMediaResult { get; set; } = true;
        public bool RequestPasswordResult { get; set; } = true;
        public bool ClearEncryptionOnModify { get; set; }

        public bool IsCancellationRequested => false;
        public void Cancel() { }
        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
        {
            CheckMediaCalls.Add(action);
            return Task.FromResult(CheckMediaResult);
        }

        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "") => Task.FromResult(true);
        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true)
        {
            DeleteBackupsCalls.Add(versions);
            return Task.CompletedTask;
        }

        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true, IReadOnlyList<int>? versionIds = null) => Task.CompletedTask;
        public CancellationToken GetNewCancellationToken() => CancellationToken.None;
        public Task<bool> RequestPassword()
        {
            RequestPasswordCallCount++;
            return Task.FromResult(RequestPasswordResult);
        }

        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true)
        {
            ModifyBackupCalls++;
            if (ClearEncryptionOnModify)
            {
                configuration.Encrypt = 0;
                configuration.EncryptPassMD5 = "";
            }

            return Task.CompletedTask;
        }
    }

    private sealed class SettingsQueryManager : IQueryManager
    {
        public List<VersionDetails> Versions { get; set; } = [];

        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public string GetFileNameFromDrive(FileTableRow file) => "";
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult(("", false));
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(new FileDetails());
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult("");
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(Versions.Count);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult<VersionDetails>(null);
        public List<VersionDetails> GetVersions(bool desc = true) => Versions;
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }

    private sealed class SettingsBackupTargetService : IBackupTargetService
    {
        public Task<BackupTargetMoveResult> MoveExistingBackupDataAsync(string currentFolderPath, string newFolderPath) => Task.FromResult(BackupTargetMoveResult.Succeeded);
    }

    private sealed class SettingsSwitchStorageService : ISwitchStorageService
    {
        public bool LocalTargetContainsBackupData(string driveRoot) => false;
        public bool UncTargetContainsBackupData(string uncPath) => false;
        public void SyncDatabaseToCurrentMedium(string databaseFile) { }
        public Task SwitchToLocalAsync(string driveRoot, string? mediaVolumeSerial, string databaseFile) => Task.CompletedTask;
        public Task SwitchToUncAsync(SwitchStorageUncTarget unc, string databaseFile) => Task.CompletedTask;
        public Task SwitchToFtpAsync(SwitchStorageFtpTarget ftp, string databaseFile) => Task.CompletedTask;
    }

    private sealed class SettingsOrchestrationService : IOrchestrationService
    {
        public Task InitializeAsync() => Task.CompletedTask;
        public Task StartAsync(bool turnOn = false) => Task.CompletedTask;
        public Task StopAsync(bool turnOff = false) => Task.CompletedTask;
        public Task RefreshAutomationAsync() => Task.CompletedTask;
    }

    private sealed class SettingsStartupLaunchAdapter : IStartupLaunchAdapter
    {
        public bool IsEnabled() => false;
        public bool TrySetEnabled(bool enabled) => true;
    }

    private sealed class SettingsUpdateService : IUpdateService
    {
        public Task InitializeAsync(Action onApplicationExitRequested) => Task.CompletedTask;
        public Task CheckAsync(bool notifyWhenUpToDate) => Task.CompletedTask;
        public Task MaybeCheckOnStartupAsync() => Task.CompletedTask;
        public Task<bool> GetAutoSearchEnabledAsync() => Task.FromResult(true);
        public Task SetAutoSearchEnabledAsync(bool enabled) => Task.CompletedTask;
        public Task<bool> GetDownloadBetaAsync() => Task.FromResult(false);
        public Task SetDownloadBetaAsync(bool enabled) => Task.CompletedTask;
        public Task<string> ResetUniqueUserIdAsync() => Task.FromResult("");
    }
}

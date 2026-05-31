// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Security;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Windows.UI.Popups;

namespace BSH.Test;

public class WinUiSettingsParityTests
{
    [Test]
    public void FilterSettingsPersistMaxFileSizeAndRejectInvalidRegex()
    {
        var configuration = new TestConfigurationManager
        {
            SourceFolder = @"C:\Users\alex\Documents",
            ExcludeFileBigger = "2048"
        };
        var viewModel = new FilterViewModel(configuration);

        Assert.That(viewModel.ExcludeFilesBigger, Is.True);
        Assert.That(viewModel.ExcludeFileBigger, Is.EqualTo(2048));

        viewModel.AddRegexCommand.Execute("[broken");

        Assert.That(viewModel.RegexPatterns, Is.Empty);
        Assert.That(viewModel.ValidationErrorMessage, Does.Contain("regular expression"));

        viewModel.ExcludeFilesBigger = false;
        viewModel.SaveToConfiguration();

        Assert.That(configuration.ExcludeFileBigger, Is.Empty);
    }

    [Test]
    public void FilterSettingsPersistCompressionExclusions()
    {
        var configuration = new TestConfigurationManager
        {
            ExcludeCompression = ".zip|rar"
        };
        var viewModel = CreateSettingsViewModel(configuration);

        viewModel.OnNavigatedTo(null!);
        viewModel.AddCompressionExclusion("7z");
        viewModel.SelectedCompressionExclusion = ".zip";
        viewModel.RemoveCompressionExclusionCommand.Execute(null);

        Assert.That(viewModel.CompressionExclusions, Is.EquivalentTo(new[] { ".rar", ".7z" }));
        Assert.That(configuration.ExcludeCompression, Is.EqualTo(".rar|.7z"));
    }

    [Test]
    public void SourceManagementRejectsDuplicateNamesAndRiskyRoots()
    {
        var configuration = new TestConfigurationManager
        {
            SourceFolder = @"C:\Users\alex\Documents"
        };
        var viewModel = CreateSettingsViewModel(configuration);

        viewModel.OnNavigatedTo(null!);

        Assert.That(viewModel.TryAddSourceFolderPath(@"D:\Archive\Documents"), Is.False);
        Assert.That(viewModel.SourceValidationErrorMessage, Does.Contain("name"));
        Assert.That(viewModel.TryAddSourceFolderPath(@"C:\"), Is.False);
        Assert.That(viewModel.SourceValidationErrorMessage, Does.Contain("drive root"));
        Assert.That(configuration.SourceFolder, Is.EqualTo(@"C:\Users\alex\Documents"));
    }

    [Test]
    public async Task MovingLocalTargetMovesBackupDataAndPersistsNewPath()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var oldPath = Path.Combine(root, "old");
        var newPath = Path.Combine(root, "new");
        Directory.CreateDirectory(oldPath);
        File.WriteAllText(Path.Combine(oldPath, "backup.db"), "data");

        try
        {
            var configuration = new TestConfigurationManager
            {
                BackupFolder = oldPath
            };
            var viewModel = CreateSettingsViewModel(configuration);

            await viewModel.MoveExistingLocalBackupDataAsync(newPath);

            Assert.That(File.Exists(Path.Combine(newPath, "backup.db")), Is.True);
            Assert.That(Directory.Exists(oldPath), Is.False);
            Assert.That(configuration.BackupFolder, Is.EqualTo(newPath));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Test]
    public void TargetSettingsPersistFtpAndUncValues()
    {
        var configuration = new TestConfigurationManager();
        var viewModel = CreateSettingsViewModel(configuration);

        viewModel.OnNavigatedTo(null!);

        viewModel.LocalUNCUser = "user";
        viewModel.LocalUNCPassword = "password";
        viewModel.FtpRemoteHost = "ftp.example.test";
        viewModel.FtpRemotePort = 2121;
        viewModel.FtpRemoteUser = "ftp-user";
        viewModel.FtpRemotePassword = "ftp-password";
        viewModel.FtpRemotePath = "/backup";
        viewModel.FtpRemoteEncoding = "UTF8";
        viewModel.FtpRemoteEnforceUnencrypted = true;

        Assert.That(configuration.UNCUsername, Is.EqualTo("user"));
        Assert.That(Crypto.DecryptString(configuration.UNCPassword, System.Security.Cryptography.DataProtectionScope.LocalMachine), Is.EqualTo("password"));
        Assert.That(configuration.FtpHost, Is.EqualTo("ftp.example.test"));
        Assert.That(configuration.FtpPort, Is.EqualTo("2121"));
        Assert.That(configuration.FtpUser, Is.EqualTo("ftp-user"));
        Assert.That(configuration.FtpPass, Is.EqualTo("ftp-password"));
        Assert.That(configuration.FtpFolder, Is.EqualTo("/backup"));
        Assert.That(configuration.FtpCoding, Is.EqualTo("UTF8"));
        Assert.That(configuration.FtpEncryptionMode, Is.EqualTo("3"));
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

    private static SettingsViewModel CreateSettingsViewModel(TestConfigurationManager configuration)
    {
        return new SettingsViewModel(
            configuration,
            new TestPresentationService(),
            new TestJobService(),
            new TestQueryManager(),
            new BackupTargetService());
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
        public Task<(bool, BSH.MainApp.ViewModels.Windows.NewBackupViewModel)> ShowCreateBackupWindowAsync() => throw new NotImplementedException();
        public Task<(bool, BSH.MainApp.ViewModels.Windows.EditBackupViewModel)> ShowEditBackupWindowAsync(BSH.MainApp.ViewModels.Windows.EditBackupViewModel backupViewModel) => throw new NotImplementedException();
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(true);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.Primary);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
    }

    private sealed class TestJobService : IJobService
    {
        public bool IsCancellationRequested => false;
        public void Cancel() { }
        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(true);
        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "") => Task.FromResult(true);
        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true) => Task.CompletedTask;
        public CancellationToken GetNewCancellationToken() => CancellationToken.None;
        public Task<bool> RequestPassword() => Task.FromResult(true);
        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
    }

    private sealed class TestQueryManager : IQueryManager
    {
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
        public List<VersionDetails> GetVersions(bool desc = true) => new();
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }
}

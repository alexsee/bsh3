// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Windows.UI.Popups;

namespace BSH.Test;

public class WinUiPresentationParityTests
{
    [Test]
    public void MainWindowSupportMenuDelegatesToPresentationService()
    {
        var presentationService = new TestPresentationService();
        var viewModel = new MainWindowViewModel(presentationService, buildNavigationItems: false);

        foreach (var action in MainWindowViewModel.SupportActionTags)
        {
            Assert.That(viewModel.HandleSupportAction(action), Is.True);
        }

        Assert.That(presentationService.AboutWindowCalls, Is.EqualTo(1));
        Assert.That(presentationService.HelpSupportCalls, Is.EqualTo(1));
        Assert.That(presentationService.EventLogCalls, Is.EqualTo(1));
        Assert.That(presentationService.ResetConfigurationCalls, Is.EqualTo(1));
    }

    [Test]
    public void StatusServiceShowsExceptionListForNonSilentFileErrors()
    {
        var presentationService = new TestPresentationService();
        var statusService = new StatusService(new TestConfigurationManager(), presentationService);
        var files = new Collection<FileExceptionEntry>
        {
            new()
            {
                File = new FileTableRow { FileName = "broken.txt", FilePath = @"C:\Data\" },
                Exception = new InvalidOperationException("copy failed")
            }
        };

        statusService.ReportExceptions(files, silent: false);

        Assert.That(presentationService.ExceptionListRequests, Is.EqualTo(1));
        Assert.That(presentationService.LastExceptionList, Is.SameAs(files));
    }

    [Test]
    public void StatusServiceSuppressesExceptionListForSilentFileErrors()
    {
        var presentationService = new TestPresentationService();
        var statusService = new StatusService(new TestConfigurationManager(), presentationService);

        statusService.ReportExceptions(
            new Collection<FileExceptionEntry>
            {
                new()
                {
                    File = new FileTableRow { FileName = "silent.txt", FilePath = @"C:\Data\" },
                    Exception = new InvalidOperationException("copy failed")
                }
            },
            silent: true);

        Assert.That(presentationService.ExceptionListRequests, Is.EqualTo(0));
    }

    private sealed class TestConfigurationManager : IConfigurationManager
    {
        public string AutoBackup { get; set; } = "";
        public string BackupFolder { get; set; } = "";
        public string BackupSize { get; set; } = "";
        public int Compression { get; set; }
        public string DbStatus { get; set; } = "";
        public string DBVersion { get; set; } = "";
        public string DeativateAutoBackupsWhenAkku { get; set; } = "";
        public string DoPastBackups { get; set; } = "";
        public int Encrypt { get; set; }
        public string EncryptPassMD5 { get; set; } = "";
        public string ExcludeCompression { get; set; } = "";
        public string ExcludeFile { get; set; } = "";
        public string ExcludeFileBigger { get; set; } = "";
        public string ExcludeFileTypes { get; set; } = "";
        public string ExcludeFolder { get; set; } = "";
        public string ExcludeMask { get; set; } = "";
        public string FreeSpace { get; set; } = "";
        public string FtpCoding { get; set; } = "";
        public string FtpEncryptionMode { get; set; } = "";
        public string FtpFolder { get; set; } = "";
        public string FtpHost { get; set; } = "";
        public string FtpPass { get; set; } = "";
        public string FtpPort { get; set; } = "";
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
        public MediaType MediumType { get; set; }
        public string OldBackupPrevent { get; set; } = "";
        public string RemindAfterDays { get; set; } = "";
        public string RemindSpace { get; set; } = "";
        public string ScheduleFullBackup { get; set; } = "";
        public string ShowLocalizedPath { get; set; } = "";
        public string ShowWaitOnMediaAutoBackups { get; set; } = "";
        public string SourceFolder { get; set; } = "";
        public TaskType TaskType { get; set; }
        public string UNCPassword { get; set; } = "";
        public string UNCUsername { get; set; } = "";

        public Task InitializeAsync() => Task.CompletedTask;
    }

    private sealed class TestPresentationService : IPresentationService
    {
        public int AboutWindowCalls { get; private set; }
        public int EventLogCalls { get; private set; }
        public int ExceptionListRequests { get; private set; }
        public int HelpSupportCalls { get; private set; }
        public int ResetConfigurationCalls { get; private set; }
        public IReadOnlyCollection<FileExceptionEntry>? LastExceptionList { get; private set; }

        public Task CloseBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task CloseMainWindowAsync() => Task.CompletedTask;
        public Task<TaskCompleteAction> CloseStatusWindowAsync() => Task.FromResult(TaskCompleteAction.NoAction);
        public Task OpenCurrentEventLogAsync() { EventLogCalls++; return Task.CompletedTask; }
        public Task OpenHelpSupportAsync() { HelpSupportCalls++; return Task.CompletedTask; }
        public Task<(string? password, bool persist)> RequestPasswordAsync() => Task.FromResult<(string?, bool)>((null, false));
        public Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);
        public Task ResetConfigurationAsync() { ResetConfigurationCalls++; return Task.CompletedTask; }
        public Task ShowAboutWindowAsync() { AboutWindowCalls++; return Task.CompletedTask; }
        public Task ShowBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new NewBackupViewModel()));
        public Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;

        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files)
        {
            ExceptionListRequests++;
            LastExceptionList = files;
            return Task.CompletedTask;
        }

        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.None);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace BSH.Test.Fakes;

/// <summary>
/// Virtual default for <see cref="IPresentationService"/>. Subclasses override members a test observes
/// or that returned a different value.
/// </summary>
public class FakePresentationService : IPresentationService
{
    public virtual Task<TaskCompleteAction> CloseStatusWindowAsync() => Task.FromResult(TaskCompleteAction.NoAction);

    public virtual Task OpenCurrentEventLogAsync() => Task.CompletedTask;

    public virtual Task OpenHelpSupportAsync() => Task.CompletedTask;

    public virtual Task<(string? password, bool persist)> RequestPasswordAsync() => Task.FromResult<(string?, bool)>((null, false));

    public virtual Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);

    public virtual Task ResetConfigurationAsync() => Task.CompletedTask;

    public virtual Task ShowAboutWindowAsync() => Task.CompletedTask;

    public virtual Task ShowBackupBrowserWindowAsync() => Task.CompletedTask;

    public virtual Task ShowCompressionExclusionsWindowAsync() => Task.CompletedTask;

    public virtual Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new NewBackupViewModel()));

    public virtual Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));

    public virtual Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);

    public virtual Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;

    public virtual Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;

    public virtual Task ShowLastBackupFailedFilesAsync(LastBackupResult result) => Task.CompletedTask;

    public virtual Task ShowMainWindowAsync() => Task.CompletedTask;

    public virtual Task ShowStatusWindowAsync() => Task.CompletedTask;

    public virtual Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.None);

    public virtual Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;

    public virtual Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;

    public virtual Task<bool> ShowSwitchStorageWindowAsync() => Task.FromResult(false);
}

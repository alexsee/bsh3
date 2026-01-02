// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace BSH.MainApp.Contracts.Services;

public interface IPresentationService
{
    Task CloseBackupBrowserWindowAsync();
    Task CloseMainWindowAsync();
    Task<TaskCompleteAction> CloseStatusWindowAsync();
    Task<(string? password, bool persist)> RequestPasswordAsync();
    Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile);
    Task ShowAboutWindowAsync();
    Task ShowBackupBrowserWindowAsync();
    Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync();
    Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel);
    Task<bool> ShowDeleteBackupWindowAsync();
    Task ShowErrorInsufficientDiskSpaceAsync();
    Task ShowMainWindowAsync();
    Task ShowStatusWindowAsync();
    Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1);
    Task ShowExcludeFileFolderWindowAsync();
}
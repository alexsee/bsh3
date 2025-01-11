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
    void CloseBackupBrowserWindow();
    void CloseMainWindow();
    TaskCompleteAction CloseStatusWindow();
    Task<(string? password, bool persist)> RequestPassword();
    Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile);
    void ShowAboutWindow();
    void ShowBackupBrowserWindow();
    Task<(bool, NewBackupViewModel)> ShowCreateBackupWindow();
    void ShowErrorInsufficientDiskSpace();
    void ShowMainWindow();
    void ShowStatusWindow();
    Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1);
}
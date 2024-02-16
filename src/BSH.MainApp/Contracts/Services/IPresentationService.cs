// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;
using Windows.UI.Popups;

namespace BSH.MainApp.Contracts.Services;

public interface IPresentationService
{
    void CloseBackupBrowserWindow();
    void CloseMainWindow();
    TaskCompleteAction CloseStatusWindow();
    (string? password, bool persist) RequestPassword();
    void ShowAboutWindow();
    void ShowBackupBrowserWindow();
    Task ShowCreateBackupWindow();
    void ShowErrorInsufficientDiskSpace();
    void ShowMainWindow();
    void ShowStatusWindow();

    Task<IUICommand> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1);
}
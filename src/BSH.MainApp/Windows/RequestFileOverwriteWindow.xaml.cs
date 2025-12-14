// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.ViewModels.Windows;
using WinUIEx;

namespace BSH.MainApp.Windows;

public sealed partial class RequestFileOverwriteWindow : WinUIEx.WindowEx
{
    private RequestFileOverwriteViewModel ViewModel { get; } = new RequestFileOverwriteViewModel();

    public RequestFileOverwriteWindow()
    {
        InitializeComponent();
    }

    public async Task<RequestOverwriteResult> ShowDialogAsync(FileTableRow localFile, FileTableRow remoteFile)
    {
        ViewModel.FileName = localFile.FileName;
        ViewModel.SourceFileSize = localFile.FileSize;
        ViewModel.SourceLastModified = localFile.FileDateModified;
        ViewModel.DestinationFileSize = remoteFile.FileSize;
        ViewModel.DestinationLastModified = remoteFile.FileDateModified;

        Activate();
        this.CenterOnScreen();

        var result = await ViewModel.TaskCompletionSource.Task;
        Close();
        return result;
    }
}

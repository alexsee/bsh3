// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.ViewModels.Windows;

namespace BSH.MainApp.Windows;
public sealed partial class RequestFileOverwriteWindow : WinUIEx.WindowEx
{
    private RequestFileOverwriteViewModel ViewModel { get; } = new RequestFileOverwriteViewModel();

    public RequestFileOverwriteWindow()
    {
        this.InitializeComponent();
    }

    public async Task<RequestOverwriteResult> ShowDialogAsync(FileTableRow localFile, FileTableRow remoteFile)
    {
        this.ViewModel.FileName = localFile.FileName;
        this.ViewModel.SourceFileSize = localFile.FileSize;
        this.ViewModel.SourceLastModified = localFile.FileDateModified;
        this.ViewModel.DestinationFileSize = remoteFile.FileSize;
        this.ViewModel.DestinationLastModified = remoteFile.FileDateModified;

        Activate();

        var result = await this.ViewModel.TaskCompletionSource.Task;
        Close();
        return result;
    }
}

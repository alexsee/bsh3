// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.ViewModels.Windows;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSH.MainApp.Windows;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RequestFileOverwriteWindow : WinUIEx.WindowEx
{
    private RequestFileOverwriteViewModel ViewModel { get; } = new RequestFileOverwriteViewModel();

    public RequestFileOverwriteWindow()
    {
        this.InitializeComponent();
    }

    public async Task<RequestOverwriteResult> ShowDialogAsync(FileTableRow localFile, FileTableRow remoteFile)
    {
        this.ViewModel.TaskCompletionSource = new TaskCompletionSource<RequestOverwriteResult>();

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

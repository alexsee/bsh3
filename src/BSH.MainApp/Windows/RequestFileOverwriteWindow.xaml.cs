// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.Windows;

public sealed partial class RequestFileOverwriteWindow : WinUIEx.WindowEx
{
    private readonly ModalCloseHandler closeHandler = new();

    private RequestFileOverwriteViewModel ViewModel { get; } = new RequestFileOverwriteViewModel();

    public RequestFileOverwriteWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    public Task<RequestOverwriteResult> ShowDialogAsync(FileTableRow localFile, FileTableRow remoteFile)
    {
        ViewModel.FileName = localFile.FileName;
        ViewModel.SourceFileSize = localFile.FileSize;
        ViewModel.SourceLastModified = localFile.FileDateModified;
        ViewModel.DestinationFileSize = remoteFile.FileSize;
        ViewModel.DestinationLastModified = remoteFile.FileDateModified;

        Activate();
        this.CenterOnMainWindow();
        return closeHandler.AwaitThenCloseAsync(ViewModel.TaskCompletionSource.Task, Close);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        closeHandler.HandleClosed(ViewModel.OnWindowClosed);
    }
}

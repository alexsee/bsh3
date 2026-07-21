// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime.Ports;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.Services;

/// <summary>
/// WinUI implementation of <see cref="IJobSessionPresenter"/> using existing presentation
/// and status services.
/// </summary>
public sealed class WinUIJobSessionPresenter : IJobSessionPresenter
{
    private readonly Action cancelAction;
    private readonly ICompletionActionService completionActionService;
    private readonly IPresentationService presentationService;
    private readonly IStatusService statusService;
    private CancellationToken cancellationToken;
    private bool statusWindowShown;

    public WinUIJobSessionPresenter(
        IPresentationService presentationService,
        IStatusService statusService,
        Action cancelAction,
        ICompletionActionService? completionActionService = null)
    {
        ArgumentNullException.ThrowIfNull(presentationService);
        ArgumentNullException.ThrowIfNull(statusService);
        ArgumentNullException.ThrowIfNull(cancelAction);

        this.cancelAction = cancelAction;
        this.completionActionService = completionActionService ?? new CompletionActionService();
        this.presentationService = presentationService;
        this.statusService = statusService;
    }

    public async Task ShowStatusWindowAsync()
    {
        await presentationService.ShowStatusWindowAsync();
        statusWindowShown = true;
    }

    public async Task CompleteAsync(bool triggerShutdown = false, bool triggerHibernate = false, bool honorCompletionActions = true)
    {
        var action = TaskCompleteAction.NoAction;
        if (statusWindowShown)
        {
            action = await presentationService.CloseStatusWindowAsync();
            statusWindowShown = false;
        }

        var completionAction = ResolveCompletionAction(triggerShutdown, triggerHibernate, honorCompletionActions, action);
        if (completionAction != TaskCompleteAction.NoAction)
        {
            await completionActionService.ExecuteAsync(completionAction);
        }
    }

    private static TaskCompleteAction ResolveCompletionAction(
        bool triggerShutdown,
        bool triggerHibernate,
        bool honorCompletionActions,
        TaskCompleteAction selectedAction)
    {
        if (triggerShutdown)
        {
            return TaskCompleteAction.ShutdownPC;
        }

        if (triggerHibernate)
        {
            return TaskCompleteAction.HibernatePC;
        }

        return honorCompletionActions ? selectedAction : TaskCompleteAction.NoAction;
    }

    public Task ShowErrorTaskRunningAsync()
    {
        return presentationService.ShowMessageBoxAsync(
            "MSG_TASK_RUNNING_TITLE".GetLocalized(),
            "MSG_TASK_RUNNING_TEXT".GetLocalized(),
            null);
    }

    public Task ShowErrorDeviceNotReadyAsync()
    {
        return presentationService.ShowMessageBoxAsync(
            "MSG_BACKUP_DEVICE_NOT_READY_TITLE".GetLocalized(),
            "MSG_BACKUP_DEVICE_NOT_READY_TEXT".GetLocalized(),
            null);
    }

    public Task ShowErrorPasswordRequiredAsync()
    {
        return presentationService.ShowMessageBoxAsync(
            "Password_Required_Title".GetLocalized(),
            "Password_Required_Text".GetLocalized(),
            null);
    }

    public async Task<JobSessionPasswordRequest> RequestPasswordAsync()
    {
        var (password, persist) = await presentationService.RequestPasswordAsync();
        return new JobSessionPasswordRequest(password, persist);
    }

    public Task ShowErrorPasswordWrongAsync()
    {
        return presentationService.ShowMessageBoxAsync(
            "MSG_PASSWORD_WRONG_TITLE".GetLocalized(),
            "MSG_PASSWORD_WRONG_TEXT".GetLocalized(),
            null);
    }

    public Task CancelAsync()
    {
        cancelAction();
        return Task.CompletedTask;
    }

    public CancellationToken GetCancellationToken() => cancellationToken;

    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        this.cancellationToken = cancellationToken;
    }

    public FileOverwrite ResolveBatchOverwriteChoice(FileOverwrite currentOverwrite)
    {
        return statusService.LastFileOverwriteChoice switch
        {
            RequestOverwriteResult.OverwriteAll => FileOverwrite.Overwrite,
            RequestOverwriteResult.NoOverwriteAll => FileOverwrite.DontCopy,
            _ => currentOverwrite
        };
    }

    public void ReportAction(ActionType action, bool silent) => statusService.ReportAction(action, silent);

    public void ReportState(JobState jobState) => statusService.ReportState(jobState);

    public void ReportStatus(string title, string text) => statusService.ReportStatus(title, text);

    public void ReportProgress(int total, int current) => statusService.ReportProgress(total, current);

    public void ReportFileProgress(string file) => statusService.ReportFileProgress(file);

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) => statusService.ReportExceptions(files, silent);

    public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) => statusService.RequestOverwrite(localFile, remoteFile);

    public Task RequestShowErrorInsufficientDiskSpaceAsync() => statusService.RequestShowErrorInsufficientDiskSpaceAsync();
}

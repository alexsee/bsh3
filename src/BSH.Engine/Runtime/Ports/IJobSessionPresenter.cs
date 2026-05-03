// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Jobs;

namespace Brightbits.BSH.Engine.Runtime.Ports;

/// <summary>
/// Defines the contract for job-session presentation in a specific product (WinForms, CLI, etc.).
/// Unifies session lifecycle, progress/state reporting, and in-session interaction responsibilities.
/// </summary>
public interface IJobSessionPresenter : IJobReport
{
    /// <summary>
    /// Shows the status window for the running job session.
    /// </summary>
    Task ShowStatusWindowAsync();

    /// <summary>
    /// Handles completion of the job session, including optional shutdown/hibernate actions.
    /// </summary>
    Task CompleteAsync(bool triggerShutdown = false, bool triggerHibernate = false, bool honorCompletionActions = true);

    /// <summary>
    /// Shows error dialog when another task is already running.
    /// </summary>
    Task ShowErrorTaskRunningAsync();

    /// <summary>
    /// Shows error dialog when the backup device is not ready.
    /// </summary>
    Task ShowErrorDeviceNotReadyAsync();

    /// <summary>
    /// Handles password request cancellation.
    /// </summary>
    Task ShowErrorPasswordRequiredAsync();

    /// <summary>
    /// Requests the password from the user for the current live session.
    /// </summary>
    Task<JobSessionPasswordRequest> RequestPasswordAsync();

    /// <summary>
    /// Shows an error when the entered password is incorrect.
    /// </summary>
    Task ShowErrorPasswordWrongAsync();

    /// <summary>
    /// Cancels the running job session.
    /// </summary>
    Task CancelAsync();

    /// <summary>
    /// Gets the cancellation token for the current session.
    /// </summary>
    CancellationToken GetCancellationToken();

    /// <summary>
    /// Sets the cancellation token for the current session.
    /// </summary>
    void SetCancellationToken(CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the overwrite mode that should be carried into the next batch restore item.
    /// </summary>
    FileOverwrite ResolveBatchOverwriteChoice(FileOverwrite currentOverwrite);
}

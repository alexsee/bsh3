// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Runtime.Ports;

namespace Brightbits.BSH.Engine.Runtime;

public enum JobSessionStartFailure
{
    None = 0,
    TaskRunning = 1,
    DeviceNotReady = 2,
    PasswordRequired = 3
}

/// <summary>
/// Represents the outcome of running a single-backup job session.
/// </summary>
public readonly struct SingleBackupSessionResult
{
    /// <summary>
    /// Gets a value indicating whether the job session was prepared and started.
    /// </summary>
    public bool Started { get; init; }

    /// <summary>
    /// Gets a value indicating whether cancellation was requested for the running session.
    /// </summary>
    public bool Canceled { get; init; }

    /// <summary>
    /// Gets the startup failure reason when the session could not be started.
    /// </summary>
    public JobSessionStartFailure Failure { get; init; }
}

/// <summary>
/// Executes shared job-session orchestration flows on top of <see cref="JobRuntime"/>
/// and <see cref="IBackupService"/>.
/// </summary>
public sealed class JobSessionRunner
{
    private readonly IBackupService backupService;
    private readonly JobRuntime jobRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobSessionRunner"/> class.
    /// </summary>
    /// <param name="backupService">Backup engine service used to execute backup operations.</param>
    /// <param name="jobRuntime">Shared runtime responsible for session preparation and cancellation lifecycle.</param>
    public JobSessionRunner(IBackupService backupService, JobRuntime jobRuntime)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(jobRuntime);

        this.backupService = backupService;
        this.jobRuntime = jobRuntime;
    }

    /// <summary>
    /// Runs a single-backup session with full error handling and user dialogs.
    /// </summary>
    public async Task<SingleBackupSessionResult> RunSingleBackupAsync(
        string title,
        string description,
        IJobSessionPresenter presenter,
        bool statusDialog = true,
        bool fullBackup = false,
        string sourceFolders = "")
    {
        ArgumentNullException.ThrowIfNull(presenter);

        IJobReport jobReport = presenter;

        try
        {
            if (statusDialog)
            {
                await presenter.ShowStatusWindowAsync();
            }

            var cancellationToken = await jobRuntime.PrepareAsync(ActionType.Backup, statusDialog);
            presenter.SetCancellationToken(cancellationToken);

            try
            {
                await backupService.StartBackup(title, description, ref jobReport, cancellationToken, fullBackup, sourceFolders, !statusDialog);
            }
            catch
            {
            }

            return new SingleBackupSessionResult()
            {
                Started = true,
                Canceled = cancellationToken.IsCancellationRequested,
                Failure = JobSessionStartFailure.None
            };
        }
        catch (TaskRunningException)
        {
            if (statusDialog)
            {
                await presenter.ShowErrorTaskRunningAsync();
            }

            return new SingleBackupSessionResult() { Failure = JobSessionStartFailure.TaskRunning };
        }
        catch (Exception ex) when (ex is DeviceNotReadyException || ex is DeviceContainsWrongStateException)
        {
            if (statusDialog)
            {
                await presenter.ShowErrorDeviceNotReadyAsync();
            }

            return new SingleBackupSessionResult() { Failure = JobSessionStartFailure.DeviceNotReady };
        }
        catch (PasswordRequiredException)
        {
            if (statusDialog)
            {
                await presenter.ShowErrorPasswordRequiredAsync();
            }

            return new SingleBackupSessionResult() { Failure = JobSessionStartFailure.PasswordRequired };
        }
    }
}

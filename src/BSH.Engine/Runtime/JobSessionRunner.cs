// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;

namespace Brightbits.BSH.Engine.Runtime;

/// <summary>
/// Defines startup failure reasons for a job session before a job begins execution.
/// </summary>
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
    /// Runs a complete single-backup session by preparing runtime prerequisites and
    /// executing exactly one backup operation.
    /// </summary>
    /// <param name="title">Backup title.</param>
    /// <param name="description">Backup description.</param>
    /// <param name="jobReport">Job report callback used by the backup job.</param>
    /// <param name="statusDialog">Whether the caller is running with a visible status dialog.</param>
    /// <param name="fullBackup">Whether a full backup should be executed.</param>
    /// <param name="sourceFolders">Optional source folder filter for this backup run.</param>
    /// <returns>
    /// A <see cref="SingleBackupSessionResult"/> describing startup outcome and cancellation state.
    /// </returns>
    public async Task<SingleBackupSessionResult> RunSingleBackupAsync(
        string title,
        string description,
        IJobReport jobReport,
        bool statusDialog = true,
        bool fullBackup = false,
        string sourceFolders = "")
    {
        try
        {
            var cancellationToken = await jobRuntime.PrepareAsync(ActionType.Backup, statusDialog);

            try
            {
                await backupService.StartBackup(title, description, ref jobReport, cancellationToken, fullBackup, sourceFolders, !statusDialog);
            }
            catch
            {
                // exception already handled
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
            return new SingleBackupSessionResult() { Failure = JobSessionStartFailure.TaskRunning };
        }
        catch (Exception ex) when (ex is DeviceNotReadyException || ex is DeviceContainsWrongStateException)
        {
            return new SingleBackupSessionResult() { Failure = JobSessionStartFailure.DeviceNotReady };
        }
        catch (PasswordRequiredException)
        {
            return new SingleBackupSessionResult() { Failure = JobSessionStartFailure.PasswordRequired };
        }
    }
}

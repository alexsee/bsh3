// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Service.Contracts;
using Brightbits.BSH.Engine.Types.Exceptions;
using Brightbits.BSH.Engine.Service.Jobs;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Runtime.Ports;
using Brightbits.BSH.Engine.Utils.Security;

namespace Brightbits.BSH.Engine.Runtime;

public enum JobSessionStartFailure
{
    None = 0,
    TaskRunning = 1,
    DeviceNotReady = 2,
    PasswordRequired = 3
}

/// <summary>
/// Represents the outcome of running a job session.
/// </summary>
public readonly struct JobSessionResult
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
    private readonly Func<bool> shouldResolvePassword;
    private readonly Func<string> expectedPasswordHashProvider;
    private readonly IStoredPasswordAdapter storedPasswordAdapter;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobSessionRunner"/> class.
    /// </summary>
    /// <param name="backupService">Backup engine service used to execute backup operations.</param>
    /// <param name="jobRuntime">Shared runtime responsible for session preparation and cancellation lifecycle.</param>
    public JobSessionRunner(IBackupService backupService, JobRuntime jobRuntime, Func<bool> shouldResolvePassword = null, Func<string> expectedPasswordHashProvider = null, IStoredPasswordAdapter storedPasswordAdapter = null)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(jobRuntime);

        this.backupService = backupService;
        this.jobRuntime = jobRuntime;
        this.shouldResolvePassword = shouldResolvePassword ?? (() => false);
        this.expectedPasswordHashProvider = expectedPasswordHashProvider ?? (() => string.Empty);
        this.storedPasswordAdapter = storedPasswordAdapter;
    }

    /// <summary>
    /// Runs a single-backup session with full error handling and user dialogs.
    /// </summary>
    public async Task<JobSessionResult> RunSingleBackupAsync(
        string title,
        string description,
        IJobSessionPresenter presenter,
        bool statusDialog = true,
        bool fullBackup = false,
        string sourceFolders = "")
    {
        return await RunSingleOperationAsync(
            ActionType.Backup,
            presenter,
            statusDialog,
            requirePassword: true,
            async (jobReport, cancellationToken) =>
            {
                await backupService.StartBackup(title, description, jobReport, cancellationToken, fullBackup, sourceFolders, !statusDialog);
            });
    }

    /// <summary>
    /// Runs a single restore session with shared session preparation and error handling.
    /// </summary>
    public async Task<JobSessionResult> RunSingleRestoreAsync(
        string version,
        string file,
        string destination,
        IJobSessionPresenter presenter,
        bool statusDialog = true,
        FileOverwrite overwrite = FileOverwrite.Ask)
    {
        return await RunSingleOperationAsync(
            ActionType.Restore,
            presenter,
            statusDialog,
            requirePassword: true,
            async (jobReport, cancellationToken) =>
            {
                await backupService.StartRestore(version, file, destination, jobReport, cancellationToken, overwrite, !statusDialog);
            });
    }

    /// <summary>
    /// Runs a batch restore session with shared lifecycle orchestration, overwrite carry-forward,
    /// exception aggregation, and final-state reporting.
    /// </summary>
    public async Task<JobSessionResult> RunBatchRestoreAsync(
        string version,
        IReadOnlyList<string> files,
        string destination,
        IJobSessionPresenter presenter,
        bool statusDialog = true)
    {
        ArgumentNullException.ThrowIfNull(files);

        var overwrite = FileOverwrite.Ask;

        return await RunBatchOperationAsync(
            ActionType.Restore,
            presenter,
            statusDialog,
            requirePassword: true,
            files.Count,
            async (jobReport, cancellationToken, index) =>
            {
                var file = files[index];
                await backupService.StartRestore(version, file, destination, jobReport, cancellationToken, overwrite, !statusDialog);
                overwrite = presenter.ResolveBatchOverwriteChoice(overwrite);
            });
    }

    /// <summary>
    /// Runs a single delete session with shared session preparation and error handling.
    /// </summary>
    public async Task<JobSessionResult> RunSingleDeleteAsync(
        string version,
        IJobSessionPresenter presenter,
        bool statusDialog = true)
    {
        return await RunSingleOperationAsync(
            ActionType.Delete,
            presenter,
            statusDialog,
            requirePassword: false,
            async (jobReport, cancellationToken) =>
            {
                await backupService.StartDelete(version, jobReport, cancellationToken, !statusDialog);
            });
    }

    /// <summary>
    /// Runs a batch delete session with shared lifecycle orchestration, exception aggregation,
    /// and final-state reporting.
    /// </summary>
    public async Task<JobSessionResult> RunBatchDeleteAsync(
        IReadOnlyList<string> versions,
        IJobSessionPresenter presenter,
        bool statusDialog = true)
    {
        ArgumentNullException.ThrowIfNull(versions);

        return await RunBatchOperationAsync(
            ActionType.Delete,
            presenter,
            statusDialog,
            requirePassword: false,
            versions.Count,
            async (jobReport, cancellationToken, index) =>
            {
                var version = versions[index];
                await backupService.StartDelete(version, jobReport, cancellationToken, !statusDialog);
            });
    }

    /// <summary>
    /// Runs a single delete-single-file session with shared session preparation and error handling.
    /// </summary>
    public async Task<JobSessionResult> RunSingleDeleteSingleAsync(
        string fileFilter,
        string pathFilter,
        IJobSessionPresenter presenter,
        bool statusDialog = true)
    {
        return await RunSingleOperationAsync(
            ActionType.Delete,
            presenter,
            statusDialog,
            requirePassword: false,
            async (jobReport, cancellationToken) =>
            {
                await backupService.StartDeleteSingle(fileFilter, pathFilter, jobReport, cancellationToken, !statusDialog);
            });
    }

    /// <summary>
    /// Runs a single modify session with shared session preparation and error handling.
    /// </summary>
    public async Task<JobSessionResult> RunSingleModifyAsync(
        IJobSessionPresenter presenter,
        bool statusDialog = true)
    {
        return await RunSingleOperationAsync(
            ActionType.Modify,
            presenter,
            statusDialog,
            requirePassword: true,
            async (jobReport, cancellationToken) =>
            {
                await backupService.StartEdit(jobReport, cancellationToken, !statusDialog);
            });
    }

    private async Task<JobSessionResult> RunSingleOperationAsync(
        ActionType action,
        IJobSessionPresenter presenter,
        bool statusDialog,
        bool requirePassword,
        Func<IJobReport, CancellationToken, Task> startAsync)
    {
        ArgumentNullException.ThrowIfNull(presenter);
        ArgumentNullException.ThrowIfNull(startAsync);

        IJobReport jobReport = presenter;

        try
        {
            if (statusDialog)
            {
                await presenter.ShowStatusWindowAsync();
            }

            var cancellationToken = await jobRuntime.PrepareAsync(action, statusDialog, requirePassword: false);
            presenter.SetCancellationToken(cancellationToken);

            if (requirePassword)
            {
                await ResolvePasswordAsync(presenter);
            }

            try
            {
                await startAsync(jobReport, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                jobReport.ReportExceptions(new Collection<FileExceptionEntry> { new() { Exception = ex } }, !statusDialog);
            }

            return new JobSessionResult()
            {
                Started = true,
                Canceled = cancellationToken.IsCancellationRequested,
                Failure = JobSessionStartFailure.None
            };
        }
        catch (Exception ex)
        {
            return await HandleSessionStartFailureAsync(ex, presenter, statusDialog);
        }
    }

    private async Task<JobSessionResult> RunBatchOperationAsync(
        ActionType action,
        IJobSessionPresenter presenter,
        bool statusDialog,
        bool requirePassword,
        int itemCount,
        Func<IJobReport, CancellationToken, int, Task> runItemAsync)
    {
        ArgumentNullException.ThrowIfNull(presenter);
        ArgumentNullException.ThrowIfNull(runItemAsync);

        try
        {
            if (statusDialog)
            {
                await presenter.ShowStatusWindowAsync();
            }

            var cancellationToken = await jobRuntime.PrepareAsync(action, statusDialog, requirePassword: false);
            presenter.SetCancellationToken(cancellationToken);

            if (requirePassword)
            {
                await ResolvePasswordAsync(presenter);
            }

            presenter.ReportAction(action, !statusDialog);
            presenter.ReportState(JobState.RUNNING);

            var forwardJobReport = new ForwardJobReport(presenter);

            for (var i = 0; i < itemCount; i++)
            {
                try
                {
                    presenter.ReportProgress(itemCount, i + 1);
                    IJobReport activeReport = forwardJobReport;
                    await runItemAsync(activeReport, cancellationToken, i);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    forwardJobReport.ReportExceptions(new Collection<FileExceptionEntry> { new() { Exception = ex } }, !statusDialog);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            forwardJobReport.ForwardExceptions(!statusDialog);

            var finalState = cancellationToken.IsCancellationRequested
                ? JobState.CANCELED
                : (forwardJobReport.HasExceptions ? JobState.ERROR : JobState.FINISHED);
            presenter.ReportState(finalState);

            return new JobSessionResult()
            {
                Started = true,
                Canceled = cancellationToken.IsCancellationRequested,
                Failure = JobSessionStartFailure.None
            };
        }
        catch (Exception ex)
        {
            return await HandleSessionStartFailureAsync(ex, presenter, statusDialog);
        }
    }

    private static async Task<JobSessionResult> HandleSessionStartFailureAsync(
        Exception exception,
        IJobSessionPresenter presenter,
        bool statusDialog)
    {
        if (exception is TaskRunningException)
        {
            if (statusDialog)
            {
                await presenter.ShowErrorTaskRunningAsync();
            }

            return new JobSessionResult() { Failure = JobSessionStartFailure.TaskRunning };
        }

        if (exception is DeviceNotReadyException or DeviceContainsWrongStateException)
        {
            if (statusDialog)
            {
                await presenter.ShowErrorDeviceNotReadyAsync();
            }

            return new JobSessionResult() { Failure = JobSessionStartFailure.DeviceNotReady };
        }

        if (exception is PasswordRequiredException)
        {
            if (statusDialog)
            {
                await presenter.ShowErrorPasswordRequiredAsync();
            }

            return new JobSessionResult() { Failure = JobSessionStartFailure.PasswordRequired };
        }

        throw exception;
    }

    public async Task<bool> EnsurePasswordResolvedAsync(IJobSessionPresenter presenter)
    {
        try
        {
            await ResolvePasswordAsync(presenter);
            return true;
        }
        catch (PasswordRequiredException)
        {
            return false;
        }
    }

    private async Task ResolvePasswordAsync(IJobSessionPresenter presenter)
    {
        if (backupService.HasPassword() || !shouldResolvePassword())
        {
            return;
        }

        var storedPassword = storedPasswordAdapter == null
            ? string.Empty
            : await storedPasswordAdapter.GetPasswordAsync();
        if (IsValidPassword(storedPassword))
        {
            backupService.SetPassword(storedPassword);
            return;
        }

        var request = await presenter.RequestPasswordAsync();
        while (!string.IsNullOrEmpty(request.Password))
        {
            if (IsValidPassword(request.Password))
            {
                backupService.SetPassword(request.Password);

                if (request.Persist)
                {
                    if (storedPasswordAdapter != null)
                    {
                        await storedPasswordAdapter.StorePasswordAsync(request.Password);
                    }
                }

                return;
            }

            await presenter.ShowErrorPasswordWrongAsync();
            request = await presenter.RequestPasswordAsync();
        }

        throw new PasswordRequiredException();
    }

    private bool IsValidPassword(string password)
    {
        var expectedPasswordHash = expectedPasswordHashProvider() ?? string.Empty;
        return !string.IsNullOrEmpty(password) &&
            string.Equals(Hash.GetMD5Hash(password) ?? string.Empty, expectedPasswordHash, StringComparison.Ordinal);
    }
}

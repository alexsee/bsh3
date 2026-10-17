// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Serilog;

namespace Brightbits.BSH.Engine.Runtime;

/// <summary>
/// What a backup session reported, as observed by <see cref="JobSessionRunner"/>.
/// </summary>
public sealed class BackupSessionObservation
{
    public JobSessionResult Result { get; init; }

    /// <summary>
    /// True when the user (or the cancellation token) stopped the backup, as opposed to the job
    /// aborting itself, for example because the backup device ran out of space.
    /// </summary>
    public bool CanceledByUser { get; init; }

    public bool InsufficientDiskSpaceReported { get; init; }

    public IReadOnlyCollection<FileExceptionEntry> Errors { get; init; } = [];
}

/// <summary>
/// Persists the outcome of each backup run in <see cref="IConfigurationManager.LastBackupResult"/>.
/// </summary>
public sealed class BackupResultRecorder
{
    private static readonly ILogger Logger = Log.ForContext<BackupResultRecorder>();

    private readonly IConfigurationManager configurationManager;
    private readonly Func<DateTime> clock;

    public BackupResultRecorder(IConfigurationManager configurationManager, Func<DateTime> clock = null)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        this.configurationManager = configurationManager;
        this.clock = clock ?? (() => DateTime.Now);
    }

    public LastBackupResult Load() => LastBackupResult.FromJson(configurationManager.LastBackupResult);

    /// <summary>
    /// Captures the state needed to tell afterwards whether the backup wrote a new version.
    /// </summary>
    public string CaptureCheckpoint() => configurationManager.LastBackupDone ?? "";

    public void Record(string checkpoint, BackupSessionObservation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);

        try
        {
            var result = Classify(checkpoint, observation);
            if (result != null)
            {
                configurationManager.LastBackupResult = result.ToJson();
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Could not store the result of the last backup.");
        }
    }

    private LastBackupResult Classify(string checkpoint, BackupSessionObservation observation)
    {
        var session = observation.Result;

        if (!session.Started)
        {
            return session.Failure == JobSessionStartFailure.DeviceNotReady
                ? CreateFailure(BackupFailureReason.DeviceNotReady, "", [])
                : null;
        }

        if (observation.CanceledByUser)
        {
            return null;
        }

        var fileErrors = observation.Errors.Where(entry => entry.File != null).ToList();
        var jobErrors = observation.Errors.Where(entry => entry.File == null).ToList();

        if (observation.InsufficientDiskSpaceReported)
        {
            return CreateFailure(BackupFailureReason.InsufficientDiskSpace, "", fileErrors);
        }

        if (jobErrors.Count > 0)
        {
            var exception = jobErrors[0].Exception;
            var reason = exception is DeviceNotReadyException or DeviceContainsWrongStateException
                ? BackupFailureReason.DeviceNotReady
                : BackupFailureReason.Other;
            return CreateFailure(reason, exception?.Message ?? "", fileErrors);
        }

        if (!session.HasErrors && !session.Canceled)
        {
            return CreateResult(BackupOutcome.Succeeded, BackupFailureReason.None, "", []);
        }

        var newVersionWritten = !string.IsNullOrEmpty(configurationManager.LastBackupDone)
            && !string.Equals(configurationManager.LastBackupDone, checkpoint, StringComparison.Ordinal);

        return newVersionWritten
            ? CreateResult(BackupOutcome.CompletedWithErrors, BackupFailureReason.None, "", fileErrors)
            : CreateFailure(BackupFailureReason.Other, "", fileErrors);
    }

    private LastBackupResult CreateFailure(BackupFailureReason reason, string message, IReadOnlyList<FileExceptionEntry> fileErrors)
    {
        return CreateResult(BackupOutcome.Failed, reason, message, fileErrors);
    }

    private LastBackupResult CreateResult(BackupOutcome outcome, BackupFailureReason reason, string message, IReadOnlyList<FileExceptionEntry> fileErrors)
    {
        return new LastBackupResult
        {
            CompletedAt = clock(),
            Outcome = outcome,
            FailureReason = reason,
            FailureMessage = message,
            FailedFileCount = fileErrors.Count,
            FailedFiles = fileErrors
                .Take(LastBackupResult.MaxStoredFiles)
                .Select(entry => new LastBackupFailedFile
                {
                    Path = entry.File.FileNamePath(),
                    Message = GetMessage(entry.Exception)
                })
                .ToList()
        };
    }

    private static string GetMessage(Exception exception)
    {
        if (exception is FileNotProcessedException && exception.InnerException != null)
        {
            return exception.InnerException.Message;
        }

        return exception?.Message ?? "";
    }
}

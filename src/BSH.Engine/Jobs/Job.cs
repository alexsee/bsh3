// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for all job tasks
/// </summary>
public abstract class Job : IDisposable
{
    private static readonly ILogger _logger = Log.ForContext<Job>();

    protected readonly IStorageProvider storage;

    protected readonly IDbClientFactory dbClientFactory;

    protected readonly IQueryManager queryManager;

    protected readonly IConfigurationManager configurationManager;
    protected readonly IVersionQueryRepository versionQueryRepository;

    protected readonly bool silent;

    private readonly List<IJobReport> observers = new();
    private bool disposed;
    private bool keepsSystemAwake;

    public Collection<FileExceptionEntry> FileErrorList
    {
        get;
    }

    protected Job(
        IStorageProvider storage,
        IDbClientFactory dbClientFactory,
        IQueryManager queryManager,
        IConfigurationManager configurationManager,
        IVersionQueryRepository versionQueryRepository,
        bool silent = false)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(dbClientFactory);
        ArgumentNullException.ThrowIfNull(queryManager);
        ArgumentNullException.ThrowIfNull(configurationManager);
        ArgumentNullException.ThrowIfNull(versionQueryRepository);

        this.storage = storage;
        this.dbClientFactory = dbClientFactory;
        this.queryManager = queryManager;
        this.configurationManager = configurationManager;
        this.versionQueryRepository = versionQueryRepository;
        this.silent = silent;
        this.FileErrorList = new Collection<FileExceptionEntry>();
    }

    /// <summary>
    /// Adds the given exception to the file exception list.
    /// </summary>
    /// <param name="versionDate">The version date of the backup.</param>
    /// <param name="file">The file that could not be copied.</param>
    /// <param name="ex">The exception that occured.</param>
    /// <returns></returns>
    protected FileExceptionEntry AddFileErrorToList(string versionDate, FileTableRow file, Exception ex)
    {
        return CreateFileErrorEntry(file, ex, versionDate, null);
    }

    /// <summary>
    /// Adds the given exception to the file exception list.
    /// </summary>
    /// <param name="versionDate">The version date of the backup.</param>
    /// <param name="versionId">The version id of the backup.</param>
    /// <param name="file">The file that could not be copied.</param>
    /// <param name="ex">The exception that occured.</param>
    /// <returns></returns>
    protected FileExceptionEntry AddFileErrorToList(string versionDate, long versionId, FileTableRow file, Exception ex)
    {
        return CreateFileErrorEntry(file, ex, versionDate, versionId);
    }

    /// <summary>
    /// Adds the given exception to the file exception list.
    /// </summary>
    /// <param name="file">The file that could not be copied.</param>
    /// <param name="ex">The exception that occured.</param>
    /// <returns></returns>
    protected FileExceptionEntry AddFileErrorToList(FileTableRow file, Exception ex)
    {
        return CreateFileErrorEntry(file, ex, null, null);
    }

    private FileExceptionEntry CreateFileErrorEntry(FileTableRow file, Exception ex, string versionDate, long? versionId)
    {
        var fileExceptionEntry = new FileExceptionEntry()
        {
            Exception = ex,
            File = file,
            NewVersionDate = versionDate,
        };

        if (versionId.HasValue)
        {
            fileExceptionEntry.NewVersionId = versionId.Value;
        }

        FileErrorList.Add(fileExceptionEntry);
        return fileExceptionEntry;
    }

    private void ForEachObserver(Action<IJobReport> report)
    {
        foreach (var observer in observers.ToArray())
        {
            try
            {
                report(observer);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    private async Task ForEachObserverAsync(Func<IJobReport, Task> report)
    {
        foreach (var observer in observers.ToArray())
        {
            try
            {
                await report(observer);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    public void ReportState(JobState jobState)
    {
        ForEachObserver(observer => observer.ReportState(jobState));
    }

    protected void ReportStatus(string title, string text)
    {
        ForEachObserver(observer => observer.ReportStatus(title, text));
    }

    protected void ReportProgress(int total, int current)
    {
        ForEachObserver(observer => observer.ReportProgress(total, current));
    }

    protected void ReportFileProgress(string file)
    {
        ForEachObserver(observer => observer.ReportFileProgress(file));
    }

    protected void ReportExceptions(Collection<FileExceptionEntry> files)
    {
        ForEachObserver(observer => observer.ReportExceptions(files, this.silent));
    }

    protected async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        var snapshot = observers.ToArray();
        if (snapshot.Length > 0)
        {
            return await snapshot[0].RequestOverwrite(localFile, remoteFile);
        }

        return RequestOverwriteResult.Overwrite;
    }

    protected async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        await ForEachObserverAsync(observer => observer.RequestShowErrorInsufficientDiskSpaceAsync());
    }

    public void AddObserver(IJobReport observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        observers.Add(observer);
    }

    public void RemoveObserver(IJobReport observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        observers.Remove(observer);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        try
        {
            storage.Dispose();
        }
        finally
        {
            if (keepsSystemAwake)
            {
                keepsSystemAwake = false;
                Win32Stuff.AllowSystemSleep();
            }
        }

        GC.SuppressFinalize(this);
    }

    protected void KeepSystemAwake()
    {
        Win32Stuff.KeepSystemAwake();
        keepsSystemAwake = true;
    }

    /// <summary>
    /// Deletes a single file from the backup device via the storage provider.
    /// </summary>
    /// <exception cref="FileNotProcessedException"></exception>
    protected void DeleteFileFromDevice(string fileName, string filePath, string longFileName, string versionDate, string fileType)
    {
        var kind = FileTypeKindExtensions.ParseFileTypeKind(fileType);

        // determine remote file name
        string remoteFile;
        if (kind.UsesLongFileNameStorage() && !string.IsNullOrEmpty(longFileName))
        {
            remoteFile = StoragePath.BuildLongFilePath(versionDate, longFileName);
        }
        else
        {
            remoteFile = Path.Combine(versionDate + filePath, fileName);
        }

        // delete file
        try
        {
            StoragePath.DeleteFromStorageByType(storage, kind, remoteFile);
        }
        catch (Exception ex)
        {
            throw new FileNotProcessedException(ex);
        }
    }

    /// <summary>
    /// Updates the database on the storage device. Storage must still be open.
    /// </summary>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    protected void UpdateDatabaseOnStorage()
    {
        try
        {
            if (int.TryParse(configurationManager.OldBackupPrevent, out var storageVersion))
            {
                storage.UpdateStorageVersion(storageVersion);
            }
            else
            {
                _logger.Warning("Stored backup version '{Version}' is not numeric; skipping storage version update.", configurationManager.OldBackupPrevent);
            }

            storage.UploadDatabaseFile(dbClientFactory.DatabaseFile);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database file could not be refreshed on storage device.");

            ReportState(JobState.ERROR);

            throw new DatabaseFileNotUpdatedException();
        }
    }

    /// <summary>
    /// Updates the free disk space on the database.
    /// </summary>
    protected async Task UpdateFreeDiskSpaceAsync()
    {
        try
        {
            configurationManager.FreeSpace = storage.GetFreeSpace().ToString();

            using var dbClient = dbClientFactory.CreateDbClient();
            configurationManager.BackupSize = (await versionQueryRepository.GetTotalBackupFileSizeAsync(dbClient)).ToString();
        }
        catch (Exception ex)
        {
            // not important
            _logger.Warning(ex, "Could not update free space variable due to exception.");
        }
    }
}

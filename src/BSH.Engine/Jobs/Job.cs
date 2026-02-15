// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for all job tasks
/// </summary>
public abstract class Job
{
    private static readonly ILogger _logger = Log.ForContext<Job>();

    protected readonly IStorageProvider storage;

    protected readonly IDbClientFactory dbClientFactory;

    protected readonly IQueryManager queryManager;

    protected readonly IConfigurationManager configurationManager;

    protected readonly bool silent;

    private readonly List<IJobReport> observers = new();

    public Collection<FileExceptionEntry> FileErrorList
    {
        get;
    }

    protected Job(IStorageProvider storage, IDbClientFactory dbClientFactory, IQueryManager queryManager, IConfigurationManager configurationManager, bool silent = false)
    {
        this.storage = storage;
        this.dbClientFactory = dbClientFactory;
        this.queryManager = queryManager;
        this.configurationManager = configurationManager;
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
        var fileExceptionEntry = new FileExceptionEntry()
        {
            Exception = ex,
            File = file,
            NewVersionDate = versionDate,
        };

        FileErrorList.Add(fileExceptionEntry);
        return fileExceptionEntry;
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
        var fileExceptionEntry = new FileExceptionEntry()
        {
            Exception = ex,
            File = file,
            NewVersionDate = versionDate,
            NewVersionId = versionId
        };

        FileErrorList.Add(fileExceptionEntry);
        return fileExceptionEntry;
    }

    /// <summary>
    /// Adds the given exception to the file exception list.
    /// </summary>
    /// <param name="file">The file that could not be copied.</param>
    /// <param name="ex">The exception that occured.</param>
    /// <returns></returns>
    protected FileExceptionEntry AddFileErrorToList(FileTableRow file, Exception ex)
    {
        var fileExceptionEntry = new FileExceptionEntry()
        {
            Exception = ex,
            File = file,
        };

        FileErrorList.Add(fileExceptionEntry);
        return fileExceptionEntry;
    }

    public void ReportState(JobState jobState)
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.ReportState(jobState);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    protected void ReportStatus(string title, string text)
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.ReportStatus(title, text);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    protected void ReportProgress(int total, int current)
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.ReportProgress(total, current);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    protected void ReportFileProgress(string file)
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.ReportFileProgress(file);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    protected void ReportExceptions(Collection<FileExceptionEntry> files)
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.ReportExceptions(files, this.silent);
            }
            catch
            {
                // ignore exception
            }
        }
    }

    protected async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        if (observers.Count > 0)
        {
            return await observers[0].RequestOverwrite(localFile, remoteFile);
        }

        return RequestOverwriteResult.Overwrite;
    }

    protected async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        foreach (var observer in observers)
        {
            try
            {
                await observer.RequestShowErrorInsufficientDiskSpaceAsync();
            }
            catch
            {
                // ignore exception
            }
        }
    }

    public void AddObserver(IJobReport observer)
    {
        try
        {
            observers.Add(observer);
        }
        catch
        {
            // ignore exception
        }
    }

    public void RemoveObserver(IJobReport observer)
    {
        try
        {
            observers.Remove(observer);
        }
        catch
        {
            // ignore exception
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
            storage.UpdateStorageVersion(int.Parse(configurationManager.OldBackupPrevent));
            storage.UploadDatabaseFile(dbClientFactory.DatabaseFile);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database file could not be refreshed on storage device.");

            ReportState(JobState.ERROR);

            // standby mode
            Win32Stuff.AllowSystemSleep();
            storage.Dispose();

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
            IVersionQueryRepository versionQueryRepository = new VersionQueryRepository();
            configurationManager.BackupSize = (await versionQueryRepository.GetTotalBackupFileSizeAsync(dbClient)).ToString();
        }
        catch (Exception ex)
        {
            // not important
            _logger.Warning(ex, "Could not update free space variable due to exception.");
        }
    }
}

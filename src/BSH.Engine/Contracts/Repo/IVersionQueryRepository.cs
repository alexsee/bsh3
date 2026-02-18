// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Contracts.Repo;

public interface IVersionQueryRepository
{
    Task<string> GetLastVersionDateAsync(DbClient dbClient);
    Task<int> CountRestoreFilesByPathAsync(DbClient dbClient, int versionId, string filePath);
    Task<DbDataReader> GetRestoreSingleFileAsync(DbClient dbClient, int versionId, string fileName, string filePath);
    Task<DbDataReader> GetRestoreFilesByPathAsync(DbClient dbClient, int versionId, string filePath);
    Task<DbDataReader> GetRestoreFoldersByPathAsync(DbClient dbClient, int versionId, string filePath);
    DataSet GetFilesToDeleteForVersion(DbClient dbClient, int versionId);
    Task<DbDataReader> GetOrphanedVersionDatesAsync(DbClient dbClient);
    Task<IReadOnlyList<int>> GetFileIdsForDeleteSingleAsync(DbClient dbClient, string fileFilter, string pathFilter);
    Task<DbDataReader> GetFileVersionsForDeleteSingleAsync(DbClient dbClient, int fileId);
    Task<int> CountEditableFilesAsync(DbClient dbClient);
    Task<DbDataReader> GetEditableFilesAsync(DbClient dbClient);
    Task<double> GetTotalBackupFileSizeAsync(DbClient dbClient);
}

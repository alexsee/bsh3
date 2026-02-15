// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Contracts.Repo;

public interface IBackupMutationRepository
{
    Task<long> CreateVersionAsync(DbClient dbClient, string newVersionDate, string title, string description, bool fullBackup, string sources);
    Task<long?> GetFileIdAsync(DbClient dbClient, string fileName, string filePath);
    Task<long> CreateFileAsync(DbClient dbClient, string fileName, string filePath);
    Task<long?> GetMatchingFileVersionIdAsync(DbClient dbClient, long fileId, double fileSize, DateTime fileDateModified);
    Task AddFileLinkAsync(DbClient dbClient, long fileVersionId, long versionId);
    Task<long> AddOrGetFolderIdAsync(DbClient dbClient, string folder);
    Task AddFolderLinkAsync(DbClient dbClient, long folderId, long versionId);
    Task AddFolderJunctionAsync(DbClient dbClient, string junctionPath, string displayName);
    Task AddFileVersionWithLinkAsync(DbClient dbClient, long fileId, double newVersionId, double fileSize, DateTime fileDateCreated, DateTime fileDateModified, int fileType, string longFileName);
    Task RenameVersionDateAsync(DbClient dbClient, long versionId, string newVersionDate);
    Task DeleteFileVersionAsync(DbClient dbClient, long fileVersionId);
    Task DeleteVersionMetadataAsync(DbClient dbClient, int versionId);
    Task DeleteSingleFileMetadataAsync(DbClient dbClient, string fileFilter, string pathFilter);
    Task UpdateFileVersionTypeAsync(DbClient dbClient, int fileVersionId, int fileType);
    Task SetVersionStableAsync(string version, bool stable);
    Task UpdateVersionDetailsAsync(int versionId, string title, string description);
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;

namespace BSH.Test.Fakes;

/// <summary>
/// Virtual default for <see cref="IQueryManager"/>. Subclasses override members a test observes
/// or that returned a different value.
/// </summary>
public class FakeQueryManager : IQueryManager
{
    public virtual Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);

    public virtual Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);

    public virtual string GetFileNameFromDrive(FileTableRow file) => file.FileName;

    public virtual Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult((fileName, false));

    public virtual Task<VersionChangeStatistics> GetVersionChangeStatisticsAsync(string versionId) => throw new NotSupportedException();

    public virtual Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult<FileDetails>(null);

    public virtual Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());

    public virtual Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());

    public virtual Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult(folder);

    public virtual Task<VersionDetails> GetLastBackupAsync() => Task.FromResult<VersionDetails>(null);

    public virtual Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult<VersionDetails>(null);

    public virtual Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);

    public virtual Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);

    public virtual Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);

    public virtual Task<int> GetNumberOfVersionsAsync() => Task.FromResult(0);

    public virtual Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);

    public virtual Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);

    public virtual Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult<VersionDetails>(null);

    public virtual Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult<VersionDetails>(null);

    public virtual List<VersionDetails> GetVersions(bool desc = true) => [];

    public virtual Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());

    public virtual Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());

    public virtual Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
}

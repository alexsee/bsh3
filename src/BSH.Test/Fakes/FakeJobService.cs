// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp.Contracts.Services;

namespace BSH.Test.Fakes;

/// <summary>
/// Virtual default for <see cref="IJobService"/>. Subclasses override members a test observes
/// or that returned a different value.
/// </summary>
public class FakeJobService : IJobService
{
    public virtual void Cancel() { }

    public virtual Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(true);

    public virtual Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, string sourceFolders = "") => Task.FromResult(true);

    public virtual Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;

    public virtual Task<JobSessionResult> DeleteBackupsAsync(List<string> versions, bool statusDialog = true) => Task.FromResult(new JobSessionResult { Started = true });

    public virtual Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true, IReadOnlyList<int>? versionIds = null) => Task.CompletedTask;
    public virtual Task DeleteSelectedFilesAsync(IReadOnlyList<(string FileFilter, string PathFilter)> targets, IReadOnlyList<int>? versionIds) => Task.CompletedTask;

    public virtual Task<bool> RequestPassword() => Task.FromResult(true);

    public virtual Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;

    public virtual Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;

    public virtual Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
}

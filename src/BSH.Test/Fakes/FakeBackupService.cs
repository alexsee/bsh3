// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;

namespace BSH.Test.Fakes;

/// <summary>
/// Virtual default for <see cref="IBackupService"/>. Subclasses override members a test observes
/// or that returned a different value.
/// </summary>
public class FakeBackupService : IBackupService
{
    public virtual Task<bool> CheckMedia(bool quickCheck = false) => Task.FromResult(true);

    public virtual string GetPassword() => string.Empty;

    public virtual bool HasPassword() => false;

    public virtual void SetPassword(string password) { }

    public virtual Task SetStableAsync(string version, bool stable) => Task.CompletedTask;

    public virtual Task UpdateVersionAsync(string version, VersionDetails versionDetails) => Task.CompletedTask;

    public virtual Task StartBackup(string title, string description, IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false) => Task.CompletedTask;

    public virtual Task StartDelete(string version, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;

    public virtual Task StartDeleteSingle(string fileFilter, string pathFilter, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false, IReadOnlyList<int> versionIds = null) => Task.CompletedTask;

    public virtual Task StartEdit(IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;

    public virtual Task StartRestore(string version, string file, string destination, IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => Task.CompletedTask;

    public virtual void UpdateDatabaseFile(string databaseFile) { }
}

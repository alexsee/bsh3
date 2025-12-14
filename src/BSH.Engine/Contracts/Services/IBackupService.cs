// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Contracts.Services;

public interface IBackupService
{
    Task<bool> CheckMedia(bool quickCheck = false);
    string GetPassword();
    bool HasPassword();
    void SetPassword(string password);
    Task SetStableAsync(string version, bool stable);
    Task UpdateVersionAsync(string version, VersionDetails versionDetails);
    Task StartBackup(string title, string description, ref IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false);
    Task StartDelete(string version, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false);
    Task StartDeleteSingle(string fileFilter, string pathFilter, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false);
    Task StartEdit(ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false);
    Task StartRestore(string version, string file, string destination, ref IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false);
    void UpdateDatabaseFile(string databaseFile);
}
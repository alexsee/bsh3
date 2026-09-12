// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using BSH.Test.Helpers;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Covers the hardened <see cref="Job"/> observer and storage-version paths.
/// </summary>
public class JobTests
{
    [Test]
    public async Task AddObserverAndRemoveObserverRejectNull()
    {
        await using var context = await EngineJobTestContext.CreateAsync("job-observers");
        using var job = CreateTestJob(context);

        Assert.Throws<ArgumentNullException>(() => job.AddObserver(null!));
        Assert.Throws<ArgumentNullException>(() => job.RemoveObserver(null!));
    }

    [Test]
    public async Task FailingObserverDoesNotBreakDiskSpaceNotification()
    {
        await using var context = await EngineJobTestContext.CreateAsync("job-observer-errors");
        using var job = CreateTestJob(context);
        job.AddObserver(new ThrowingJobReport());

        Assert.DoesNotThrowAsync(async () => await job.RequestDiskSpaceErrorAsync());
    }

    [Test]
    public async Task NonNumericStorageVersionSkipsStorageUpdateButUploadsDatabase()
    {
        await using var context = await EngineJobTestContext.CreateAsync("job-storage-version");
        context.ConfigurationManager.OldBackupPrevent = "not-a-version";

        var storage = new StorageMock();
        using var job = CreateTestJob(context, storage);

        job.UpdateDatabase();

        Assert.That(storage.UploadDatabaseFileCalls, Is.EqualTo(1));
    }

    private static TestJob CreateTestJob(EngineJobTestContext context, StorageMock? storage = null)
    {
        return new TestJob(
            storage ?? new StorageMock(),
            context.DbFactory,
            context.QueryManager,
            context.ConfigurationManager,
            context.VersionQueryRepository);
    }

    private sealed class TestJob : Job
    {
        public TestJob(
            IStorageProvider storage,
            IDbClientFactory dbClientFactory,
            IQueryManager queryManager,
            IConfigurationManager configurationManager,
            IVersionQueryRepository versionQueryRepository)
            : base(storage, dbClientFactory, queryManager, configurationManager, versionQueryRepository)
        {
        }

        public Task RequestDiskSpaceErrorAsync() => RequestShowErrorInsufficientDiskSpaceAsync();

        public void UpdateDatabase() => UpdateDatabaseOnStorage();
    }

    private sealed class ThrowingJobReport : IJobReport
    {
        public void ReportAction(ActionType action, bool silent) => throw new InvalidOperationException("observer failed");
        public void ReportState(JobState jobState) => throw new InvalidOperationException("observer failed");
        public void ReportStatus(string title, string text) => throw new InvalidOperationException("observer failed");
        public void ReportProgress(int total, int current) => throw new InvalidOperationException("observer failed");
        public void ReportFileProgress(string file) => throw new InvalidOperationException("observer failed");
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) => throw new InvalidOperationException("observer failed");

        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) => throw new InvalidOperationException("observer failed");

        public Task RequestShowErrorInsufficientDiskSpaceAsync() => throw new InvalidOperationException("observer failed");
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;

namespace BSH.Test.Helpers;

/// <summary>
/// Lightweight <see cref="IJobReport"/> for asserting job state and overwrite prompts.
/// </summary>
public sealed class JobReportStub : IJobReport
{
    public RequestOverwriteResult OverwriteResult { get; set; } = RequestOverwriteResult.Overwrite;

    public int RequestOverwriteCalls { get; private set; }

    public int InsufficientDiskSpaceCalls { get; private set; }

    public List<JobState> ReportedStates { get; } = [];

    public List<(string Title, string Text)> ReportedStatuses { get; } = [];

    public void ReportAction(ActionType action, bool silent) { }

    public void ReportState(JobState jobState) => ReportedStates.Add(jobState);

    public void ReportStatus(string title, string text) => ReportedStatuses.Add((title, text));

    public void ReportProgress(int total, int current) { }

    public void ReportFileProgress(string file) { }

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) { }

    public Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        InsufficientDiskSpaceCalls++;
        return Task.CompletedTask;
    }

    public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        RequestOverwriteCalls++;
        return Task.FromResult(OverwriteResult);
    }
}

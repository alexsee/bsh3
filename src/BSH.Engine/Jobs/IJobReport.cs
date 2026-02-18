// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime.Ports;

namespace Brightbits.BSH.Engine.Jobs;

public interface IJobReport : IJobRuntimeEvents
{
}

public class ForwardJobReport : IJobReport
{
    private readonly IJobReport report;
    private List<FileExceptionEntry> files = new();

    public ForwardJobReport(IJobReport report)
    {
        this.report = report;
    }

    public void ReportAction(ActionType action, bool silent)
    {
        // not used
    }

    public void ReportState(JobState jobState)
    {
        // not used
    }

    public void ReportStatus(string title, string text)
    {
        this.report.ReportStatus(title, text);
    }

    public void ReportProgress(int total, int current)
    {
        // not used
    }

    public void ReportFileProgress(string file)
    {
        this.report.ReportFileProgress(file);
    }

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
    {
        this.files.AddRange(files);
    }

    public async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        return await this.report.RequestOverwrite(localFile, remoteFile);
    }

    public async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        await this.report.RequestShowErrorInsufficientDiskSpaceAsync();
    }

    public void ForwardExceptions(bool silent)
    {
        this.report.ReportExceptions(new Collection<FileExceptionEntry>(this.files), silent);
    }
}

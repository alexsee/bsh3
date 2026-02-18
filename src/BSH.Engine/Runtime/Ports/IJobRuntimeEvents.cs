// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Runtime.Ports;

public interface IJobRuntimeEvents
{
    void ReportAction(ActionType action, bool silent);

    void ReportState(JobState jobState);

    void ReportStatus(string title, string text);

    void ReportProgress(int total, int current);

    void ReportFileProgress(string file);

    void ReportExceptions(Collection<FileExceptionEntry> files, bool silent);

    Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile);

    Task RequestShowErrorInsufficientDiskSpaceAsync();
}

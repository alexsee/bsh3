// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Jobs;

public interface IJobReport
{
    void ReportAction(ActionType action, bool silent);

    void ReportState(JobState jobState);

    void ReportStatus(string title, string text);

    void ReportProgress(int total, int current);

    void ReportFileProgress(string file);

    void ReportExceptions(Collection<FileExceptionEntry> files, bool silent);

    RequestOverwriteResult RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile);

    void RequestShowErrorInsufficientDiskSpace();
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
        // not used
    }

    public void ReportProgress(int total, int current)
    {
        // not used
    }

    public void ReportFileProgress(string file)
    {
        // not used
    }

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
    {
        this.files.AddRange(files);
    }

    public RequestOverwriteResult RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        return this.report.RequestOverwrite(localFile, remoteFile);
    }

    public void RequestShowErrorInsufficientDiskSpace()
    {
        this.report.RequestShowErrorInsufficientDiskSpace();
    }

    public void ForwardExceptions(bool silent)
    {
        this.report.ReportExceptions(new Collection<FileExceptionEntry>(this.files), silent);
    }
}
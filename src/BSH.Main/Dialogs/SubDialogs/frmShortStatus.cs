// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.Main.Properties;
using Humanizer;

namespace Brightbits.BSH.Main;

public partial class frmShortStatus : IJobReport
{
    public frmShortStatus()
    {
        InitializeComponent();
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
        if (!IsHandleCreated)
        {
            return;
        }

        Invoke(new Action(() => lblStatus.Text = title));
    }

    public void ReportProgress(int total, int current)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        Invoke(new Action(() =>
        {
            pbarStatus.Maximum = total;
            pbarStatus.Value = current;
        }));
    }

    private DateTime lastTimeRefreshed = DateTime.Now;

    public void ReportFileProgress(string file)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        if (DateTime.Now - lastTimeRefreshed < TimeSpan.FromMilliseconds(100d))
        {
            return;
        }

        lastTimeRefreshed = DateTime.Now;
        Invoke(new Action(() =>
        {
            if (string.IsNullOrEmpty(file))
            {
                lblStatus.Text = Resources.DLG_SHORT_STATUS_DEFAULT_STATUS_TEXT;
            }
            else
            {
                lblStatus.Text = Resources.DLG_SHORT_STATUS_FILES_STATUS_TEXT.FormatWith(System.IO.Path.GetFileName(file));
            }
        }));
    }

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
    {
        // not used
    }

    public async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        return RequestOverwriteResult.NoOverwriteAll;
    }

    public async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        // not used
    }
}
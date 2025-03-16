// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;

namespace Brightbits.BSH.Main;

public interface IStatusReport
{
    void ReportAction(ActionType action, bool silent);

    void ReportState(JobState jobState);

    void ReportStatus(string title, string text);

    void ReportProgress(int total, int current);

    void ReportFileProgress(string file);

    void ReportSystemStatus(SystemStatus systemStatus);
}
// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.WinUI;

namespace BSH.MainApp.Models;

public sealed record BrowserVersionChangeStatistics(long Added, long Modified, long Deleted)
{
    public string AddedCount => $"+{Added:N0}";
    public string ModifiedCount => $"~{Modified:N0}";
    public string DeletedCount => $"−{Deleted:N0}";

    public string AddedText => string.Format("Browser_Version_AddedFiles".GetLocalized() ?? "+{0:N0} added files", Added);
    public string ModifiedText => string.Format("Browser_Version_ModifiedFiles".GetLocalized() ?? "~{0:N0} modified files", Modified);
    public string DeletedText => string.Format("Browser_Version_DeletedFiles".GetLocalized() ?? "−{0:N0} deleted files", Deleted);
}

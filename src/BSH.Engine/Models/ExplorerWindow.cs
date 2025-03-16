// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Models;

public class ExplorerWindow
{
    public string Path
    {
        get; set;
    }

    public string WindowTitle
    {
        get; set;
    }

    public ExplorerWindow(string path, string windowTitle)
    {
        Path = path;
        WindowTitle = windowTitle;
    }
}

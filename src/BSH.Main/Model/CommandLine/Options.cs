// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommandLine;

namespace BSH.Main.Model.CommandLine;

public class Options
{
    [Option("delayedstart", Default = false)]
    public bool DelayedStart
    {
        get; set;
    }

    [Option("databasefile")]
    public string DatabaseFile
    {
        get; set;
    }

    [Option("config")]
    public bool ShowConfig
    {
        get; set;
    }

    [Option("browser")]
    public bool ShowBrowser
    {
        get; set;
    }
}

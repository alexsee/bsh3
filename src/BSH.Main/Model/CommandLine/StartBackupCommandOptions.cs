// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommandLine;

namespace BSH.Main.Model.CommandLine;

[Verb("startbackup", HelpText = "Starts a manual backup with default configuration.")]
public class StartBackupCommandOptions
{
    [Option("title", Default = "Manuelle Sicherung", HelpText = "Specifies the title of the backup.")]
    public string Title
    {
        get; set;
    }

    [Option("description", Default = "", HelpText = "Specifies the description of the backup.")]
    public string Description
    {
        get; set;
    }

    [Option("shutdownapp", Default = false)]
    public bool ShutdownApp
    {
        get; set;
    }

    [Option("shutdownpc", Default = false)]
    public bool ShutdownPC
    {
        get; set;
    }

    [Option("autodelete", Default = false)]
    public bool AutoDeletion
    {
        get; set;
    }
}

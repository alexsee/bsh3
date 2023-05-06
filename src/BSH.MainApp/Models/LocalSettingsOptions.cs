// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Models;

public class LocalSettingsOptions
{
    public string? ApplicationDataFolder
    {
        get; set;
    }

    public string? LocalSettingsFile
    {
        get; set;
    }
}
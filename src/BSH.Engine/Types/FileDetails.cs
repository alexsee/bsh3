// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace Brightbits.BSH.Engine.Types;

public class FileDetails
{
    public string Name
    {
        get; set;
    }

    public string RestorePath
    {
        get; set;
    }

    public string Type
    {
        get; set;
    }

    public double Size
    {
        get; set;
    }

    public DateTime Created
    {
        get; set;
    }

    public DateTime Modified
    {
        get; set;
    }

    public IReadOnlyList<VersionDetails> AvailableVersions
    {
        get; set;
    } = [];
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Types;

public class VersionDetails
{
    public string Id
    {
        get; set;
    }

    public DateTime CreationDate
    {
        get; set;
    }

    public string Title
    {
        get; set;
    }

    public string Description
    {
        get; set;
    }

    public bool Stable
    {
        get; set;
    }

    public string Sources
    {
        get; set;
    }

    public long Size
    {
        get; set;
    }
}

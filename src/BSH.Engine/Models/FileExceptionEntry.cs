// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Models;

public class FileExceptionEntry
{
    public Exception Exception
    {
        get; set;
    }

    public FileTableRow File
    {
        get; set;
    }

    public long NewVersionId
    {
        get; set;
    }

    public string NewVersionDate
    {
        get; set;
    }
}

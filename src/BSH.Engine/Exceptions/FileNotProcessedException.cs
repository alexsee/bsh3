// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Exceptions;

public class FileNotProcessedException : Exception
{
    public bool RequestCancel
    {
        get; set;
    }

    public FileNotProcessedException() : base() { }

    public FileNotProcessedException(Exception ex, bool cancel = false) : base($"Datei konnte nicht von/nach Backupmedium kopiert werden ({ex.Message}).", ex)
    {
        this.RequestCancel = cancel;
    }
}

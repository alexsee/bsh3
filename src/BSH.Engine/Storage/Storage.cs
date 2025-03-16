// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Storage;

public abstract class Storage
{
    protected static string CleanRemoteFileName(string remoteFile)
    {
        ArgumentNullException.ThrowIfNull(remoteFile);

        var result = remoteFile;

        if (remoteFile.StartsWith('\\'))
        {
            if (remoteFile.Length > 1)
            {
                result = remoteFile.Substring(1);
            }
            else
            {
                return "";
            }
        }

        return result;
    }

    protected static string GetLocalFileName(string localFile)
    {
        ArgumentNullException.ThrowIfNull(localFile);

        if (localFile.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
        {
            return localFile;
        }

        return "\\\\?\\" + localFile;
    }
}

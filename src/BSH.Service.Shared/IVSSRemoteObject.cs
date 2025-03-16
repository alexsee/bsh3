// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace BSH.Service.Shared
{
    public interface IVSSRemoteObject
    {
        bool CopyFileWithVSS(string vssServiceFolder, string source, string destination);

        Exception GetException();
    }
}

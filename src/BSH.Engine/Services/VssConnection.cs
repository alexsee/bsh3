// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.Service.Shared;

namespace Brightbits.BSH.Engine.Services;

public static class VssConnection
{
    public const string StablePipeName = VssPipeNames.Stable;
    public const string PreviewPipeName = VssPipeNames.Preview;

    public static string ResolveInstalledPipe(string directory)
    {
        return VssPipeNames.ReadInstalledPipeName(directory);
    }
}

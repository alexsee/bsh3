// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Models;

public sealed record BackupSpaceCheckResult(long EstimatedRequiredSpace, long AvailableSpace)
{
    public bool ShouldWarn => AvailableSpace > 0 && EstimatedRequiredSpace > AvailableSpace;
}

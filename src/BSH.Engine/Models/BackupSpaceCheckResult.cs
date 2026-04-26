// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Models;

public sealed class BackupSpaceCheckResult
{
    public BackupSpaceCheckResult(long estimatedRequiredSpace, long availableSpace)
    {
        EstimatedRequiredSpace = estimatedRequiredSpace;
        AvailableSpace = availableSpace;
    }

    public long EstimatedRequiredSpace { get; }

    public long AvailableSpace { get; }

    public bool HasMeaningfulFreeSpace => AvailableSpace > 0;

    public bool ShouldWarn => HasMeaningfulFreeSpace && EstimatedRequiredSpace > AvailableSpace;
}

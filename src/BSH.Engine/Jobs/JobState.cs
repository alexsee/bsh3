// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Jobs;

public enum JobState
{
    NOT_STARTED,
    RUNNING,
    CANCELED,
    ERROR,
    FINISHED
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine;

/// <summary>
/// Backup target shape used by setup, settings, and media-switch flows.
/// Distinct from <see cref="MediaType"/> (storage backend): UNC is still
/// <see cref="MediaType.LocalDevice"/> with credentials.
/// </summary>
public enum MediaTargetKind
{
    LocalDrive,
    Unc,
    Ftp
}

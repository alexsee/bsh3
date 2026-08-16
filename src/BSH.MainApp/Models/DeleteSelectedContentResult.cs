// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace BSH.MainApp.Models;

/// <summary>
/// Result of the delete-selected-content confirmation dialog.
/// </summary>
public sealed class DeleteSelectedContentResult
{
    public static DeleteSelectedContentResult Cancelled { get; } = new() { IsCancelled = true };

    public static DeleteSelectedContentResult AllVersions { get; } = new() { DeleteFromAllVersions = true };

    public static DeleteSelectedContentResult FromVersions(IReadOnlyList<string> versionIds)
    {
        ArgumentNullException.ThrowIfNull(versionIds);
        return new DeleteSelectedContentResult
        {
            VersionIds = versionIds
        };
    }

    public bool IsCancelled { get; init; }

    public bool DeleteFromAllVersions { get; init; }

    public IReadOnlyList<string> VersionIds { get; init; } = Array.Empty<string>();
}

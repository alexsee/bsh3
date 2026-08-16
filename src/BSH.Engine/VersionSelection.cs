// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine;

/// <summary>
/// Helpers for resolving backup version IDs for range-scoped operations.
/// </summary>
public static class VersionSelection
{
    /// <summary>
    /// Returns the IDs of the <paramref name="count"/> most recent versions (by creation date).
    /// </summary>
    public static IReadOnlyList<int> SelectLastN(IReadOnlyList<VersionDetails> versions, int count)
    {
        if (versions == null || versions.Count == 0 || count <= 0)
        {
            return Array.Empty<int>();
        }

        return versions
            .OrderByDescending(v => v.CreationDate)
            .ThenByDescending(v => int.TryParse(v.Id, out var id) ? id : 0)
            .Take(count)
            .Select(ParseVersionId)
            .Where(id => id > 0)
            .ToList();
    }

    /// <summary>
    /// Returns the IDs of versions whose creation date is on or after <paramref name="since"/>.
    /// </summary>
    public static IReadOnlyList<int> SelectSinceDate(IReadOnlyList<VersionDetails> versions, DateTime since)
    {
        if (versions == null || versions.Count == 0)
        {
            return Array.Empty<int>();
        }

        return versions
            .Where(v => v.CreationDate >= since)
            .OrderBy(v => v.CreationDate)
            .ThenBy(v => int.TryParse(v.Id, out var id) ? id : 0)
            .Select(ParseVersionId)
            .Where(id => id > 0)
            .ToList();
    }

    private static int ParseVersionId(VersionDetails version)
    {
        return int.TryParse(version.Id, out var id) ? id : 0;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine;

/// <summary>
/// Resolves which backup versions a single-file delete should target.
/// </summary>
public enum DeleteSingleScopeMode
{
    AllVersions,
    LastN,
    LastDays,
    SelectedVersions
}

/// <summary>
/// Result of resolving a delete-single scope.
/// </summary>
public sealed class DeleteSingleScopeResult
{
    public static DeleteSingleScopeResult AllVersions { get; } = new() { DeleteFromAllVersions = true };

    public static DeleteSingleScopeResult None { get; } = new();

    public static DeleteSingleScopeResult FromVersionIds(IReadOnlyList<int> versionIds)
    {
        ArgumentNullException.ThrowIfNull(versionIds);
        return new DeleteSingleScopeResult { VersionIds = versionIds };
    }

    public bool DeleteFromAllVersions { get; init; }

    public IReadOnlyList<int> VersionIds { get; init; } = Array.Empty<int>();

    public bool HasTargetVersions => DeleteFromAllVersions || VersionIds.Count > 0;
}

/// <summary>
/// Shared scope resolution for WinUI and WinForms single-file delete dialogs.
/// </summary>
public static class DeleteSingleScope
{
    public static DeleteSingleScopeResult Resolve(
        DeleteSingleScopeMode mode,
        IReadOnlyList<VersionDetails> versions,
        int lastN,
        int lastDays,
        IReadOnlyList<string> selectedVersionIds,
        DateTime? now = null)
    {
        versions ??= Array.Empty<VersionDetails>();
        selectedVersionIds ??= Array.Empty<string>();

        switch (mode)
        {
            case DeleteSingleScopeMode.AllVersions:
                return DeleteSingleScopeResult.AllVersions;

            case DeleteSingleScopeMode.LastN:
                {
                    var ids = VersionSelection.SelectLastN(versions, Math.Max(1, lastN));
                    return ids.Count == 0 ? DeleteSingleScopeResult.None : DeleteSingleScopeResult.FromVersionIds(ids);
                }

            case DeleteSingleScopeMode.LastDays:
                {
                    var days = Math.Max(1, lastDays);
                    var since = (now ?? DateTime.Now).Date.AddDays(-(days - 1));
                    var ids = VersionSelection.SelectSinceDate(versions, since);
                    return ids.Count == 0 ? DeleteSingleScopeResult.None : DeleteSingleScopeResult.FromVersionIds(ids);
                }

            case DeleteSingleScopeMode.SelectedVersions:
                {
                    var ids = selectedVersionIds
                        .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList();
                    return ids.Count == 0 ? DeleteSingleScopeResult.None : DeleteSingleScopeResult.FromVersionIds(ids);
                }

            default:
                return DeleteSingleScopeResult.None;
        }
    }
}

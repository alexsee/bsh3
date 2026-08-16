// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine;

/// <summary>
/// Resolves candidate backup versions for a single-file/folder delete prompt.
/// </summary>
public static class DeleteSingleCandidateVersions
{
    /// <summary>
    /// For files, prefers versions that contain the file; otherwise returns <paramref name="allVersions"/>.
    /// </summary>
    public static async Task<IReadOnlyList<VersionDetails>> ResolveAsync(
        IQueryManager queryManager,
        IReadOnlyList<VersionDetails> allVersions,
        string currentVersionId,
        string fileName,
        string filePath,
        bool isFile)
    {
        allVersions ??= [];

        if (!isFile || queryManager == null || string.IsNullOrEmpty(currentVersionId))
        {
            return allVersions;
        }

        var details = await queryManager.GetFileDetailsAsync(currentVersionId, fileName, filePath);
        if (details?.AvailableVersions is { Count: > 0 })
        {
            return details.AvailableVersions.ToList();
        }

        return allVersions;
    }
}

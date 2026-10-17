// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

namespace BSH.Service.Shared;

/// <summary>
/// Named-pipe names for the VSS helper. Stable and Preview install side by side
/// and must not listen on the same pipe.
/// </summary>
public static class VssPipeNames
{
    public const string Stable = "backupservicehome";
    public const string Preview = "backupservicehome-preview";
    public const string PipeFileName = "vss.pipe";

    public static string Normalize(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return Stable;
        }

        var trimmed = candidate.Trim();
        if (trimmed.Length is 0 or > 80)
        {
            return Stable;
        }

        foreach (var character in trimmed)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '-')
            {
                return Stable;
            }
        }

        return trimmed;
    }

    /// <summary>
    /// Reads an optional pipe override installed next to the service or the app.
    /// A missing file keeps the stable pipe so the WinForms product is unchanged.
    /// </summary>
    public static string ReadInstalledPipeName(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Stable;
        }

        try
        {
            var path = Path.Combine(directory, PipeFileName);
            if (!File.Exists(path))
            {
                return Stable;
            }

            return Normalize(File.ReadAllText(path));
        }
        catch (IOException)
        {
            return Stable;
        }
        catch (UnauthorizedAccessException)
        {
            return Stable;
        }
    }
}

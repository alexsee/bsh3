// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Reflection;

namespace BSH.MainApp.Services;

public static class ApplicationVersionInfo
{
    public static string GetDisplayVersion(Assembly assembly)
    {
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        var assemblyVersion = assembly.GetName().Version?.ToString();

        return GetDisplayVersion(informationalVersion, assemblyVersion);
    }

    public static string GetDisplayVersion(string? informationalVersion, string? assemblyVersion)
    {
        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            var buildMetadataIndex = informationalVersion.IndexOf('+', StringComparison.Ordinal);
            return buildMetadataIndex >= 0
                ? informationalVersion[..buildMetadataIndex]
                : informationalVersion;
        }

        return string.IsNullOrWhiteSpace(assemblyVersion) ? "unknown" : assemblyVersion;
    }
}


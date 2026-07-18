// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Helpers;

public static class CompressionExclusionFormatter
{
    public static IEnumerable<string> Parse(string? value)
    {
        return (value ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeExtension)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    public static string Format(IEnumerable<string> extensions)
    {
        return string.Join("|", extensions.Select(NormalizeExtension).Where(x => !string.IsNullOrEmpty(x)));
    }

    public static string NormalizeExtension(string? extension)
    {
        var normalized = (extension ?? string.Empty).Trim().TrimStart('*').TrimStart('.').ToLowerInvariant();
        return string.IsNullOrEmpty(normalized) ? string.Empty : "." + normalized;
    }
}

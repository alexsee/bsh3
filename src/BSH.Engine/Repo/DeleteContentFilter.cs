// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Repo;

internal static class DeleteContentFilter
{
    public static (string Predicate, (string, object)[] Parameters) ForFiles(string fileName, string path)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            return ("fileName = @fileName AND filePath = @filePath", [("fileName", fileName), ("filePath", path)]);
        }

        ArgumentNullException.ThrowIfNull(path);
        var relativePath = path.Trim('\\');
        var prefix = relativePath.Length == 0 ? "\\" : "\\" + relativePath + "\\";
        return (MatchDirectoryPrefix("filePath"), [("folderPrefix", prefix)]);
    }

    // Allow historical folder rows without a trailing separator.
    public static string ForFolders => MatchDirectoryPrefix("folder || char(92)");

    private static string MatchDirectoryPrefix(string pathExpression) => $"instr({pathExpression}, @folderPrefix) = 1";
}

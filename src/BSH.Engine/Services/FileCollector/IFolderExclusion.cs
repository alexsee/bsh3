// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Text.RegularExpressions;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Utils;
using Serilog;

namespace Brightbits.BSH.Engine.Services.FileCollector;

public interface IFolderExclusion
{
    bool IsFolderFiltered(string root, DirectoryInfo directory);
}

public class PathFolderExclusion(IConfigurationManager configurationManager) : IFolderExclusion
{
    private static readonly ILogger _logger = Log.ForContext<PathFolderExclusion>();

    public bool IsFolderFiltered(string root, DirectoryInfo directory)
    {
        if (string.IsNullOrEmpty(configurationManager.ExcludeFolder))
        {
            return false;
        }

        var excludeFolders = configurationManager.ExcludeFolder.Split('|');

        foreach (var entry in excludeFolders)
        {
            // check if source folder (for drive backup)
            if (("\\" + Path.GetFileName(directory.FullName) + "\\").StartsWith(entry + "\\", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("{folderName} was ignored due to root folder path filter.", directory.FullName);
                return true;
            }

            if (("\\" + Path.Combine(Path.GetFileName(root), IOUtils.GetRelativeFolder(directory.FullName, root)) + "\\").StartsWith(entry + "\\", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("{folderName} was ignored due to folder path filter.", directory.FullName);
                return true;
            }
        }

        return false;
    }
}

public class MaskFolderExclusion(IConfigurationManager configurationManager) : IFolderExclusion
{
    private static readonly ILogger _logger = Log.ForContext<MaskFolderExclusion>();
    private Regex _regexExcludeCache;

    public bool IsFolderFiltered(string root, DirectoryInfo directory)
    {
        if (string.IsNullOrEmpty(configurationManager.ExcludeMask))
        {
            return false;
        }

        _regexExcludeCache ??= new Regex(configurationManager.ExcludeMask, RegexOptions.Compiled & RegexOptions.Singleline, TimeSpan.FromSeconds(10));

        if (_regexExcludeCache.IsMatch(directory.FullName))
        {
            _logger.Debug("{folderName} was ignored due to regex path filter.", directory.FullName);
            return true;
        }

        return false;
    }
}

public class ReparsePointFolderExclusion() : IFolderExclusion
{
    public bool IsFolderFiltered(string root, DirectoryInfo directory) => (directory.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
}

public class SystemFolderExclusion() : IFolderExclusion
{
    public bool IsFolderFiltered(string root, DirectoryInfo directory) => (directory.Attributes & FileAttributes.System) == FileAttributes.System;
}

public class TemporaryFolderExclusion() : IFolderExclusion
{
    public bool IsFolderFiltered(string root, DirectoryInfo directory) => (directory.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary;
}

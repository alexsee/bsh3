// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Utils;
using Serilog;

namespace Brightbits.BSH.Engine.Services.FileCollector;

public class FileCollectorService : IFileCollectorService
{
    private const string InaccessibleDirectoryMessage = "Directory {Directory} could not be accessed.";

    public List<IFileExclusion> FileExclusionHandlers
    {
        get;
        set;
    } = [];

    public List<IFolderExclusion> FolderExclusionHandlers
    {
        get;
        set;
    } = [];

    public List<FolderTableRow> EmptyFolders
    {
        get; set;
    }

    public List<FileTableRow> GetLocalFileList(string root, bool subFolders = true)
    {
        var result = new List<FileTableRow>();
        EmptyFolders = new List<FolderTableRow>();

        SeekFiles(new DirectoryInfo(root), result, subFolders, root);

        return result;
    }

    private void SeekFiles(DirectoryInfo directory, List<FileTableRow> fileArray, bool subFolders, string baseRoot)
    {
        try
        {
            var fileCount = CollectFiles(directory, fileArray, baseRoot);

            // search subfolders?
            if (!subFolders)
            {
                return;
            }

            // scan subfolders
            var folders = directory.GetDirectories();

            // empty folder?
            if (fileCount <= 0 && folders.Length <= 0)
            {
                var f = new FolderTableRow(directory.FullName, baseRoot);
                EmptyFolders.Add(f);
            }

            foreach (var folder in folders)
            {
                SeekSubFolder(folder, fileArray, subFolders, baseRoot);
            }
        }
        catch (IOException ex)
        {
            Log.Warning(ex, InaccessibleDirectoryMessage, directory);
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning(ex, InaccessibleDirectoryMessage, directory);
        }
    }

    private int CollectFiles(DirectoryInfo directory, List<FileTableRow> fileArray, string baseRoot)
    {
        // get files
        var files = directory.GetFiles();
        foreach (var fileEntry in files)
        {
            var file = new FileTableRow()
            {
                FileName = fileEntry.Name,
                FilePath = IOUtils.GetRelativeFolder(fileEntry.DirectoryName, baseRoot),
                FileRoot = baseRoot,
                FileDateCreated = fileEntry.CreationTimeUtc,
                FileDateModified = fileEntry.LastWriteTimeUtc,
                FileSize = fileEntry.Length,
            };

            if (FileExclusionHandlers.Any(handler => handler.IsFileExcluded(file)))
            {
                continue;
            }

            fileArray.Add(file);
        }

        return files.Length;
    }

    private void SeekSubFolder(DirectoryInfo folder, List<FileTableRow> fileArray, bool subFolders, string baseRoot)
    {
        try
        {
            if (FolderExclusionHandlers.Any(handler => handler.IsFolderFiltered(baseRoot, folder)))
            {
                return;
            }

            SeekFiles(folder, fileArray, subFolders, baseRoot);
        }
        catch (IOException ex)
        {
            Log.Warning(ex, InaccessibleDirectoryMessage, folder);
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning(ex, InaccessibleDirectoryMessage, folder);
        }
    }
}

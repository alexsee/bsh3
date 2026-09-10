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

            // search subfolders?
            if (!subFolders)
            {
                return;
            }

            // scan subfolders
            var folders = directory.GetDirectories();

            // empty folder?
            if (files.Length <= 0 && folders.Length <= 0)
            {
                var f = new FolderTableRow(directory.FullName, baseRoot);
                EmptyFolders.Add(f);
            }

            foreach (var folder in folders)
            {
                try
                {
                    if (FolderExclusionHandlers.Any(handler => handler.IsFolderFiltered(baseRoot, folder)))
                    {
                        continue;
                    }

                    SeekFiles(folder, fileArray, subFolders, baseRoot);
                }
                catch (IOException ex)
                {
                    Log.Warning(ex, "Directory {Directory} could not be accessed.", folder);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warning(ex, "Directory {Directory} could not be accessed.", folder);
                }
            }
        }
        catch (IOException ex)
        {
            Log.Warning(ex, "Directory {Directory} could not be accessed.", directory);
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning(ex, "Directory {Directory} could not be accessed.", directory);
        }
    }
}

// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Brightbits.BSH.Engine.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Brightbits.BSH.Engine.Services
{
    public class FileCollectorService
    {
        private static readonly ILogger _logger = Log.ForContext<FileCollectorService>();

        private readonly QueryManager queryManager;

        private Regex _regexExcludeCache = null;

        private string root;

        private readonly string appDataFolder;

        public List<FolderTableRow> EmptyFolders { get; set; }

        public FileCollectorService(QueryManager queryManager)
        {
            this.queryManager = queryManager;
            this.appDataFolder = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Alexosoft\Backup Service Home 3\").ToLower();
        }

        public List<FileTableRow> GetLocalFileList(string root, bool subFolders = true)
        {
            this.root = root;

            var result = new List<FileTableRow>();
            EmptyFolders = new List<FolderTableRow>();

            SeekFiles(new DirectoryInfo(root), result, subFolders);

            return result;
        }

        private void SeekFiles(DirectoryInfo root, List<FileTableRow> fileArray, bool subFolders)
        {
            try
            {
                // get files
                var files = root.GetFiles();
                foreach (var fileEntry in files)
                {
                    var file = new FileTableRow()
                    {
                        FileName = fileEntry.Name,
                        FilePath = GetRelativeFolder(fileEntry.DirectoryName, this.root),
                        FileRoot = this.root,
                        FileDateCreated = fileEntry.CreationTimeUtc,
                        FileDateModified = fileEntry.LastWriteTimeUtc,
                        FileSize = fileEntry.Length,
                    };

                    if (IsFileFiltered(file))
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
                var folders = root.GetDirectories();

                // empty folder?
                if (files.Length <= 0 && folders.Length <= 0)
                {
                    var f = new FolderTableRow(root.FullName, this.root);
                    EmptyFolders.Add(f);
                }

                foreach (var folder in folders)
                {
                    var f = new FolderTableRow(folder.FullName, this.root);

                    if (IsFolderFiltered(f))
                    {
                        continue;
                    }

                    try
                    {
                        if ((folder.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint &&
                                    (folder.Attributes & FileAttributes.System) != FileAttributes.System &&
                                    (folder.Attributes & FileAttributes.Temporary) != FileAttributes.Temporary)
                        {
                            SeekFiles(folder, fileArray, subFolders);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Directory {directory} could not be accessed.", folder);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Directory {directory} could not be accessed.", root);
            }
        }

        private bool IsFileFiltered(FileTableRow file)
        {
            // database file
            if (file.FileNamePath().ToLower().StartsWith(appDataFolder))
            {
                return true;
            }

            // file path filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeFolder))
            {
                var excludeFolders = queryManager.Configuration.ExcludeFolder.Split('|');

                foreach (var entry in excludeFolders)
                {
                    if (("\\" + Path.Combine(Path.GetFileName(file.FileRoot), file.FilePath) + "\\").StartsWith(entry + "\\", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _logger.Debug("{fileName} was ignored due to file path filter.", file.FileNamePath());
                        return true;
                    }
                }
            }

            // file type filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeFileTypes))
            {
                var excludeFileTypes = queryManager.Configuration.ExcludeFileTypes.Split('|');

                foreach (var entry in excludeFileTypes)
                {
                    var fileExt = Path.GetExtension(file.FileNamePath()).ToLower();

                    if (("." + entry.ToLower()) == fileExt)
                    {
                        _logger.Debug("{fileName} was ignored due to file extension filter.", file.FileNamePath());
                        return true;
                    }
                }
            }

            // file size filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeFileBigger) && file.FileSize > long.Parse(queryManager.Configuration.ExcludeFileBigger))
            {
                _logger.Debug("{fileName} was ignored due to file size filter.", file.FileNamePath());
                return true;
            }

            // mask filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeMask))
            {
                try
                {
                    if (_regexExcludeCache == null)
                    {
                        _regexExcludeCache = new Regex(queryManager.Configuration.ExcludeMask, RegexOptions.Compiled & RegexOptions.Singleline);
                    }

                    if (_regexExcludeCache.IsMatch(file.FileNamePath()))
                    {
                        _logger.Debug("{fileName} was ignored due to regex filter.", file.FileNamePath());
                        return true;
                    }
                }
                catch
                {
                    _logger.Error("{fileName} was not ignored due to error with regular expressions {expr}",
                        file.FileNamePath(),
                        queryManager.Configuration.ExcludeMask);
                    return false;
                }
            }

            // file name filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeFile))
            {
                var excludeFile = queryManager.Configuration.ExcludeFile.Split('|');

                foreach (var entry in excludeFile)
                {
                    if (file.FileNamePath().ToLower().EndsWith(entry.ToLower()))
                    {
                        _logger.Debug("{fileName} was ignored due to file name filter.", file.FileNamePath());
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsFolderFiltered(FolderTableRow folder)
        {
            // file path filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeFolder))
            {
                var excludeFolders = queryManager.Configuration.ExcludeFolder.Split('|');

                foreach (var entry in excludeFolders)
                {
                    // check if source folder (for drive backup)
                    if (("\\" + Path.GetFileName(folder.Folder) + "\\").StartsWith(entry + "\\", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _logger.Debug("{folderName} was ignored due to root folder path filter.", folder.Folder);
                        return true;
                    }

                    if (("\\" + Path.Combine(Path.GetFileName(this.root), GetRelativeFolder(folder.Folder, folder.RootPath)) + "\\").StartsWith(entry + "\\", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _logger.Debug("{folderName} was ignored due to folder path filter.", folder.Folder);
                        return true;
                    }
                }
            }

            // mask filter
            if (!string.IsNullOrEmpty(queryManager.Configuration.ExcludeMask))
            {
                if (_regexExcludeCache == null)
                {
                    _regexExcludeCache = new Regex(queryManager.Configuration.ExcludeMask, RegexOptions.Compiled & RegexOptions.Singleline);
                }

                if (_regexExcludeCache.IsMatch(folder.Folder))
                {
                    _logger.Debug("{folderName} was ignored due to regex path filter.", folder.Folder);
                    return true;
                }
            }

            return false;
        }

        public static string GetRelativeFolder(string path, string rootPath)
        {
            var result = path;
            result = result.Replace(rootPath, "");

            if (result.StartsWith("\\") && result.Length > 1)
            {
                result = result[1..];
            }
            else if (result == "\\")
            {
                return "";
            }

            return result;
        }
    }
}

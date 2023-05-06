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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine;

public class QueryManager : IQueryManager
{
    private readonly IDbClientFactory dbClientFactory;

    private readonly IConfigurationManager configurationManager;

    private readonly IStorageFactory storageFactory;

    public QueryManager(IDbClientFactory dbClientFactory,
        IConfigurationManager configurationManager,
        IStorageFactory storageFactory)
    {
        this.dbClientFactory = dbClientFactory;
        this.configurationManager = configurationManager;
        this.storageFactory = storageFactory;
    }

    /// <summary>
    /// Returns a the last version of the backup.
    /// </summary>
    /// <returns></returns>
    public async Task<VersionDetails> GetLastBackupAsync()
    {
        VersionDetails result = null;

        // obtain lastest backup
        using (var dbClient = dbClientFactory.CreateDbClient())
        using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM versiontable WHERE versionStatus = 0 ORDER BY versionID DESC LIMIT 1", null))
        {
            if (reader.Read())
            {
                result = VersionDetails.FromReader(reader);
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns a the last version of the full backup.
    /// </summary>
    /// <returns></returns>
    public async Task<VersionDetails> GetLastFullBackupAsync()
    {
        VersionDetails result = null;

        // obtain last full backup
        using (var dbClient = dbClientFactory.CreateDbClient())
        using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM versiontable WHERE versionStatus = 0 AND versionType = 2 ORDER BY versionID DESC LIMIT 1", null))
        {
            if (reader.Read())
            {
                result = VersionDetails.FromReader(reader);
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns all versions, ordered by date ascending (except if specified otherwise).
    /// </summary>
    /// <param name="desc"></param>
    /// <returns></returns>
    public List<VersionDetails> GetVersions(bool desc = true)
    {
        var result = new List<VersionDetails>();

        // obtain all backups
        using (var dbClient = dbClientFactory.CreateDbClient())
        using (var reader = dbClient.ExecuteDataReader(
            CommandType.Text,
            "SELECT v.*, (SELECT SUM(fileSize) FROM fileversiontable WHERE filepackage = v.versionid) AS versionSize FROM versiontable AS v WHERE v.versionStatus = 0 ORDER BY v.versionID " + (desc ? "DESC" : "ASC")
            , null))
        {
            while (reader.Read())
            {
                result.Add(VersionDetails.FromReaderDetailed(reader));
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns the number of active backups.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetNumberOfVersionsAsync()
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var result = await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM versiontable WHERE versionStatus = 0");

        return int.Parse(result.ToString());
    }

    /// <summary>
    /// Returns a the oldest version of the full backup.
    /// </summary>
    /// <returns></returns>
    public async Task<VersionDetails> GetOldestBackupAsync()
    {
        VersionDetails result = null;

        // obtain oldest backup
        using (var dbClient = dbClientFactory.CreateDbClient())
        using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM versiontable ORDER BY versionID ASC LIMIT 1", null))
        {
            if (reader.Read())
            {
                result = VersionDetails.FromReader(reader);
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns the version details for a given version id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<VersionDetails> GetVersionByIdAsync(string id)
    {
        VersionDetails result = null;

        // obtain lastest backup
        using (var dbClient = dbClientFactory.CreateDbClient())
        using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, $"SELECT * FROM versiontable WHERE versionID = {id}", null))
        {
            if (reader.Read())
            {
                result = VersionDetails.FromReader(reader);
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns all folders in an backup.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task<List<string>> GetFolderListAsync(string version, string path)
    {
        var result = new List<string>();

        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            // get folders from stored files
            var parameters = new IDataParameter[]
            {
                dbClient.CreateParameter("path", DbType.String, 0, path),
                dbClient.CreateParameter("version", DbType.Int32, 0, version)
            };

            using (var reader = await dbClient.ExecuteDataReaderAsync(
                CommandType.Text,
                "SELECT DISTINCT filePath " +
                "FROM filelink " +
                "INNER JOIN fileversiontable ON " +
                "filelink.fileversionID = fileversiontable.fileversionID " +
                "INNER JOIN filetable ON " +
                "fileversiontable.fileID = filetable.fileID " +
                "AND filetable.filePath LIKE @path " +
                "WHERE filelink.versionID = @version",
                parameters))
            {
                while (await reader.ReadAsync())
                {
                    var folder = reader.GetString("filePath");

                    if (folder.StartsWith("\\"))
                    {
                        folder = folder[1..];
                    }

                    if (folder.EndsWith("\\"))
                    {
                        folder = folder[..^1];
                    }

                    result.Add(folder);
                }
                reader.Close();
            }

            // get empty folders
            using (var reader = await dbClient.ExecuteDataReaderAsync(
                CommandType.Text,
                "Select DISTINCT folder FROM foldertable, folderlink " +
                "WHERE folderlink.versionID = @version " +
                "AND folderlink.folderid = foldertable.id " +
                "AND foldertable.folder LIKE @path",
                parameters))
            {
                while (await reader.ReadAsync())
                {
                    var folder = reader.GetString("folder");

                    if (folder.StartsWith("\\"))
                    {
                        folder = folder[1..];
                    }

                    if (folder.EndsWith("\\"))
                    {
                        folder = folder[..^1];
                    }

                    result.Add(folder);
                }
                reader.Close();
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the previous version where the corresponding search string matches.
    /// </summary>
    /// <param name="startVersion"></param>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString)
    {
        try
        {
            using var dbClient = dbClientFactory.CreateDbClient();
            var parameters = new IDataParameter[]
            {
                    dbClient.CreateParameter("startVersion", DbType.Int32, 0, int.Parse(startVersion)),
                    dbClient.CreateParameter("searchString", DbType.String, 0, "%" + searchString + "%")
            };

            var result = await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT a.versionID FROM versiontable a " +
                "WHERE a.versionID < @startVersion " +
                "AND a.versionStatus = 0 " +
                "AND EXISTS (Select 1 FROM fileversiontable, filelink, filetable " +
                "WHERE fileversiontable.fileversionid = filelink.fileversionid " +
                "AND filelink.versionID = a.versionID " +
                "AND filetable.fileID = fileversiontable.fileID " +
                "AND (filetable.filePath LIKE @searchString OR filetable.fileName LIKE @searchString)) " +
                "ORDER BY versionID DESC LIMIT 1", parameters);

            if (result != null)
            {
                return result.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the previous version where the corresponding path.
    /// </summary>
    /// <param name="startVersion"></param>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path)
    {
        try
        {
            if (!path.StartsWith("\\"))
            {
                path = "\\" + path;
            }

            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            using var dbClient = dbClientFactory.CreateDbClient();
            var parameters = new IDataParameter[]
            {
                    dbClient.CreateParameter("startVersion", DbType.Int32, 0, int.Parse(startVersion)),
                    dbClient.CreateParameter("path", DbType.String, 0, path)
            };

            var result = await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT a.versionID FROM versiontable a " +
                "WHERE a.versionID < @startVersion " +
                "AND a.versionStatus = 0 " +
                "AND EXISTS (SELECT 1 FROM fileversiontable, filelink, filetable " +
                "WHERE fileversiontable.fileversionID = filelink.fileversionID " +
                "AND filelink.versionID = a.versionID " +
                "AND filetable.fileID = fileversiontable.fileID " +
                "AND filetable.filePath = @path) " +
                "ORDER BY versionID DESC LIMIT 1", parameters);

            if (result != null)
            {
                return result.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the succeeding version where the corresponding search string matches.
    /// </summary>
    /// <param name="startVersion"></param>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString)
    {
        try
        {
            using var dbClient = dbClientFactory.CreateDbClient();
            var parameters = new IDataParameter[]
            {
                    dbClient.CreateParameter("startVersion", DbType.Int32, 0, int.Parse(startVersion)),
                    dbClient.CreateParameter("searchString", DbType.String, 0,  "%" + searchString + "%")
            };

            var result = await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT a.versionID FROM versiontable a " +
                "WHERE a.versionID > @startVersion " +
                "AND a.versionStatus = 0 " +
                "AND EXISTS (SELECT 1 FROM fileversiontable, filelink, filetable " +
                "WHERE fileversiontable.fileversionid = filelink.fileversionid " +
                "AND filelink.versionID = a.versionID " +
                "AND filetable.fileID = fileversiontable.fileID " +
                "AND (filetable.filePath LIKE @searchString OR filetable.fileName LIKE @searchString)) " +
                "ORDER BY versionID DESC LIMIT 1", parameters);

            if (result != null)
            {
                return result.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the succeeding version where the corresponding path.
    /// </summary>
    /// <param name="startVersion"></param>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path)
    {
        try
        {
            if (!path.StartsWith("\\"))
            {
                path = "\\" + path;
            }

            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            using var dbClient = dbClientFactory.CreateDbClient();
            var parameters = new IDataParameter[]
            {
                    dbClient.CreateParameter("startVersion", DbType.Int32, 0, int.Parse(startVersion)),
                    dbClient.CreateParameter("path", DbType.String, 0, path)
            };

            var result = await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT a.versionID FROM versiontable a " +
                "WHERE a.versionID > @startVersion " +
                "AND a.versionStatus = 0 " +
                "AND EXISTS (SELECT 1 FROM fileversiontable, filelink, filetable " +
                "WHERE fileversiontable.fileversionid = filelink.fileversionid " +
                "AND filelink.versionID = a.versionID " +
                "AND filetable.fileID = fileversiontable.fileID " +
                "AND filetable.filePath = @path) " +
                "ORDER BY versionID ASC LIMIT 1", parameters);

            if (result != null)
            {
                return result.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a list of all versions for a given file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath)
    {
        var result = new List<FileTableRow>();

        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            var fileSelectParameters = new IDataParameter[] {
                dbClient.CreateParameter("fileName", DbType.String, 0, fileName),
                dbClient.CreateParameter("filePath", DbType.String, 0, filePath)
            };

            // get folders from stored files
            using var reader = await dbClient.ExecuteDataReaderAsync(
                CommandType.Text,
                "SELECT filetable.fileID AS fileID, fileName, filePath, fileSize, fileDateCreated, fileDateModified, filePackage, fileType, fileStatus, versionDate, versionStatus " +
                "FROM filetable, fileversiontable, versiontable " +
                "WHERE versiontable.versionID = fileversiontable.filePackage " +
                "AND filetable.fileID = fileversiontable.fileID " +
                "AND fileName LIKE @fileName " +
                "AND filePath LIKE @filePath",
                fileSelectParameters);
            while (await reader.ReadAsync())
            {
                result.Add(FileTableRow.FromReaderFileVersion(reader));
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns a list of all files for the given version and path.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path)
    {
        var result = new List<FileTableRow>();

        // fix path
        if (!path.StartsWith("\\"))
        {
            path = "\\" + path;
        }

        if (!path.EndsWith("\\"))
        {
            path += "\\";
        }

        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            var fileSelectParameters = new IDataParameter[] {
                dbClient.CreateParameter("filePath", DbType.String, 0, path),
                dbClient.CreateParameter("versionID", DbType.Int32, 0, version)
            };

            // get folders from stored files
            using var reader = await dbClient.ExecuteDataReaderAsync(
                CommandType.Text,
                "SELECT " +
                "fileversiontable.fileStatus, " +
                "fileversiontable.fileType, " +
                "fileversiontable.fileDateModified, " +
                "fileversiontable.fileDateCreated, " +
                "fileversiontable.fileSize, " +
                "fileversiontable.filePackage, " +
                "fileversiontable.longfilename, " +
                "filetable.fileName, " +
                "filetable.filePath, " +
                "versiontable.versionDate, " +
                "filelink.versionID, " +
                "filetable.fileID " +
                "FROM  " +
                "filelink " +
                "INNER JOIN fileversiontable On (filelink.fileversionID = fileversiontable.fileversionID)  " +
                "INNER JOIN filetable On (fileversiontable.fileID = filetable.fileID)  " +
                "INNER JOIN versiontable On (versiontable.versionID = fileversiontable.filePackage)  " +
                "WHERE(filelink.versionID = @versionID) " +
                "And filePath = @filePath",
                fileSelectParameters);
            while (reader.Read())
            {
                result.Add(FileTableRow.FromReaderFile(reader));
            }

            reader.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns a local file path if the file is directly accessable, otherwise null.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public string GetFileNameFromDrive(FileTableRow file)
    {
        // check if we can directly read file?
        if (file.FileType == "1")
        {
            var folderPath = configurationManager.BackupFolder;
            folderPath = Path.Combine(folderPath, file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss"));

            if (!string.IsNullOrEmpty(file.FileLongFileName))
            {
                return Path.Combine(folderPath, "_LONGFILES_", file.FileLongFileName);
            }

            if (file.FilePath.StartsWith("\\"))
            {
                folderPath = Path.Combine(folderPath, file.FilePath[1..]);
            }
            else
            {
                folderPath = Path.Combine(folderPath, file.FilePath);
            }

            return Path.Combine(folderPath, file.FileName);
        }

        return null;
    }

    /// <summary>
    /// Returns a local file path; the file will be temporarily copied to local device.
    /// </summary>
    /// <param name="versionId"></param>
    /// <param name="fileName"></param>
    /// <param name="filePath"></param>
    /// <param name="password"></param>
    /// <param name="temp"></param>
    /// <returns></returns>
    public async Task<Tuple<string, bool>> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, SecureString password)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        using var storage = storageFactory.GetCurrentStorageProvider();
        storage.Open();

        var temp = false;
        string result = null;

        var parameters = new IDataParameter[]
        {
                dbClient.CreateParameter("versionId", DbType.Int32, 0, versionId),
                dbClient.CreateParameter("fileName", DbType.String, 0, fileName),
                dbClient.CreateParameter("filePath", DbType.String, 0, filePath)
        };

        using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text,
            "SELECT fileversiontable.*, filetable.*, versiontable.versionDate, versiontable.versionStatus " +
            "FROM filetable, fileversiontable, versiontable, filelink " +
            "WHERE filelink.fileversionID = fileversiontable.fileversionID " +
            "AND filelink.versionID = @versionId " +
            "AND fileversiontable.filePackage = versiontable.versionID " +
            "AND filetable.fileName LIKE @fileName " +
            "AND filetable.filePath LIKE @filePath " +
            "AND fileversiontable.fileID = filetable.fileID " +
            "LIMIT 1",
            parameters))
        {
            if (reader.Read())
            {
                var fileType = reader.GetInt32("fileType");

                if (fileType == 1)
                {
                    result = GetFileNameFromDrive(FileTableRow.FromReaderFileVersion(reader));
                }
                else if (fileType >= 2 && fileType <= 6)
                {
                    temp = true;

                    var localFilePath = Path.Combine(Path.GetTempPath(), reader.GetString("fileName"));
                    var remoteFilePath = "";

                    if (!string.IsNullOrEmpty(reader.GetString("longfilename")))
                    {
                        remoteFilePath = reader.GetString("versionDate") + "\\_LONG_FILES\\" + reader.GetString("longfilename");
                    }
                    else
                    {
                        remoteFilePath = reader.GetString("versionDate") + reader.GetString("filePath") + reader.GetString("fileName");
                    }

                    if (fileType == 3)
                    {
                        storage.CopyFileFromStorage(localFilePath, remoteFilePath);
                    }
                    else if (fileType == 2 || fileType == 4)
                    {
                        storage.CopyFileFromStorageCompressed(localFilePath, remoteFilePath);
                    }
                    else if (fileType == 5 || fileType == 6)
                    {
                        storage.CopyFileFromStorageEncrypted(localFilePath, remoteFilePath, password);
                    }

                    result = localFilePath;
                }
            }

            reader.Close();
        }

        return new Tuple<string, bool>(result, temp);
    }

    /// <summary>
    /// Returns true if the given path and version contains changes.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="versionId"></param>
    /// <returns></returns>
    public async Task<bool> HasChangesOrNewAsync(string path, string versionId)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var result = await dbClient.ExecuteScalarAsync($"SELECT COUNT(1) FROM fileversiontable, filetable WHERE filetable.fileID = fileversiontable.fileID AND fileversiontable.filePackage = {versionId} AND filetable.filePath LIKE \"{path}%\"");
        if (result == null)
        {
            return false;
        }

        return int.Parse(result.ToString()) > 0;
    }

    /// <summary>
    /// Returns the full restore folder name, given the folder and version.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task<string> GetFullRestoreFolderAsync(string folder, string version)
    {
        // correct path
        if (!folder.StartsWith("\\"))
        {
            folder = "\\" + folder;
        }

        if (!folder.EndsWith("\\"))
        {
            folder += "\\";
        }

        // obtain source folders
        var sourcesInVersion = (await GetVersionByIdAsync(version)).Sources;
        if (string.IsNullOrEmpty(sourcesInVersion))
        {
            sourcesInVersion = configurationManager.SourceFolder;
        }

        // add destination folders
        var destFolders = sourcesInVersion.Split('|');

        foreach (var destination in destFolders)
        {
            var directory = new DirectoryInfo(destination);

            if (folder.StartsWith("\\" + directory.Name + "\\"))
            {
                var idx = folder.IndexOf("\\" + directory.Name + "\\");

                // path found
                var result = Path.Combine(destination, folder[(idx + directory.Name.Length + 2)..]);

                if (result.EndsWith("\\"))
                {
                    return result[..^1];
                }

                return result;
            }
        }

        return null;
    }

    public async Task<string> GetLocalizedPathAsync(string path)
    {
        try
        {
            // correct path
            if (path.StartsWith("\\"))
            {
                path = path[1..];
            }

            if (path.EndsWith("\\"))
            {
                path = path[..^1];
            }

            if (configurationManager.ShowLocalizedPath != "1")
            {
                return path;
            }

            // search path in database
            using (var dbClient = dbClientFactory.CreateDbClient())
            {
                var parameters = new IDataParameter[]
                {
                    dbClient.CreateParameter("junction", DbType.String, 0, "%" + path + "%")
                };

                using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT junction, folder FROM folderjunctiontable WHERE junction LIKE @junction", parameters);
                while (reader.Read())
                {
                    var junction = reader.GetString("junction");
                    var folder = reader.GetString("folder");

                    if (path.Contains(junction))
                    {
                        reader.Close();

                        return path.Replace(junction, folder);
                    }
                }

                reader.Close();
            }

            return path;
        }
        catch
        {
            return path;
        }
    }

    public async Task<int> GetNumberOfFilesAsync()
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var result = await dbClient.ExecuteScalarAsync("SELECT COUNT(1) FROM filetable");
        if (result == null)
        {
            return 0;
        }

        return int.Parse(result.ToString());
    }

    public async Task<double> GetTotalFileSizeAsync()
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var result = await dbClient.ExecuteScalarAsync("SELECT SUM(fileSize) FROM fileversiontable");
        if (result == null)
        {
            return 0;
        }

        return double.Parse(result.ToString());
    }
}

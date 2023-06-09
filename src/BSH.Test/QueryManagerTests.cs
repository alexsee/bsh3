using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Storage;
using NUnit.Framework;

namespace BSH.Test;
public class QueryManagerTests
{
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;

    [SetUp]
    public async Task Setup()
    {
        if (dbClientFactory != null)
        {
            DbClientFactory.ClosePool();
            this.dbClientFactory = null;
        }

        // start with clean database
        if (File.Exists("testdb_querymanager.db"))
        {
            File.Delete("testdb_querymanager.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Environment.CurrentDirectory + "\\testdb_querymanager.db");

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.BackupFolder = "X:\\Backups";

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);

        // insert some data
        await PopulateExampleData();
    }

    public async Task PopulateExampleData()
    {
        // versions
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionStable, versionSources, versionStatus, versionType) VALUES (1, '01-01-2021 00-00-00', '1.0.0', 'Initial full version', 1, '', 0, 2)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionStable, versionSources, versionStatus, versionType) VALUES (2, '02-01-2021 00-00-00', '2.0.0', 'Incremental version', 1, '', 0, 1)");

        // files
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filetable (fileID, fileName, filePath) VALUES (1, 'file1.txt', '\\source_1\\')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filetable (fileID, fileName, filePath) VALUES (2, 'file2.txt', '\\source_1\\subfolder\\')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filetable (fileID, fileName, filePath) VALUES (3, 'file3.txt', '\\source_2\\subfolder\\')");

        // file links
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (1, 1)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (2, 1)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (3, 1)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (1, 2)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (2, 2)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (3, 2)");

        // file versions
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID) VALUES (1, 0, 1, 'hash1', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 100, 1, '1')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID) VALUES (2, 0, 1, 'hash1', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 100, 1, '2')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID) VALUES (3, 0, 1, 'hash1', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 100, 1, '3')");

        // folders
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO foldertable (id, folder) VALUES (1, '\\source_3\\')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO foldertable (id, folder) VALUES (2, '\\source_3\\subfolder\\')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO foldertable (id, folder) VALUES (3, '\\source_4\\subfolder\\')");

        // folder links
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO folderlink (folderid, versionid) VALUES (1, 1)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO folderlink (folderid, versionid) VALUES (2, 1)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO folderlink (folderid, versionid) VALUES (3, 1)");
    }

    [Test]
    public async Task GetLastBackupAsyncTest()
    {
        var result = await queryManager.GetLastBackupAsync();
        Assert.AreEqual("2", result.Id);
    }

    [Test]
    public async Task GetLastFullBackupAsyncTest()
    {
        var result = await queryManager.GetLastFullBackupAsync();
        Assert.AreEqual("1", result.Id);
    }

    [Test]
    public void GetVersionsTest()
    {
        var result = queryManager.GetVersions();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("2", result[0].Id);

        result = queryManager.GetVersions(false);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("1", result[0].Id);
    }

    [Test]
    public async Task GetNumberOfVersionsAsyncTest()
    {
        var result = await queryManager.GetNumberOfVersionsAsync();
        Assert.AreEqual(2, result);
    }

    [Test]
    public async Task GetOldestBackupAsyncTest()
    {
        var result = await queryManager.GetOldestBackupAsync();
        Assert.AreEqual("1", result.Id);
    }

    [Test]
    public async Task GetVersionByIdAsyncTest()
    {
        var result = await queryManager.GetVersionByIdAsync("1");
        Assert.AreEqual("1", result.Id);

        result = await queryManager.GetVersionByIdAsync("2");
        Assert.AreEqual("2", result.Id);
    }

    [Test]
    public async Task GetFolderListAsyncTest()
    {
        var result = await queryManager.GetFolderListAsync("1", "%");
        Assert.AreEqual(6, result.Count);
        Assert.AreEqual("source_1", result[0]);
        Assert.AreEqual("source_1\\subfolder", result[1]);
        Assert.AreEqual("source_2\\subfolder", result[2]);
        Assert.AreEqual("source_3", result[3]);
        Assert.AreEqual("source_3\\subfolder", result[4]);
        Assert.AreEqual("source_4\\subfolder", result[5]);
    }

    [Test]
    public async Task GetBackVersionWhereFileAsyncTest()
    {
        var result = await queryManager.GetBackVersionWhereFileAsync("2", "file1.txt");
        Assert.AreEqual("1", result);

        result = await queryManager.GetBackVersionWhereFileAsync("1", "file1.txt");
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetBackVersionWhereFilesInFolderAsyncTest()
    {
        var result = await queryManager.GetBackVersionWhereFilesInFolderAsync("2", "source_1");
        Assert.AreEqual("1", result);

        result = await queryManager.GetBackVersionWhereFilesInFolderAsync("2", "\\source_1");
        Assert.AreEqual("1", result);

        result = await queryManager.GetBackVersionWhereFilesInFolderAsync("2", "source_1\\");
        Assert.AreEqual("1", result);

        result = await queryManager.GetBackVersionWhereFilesInFolderAsync("1", "source_1");
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetNextVersionWhereFileAsyncTest()
    {
        var result = await queryManager.GetNextVersionWhereFileAsync("2", "file1.txt");
        Assert.IsNull(result);

        result = await queryManager.GetNextVersionWhereFileAsync("1", "file1.txt");
        Assert.AreEqual("2", result);
    }

    [Test]
    public async Task GetNextVersionWhereFilesInFolderAsyncTest()
    {
        var result = await queryManager.GetNextVersionWhereFilesInFolderAsync("1", "source_1");
        Assert.AreEqual("2", result);

        result = await queryManager.GetNextVersionWhereFilesInFolderAsync("1", "\\source_1");
        Assert.AreEqual("2", result);

        result = await queryManager.GetNextVersionWhereFilesInFolderAsync("1", "source_1\\");
        Assert.AreEqual("2", result);

        result = await queryManager.GetNextVersionWhereFilesInFolderAsync("2", "source_1");
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetVersionsByFileAsyncTest()
    {
        var result = await queryManager.GetVersionsByFileAsync("file1.txt", "\\source_1\\");
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public async Task GetFilesByVersionAsyncTest()
    {
        var result = await queryManager.GetFilesByVersionAsync("1", "\\source_1\\");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("file1.txt", result[0].FileName);

        result = await queryManager.GetFilesByVersionAsync("1", "source_1");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("file1.txt", result[0].FileName);

        result = await queryManager.GetFilesByVersionAsync("2", "\\source_1\\");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("file1.txt", result[0].FileName);
    }

    [Test]
    public void GetFileNameFromDriveTest()
    {
        var file = new FileTableRow()
        {
            FileType = "1",
            FileVersionDate = DateTime.Now,
            FilePath = "\\source_1",
            FileName = "file1.txt"
        };

        var result = queryManager.GetFileNameFromDrive(file);
        Assert.AreEqual("X:\\Backups\\" + file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss") + "\\source_1\\file1.txt", result);

        file.FilePath = "source_1";
        result = queryManager.GetFileNameFromDrive(file);
        Assert.AreEqual("X:\\Backups\\" + file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss") + "\\source_1\\file1.txt", result);

        file.FileLongFileName = "XYZ";
        result = queryManager.GetFileNameFromDrive(file);
        Assert.AreEqual("X:\\Backups\\" + file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss") + "\\_LONGFILES_\\XYZ", result);

        file.FileType = "2";
        result = queryManager.GetFileNameFromDrive(file);
        Assert.IsNull(result);
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
using BSH.Test.Mocks;
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
        configurationManager.SourceFolder = "Y:\\MyFiles\\source_1";

        var storageFactory = new StorageFactoryMock();
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);

        // insert some data
        await PopulateExampleData();
    }

    public async Task PopulateExampleData()
    {
        // versions
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionStable, versionSources, versionStatus, versionType) VALUES (1, '01-01-2021 00-00-00', '1.0.0', 'Initial full version', 1, 'Y:\\MyFiles\\source_1|Y:\\MyFiles\\source_2\\', 0, 2)");
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
        Assert.That(result.Id, Is.EqualTo("2"));
    }

    [Test]
    public async Task GetLastFullBackupAsyncTest()
    {
        var result = await queryManager.GetLastFullBackupAsync();
        Assert.That(result.Id, Is.EqualTo("1"));
    }

    [Test]
    public void GetVersionsTest()
    {
        var result = queryManager.GetVersions();
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("2"));

        result = queryManager.GetVersions(false);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("1"));
    }

    [Test]
    public async Task GetNumberOfVersionsAsyncTest()
    {
        var result = await queryManager.GetNumberOfVersionsAsync();
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public async Task GetOldestBackupAsyncTest()
    {
        var result = await queryManager.GetOldestBackupAsync();
        Assert.That(result.Id, Is.EqualTo("1"));
    }

    [Test]
    public async Task GetVersionByIdAsyncTest()
    {
        var result = await queryManager.GetVersionByIdAsync("1");
        Assert.That(result.Id, Is.EqualTo("1"));

        result = await queryManager.GetVersionByIdAsync("2");
        Assert.That(result.Id, Is.EqualTo("2"));
    }

    [Test]
    public async Task GetFolderListAsyncTest()
    {
        var result = await queryManager.GetFolderListAsync("1", "%");
        Assert.That(result.Count, Is.EqualTo(6));
        Assert.That(result[0], Is.EqualTo("source_1"));
        Assert.That(result[1], Is.EqualTo("source_1\\subfolder"));
        Assert.That(result[2], Is.EqualTo("source_2\\subfolder"));
        Assert.That(result[3], Is.EqualTo("source_3"));
        Assert.That(result[4], Is.EqualTo("source_3\\subfolder"));
        Assert.That(result[5], Is.EqualTo("source_4\\subfolder"));
    }

    [Test]
    public async Task GetBackVersionWhereFileAsyncTest()
    {
        var result = await queryManager.GetBackVersionWhereFileAsync("2", "file1.txt");
        Assert.That(result, Is.EqualTo("1"));

        result = await queryManager.GetBackVersionWhereFileAsync("1", "file1.txt");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetBackVersionWhereFilesInFolderAsyncTest()
    {
        var result = await queryManager.GetBackVersionWhereFilesInFolderAsync("2", "source_1");
        Assert.That(result, Is.EqualTo("1"));

        result = await queryManager.GetBackVersionWhereFilesInFolderAsync("2", "\\source_1");
        Assert.That(result, Is.EqualTo("1"));

        result = await queryManager.GetBackVersionWhereFilesInFolderAsync("2", "source_1\\");
        Assert.That(result, Is.EqualTo("1"));

        result = await queryManager.GetBackVersionWhereFilesInFolderAsync("1", "source_1");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetNextVersionWhereFileAsyncTest()
    {
        var result = await queryManager.GetNextVersionWhereFileAsync("2", "file1.txt");
        Assert.That(result, Is.Null);

        result = await queryManager.GetNextVersionWhereFileAsync("1", "file1.txt");
        Assert.That(result, Is.EqualTo("2"));
    }

    [Test]
    public async Task GetNextVersionWhereFilesInFolderAsyncTest()
    {
        var result = await queryManager.GetNextVersionWhereFilesInFolderAsync("1", "source_1");
        Assert.That(result, Is.EqualTo("2"));

        result = await queryManager.GetNextVersionWhereFilesInFolderAsync("1", "\\source_1");
        Assert.That(result, Is.EqualTo("2"));

        result = await queryManager.GetNextVersionWhereFilesInFolderAsync("1", "source_1\\");
        Assert.That(result, Is.EqualTo("2"));

        result = await queryManager.GetNextVersionWhereFilesInFolderAsync("2", "source_1");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetVersionsByFileAsyncTest()
    {
        var result = await queryManager.GetVersionsByFileAsync("file1.txt", "\\source_1\\");
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetFilesByVersionAsyncTest()
    {
        var result = await queryManager.GetFilesByVersionAsync("1", "\\source_1\\");
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("file1.txt"));

        result = await queryManager.GetFilesByVersionAsync("1", "source_1");
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("file1.txt"));

        result = await queryManager.GetFilesByVersionAsync("2", "\\source_1\\");
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FileName, Is.EqualTo("file1.txt"));
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
        Assert.That(result, Is.EqualTo("X:\\Backups\\" + file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss") + "\\source_1\\file1.txt"));

        file.FilePath = "source_1";
        result = queryManager.GetFileNameFromDrive(file);
        Assert.That(result, Is.EqualTo("X:\\Backups\\" + file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss") + "\\source_1\\file1.txt"));

        file.FileLongFileName = "XYZ";
        result = queryManager.GetFileNameFromDrive(file);
        Assert.That(result, Is.EqualTo("X:\\Backups\\" + file.FileVersionDate.ToString("dd-MM-yyyy HH-mm-ss") + "\\_LONGFILES_\\XYZ"));

        file.FileType = "2";
        result = queryManager.GetFileNameFromDrive(file);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetFileNameFromDriveAsyncTest()
    {
        var result = await queryManager.GetFileNameFromDriveAsync(1, "file1.txt", "\\source_1\\", null);
        Assert.That(result, Is.EqualTo(("X:\\Backups\\01-01-2021 00-00-00\\source_1\\file1.txt", false)));
    }

    [Test]
    public async Task HasChangesOrNewAsyncTest()
    {
        var result = await queryManager.HasChangesOrNewAsync("\\source_1\\", "1");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetFullRestoreFolderAsyncTest()
    {
        var result = await queryManager.GetFullRestoreFolderAsync("\\source_1\\", "1");
        Assert.That(result, Is.EqualTo("Y:\\MyFiles\\source_1"));

        result = await queryManager.GetFullRestoreFolderAsync("source_1", "1");
        Assert.That(result, Is.EqualTo("Y:\\MyFiles\\source_1"));

        result = await queryManager.GetFullRestoreFolderAsync("source_1", "2");
        Assert.That(result, Is.EqualTo("Y:\\MyFiles\\source_1"));

        result = await queryManager.GetFullRestoreFolderAsync("source_2", "1");
        Assert.That(result, Is.EqualTo("Y:\\MyFiles\\source_2"));

        result = await queryManager.GetFullRestoreFolderAsync("source_3", "1");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLocalizedPathAsyncTest()
    {
        var result = await queryManager.GetLocalizedPathAsync("\\source_1\\");
        Assert.That(result, Is.EqualTo("source_1"));

        // activate configuration
        configurationManager.ShowLocalizedPath = "1";

        result = await queryManager.GetLocalizedPathAsync("\\source_1\\");
        Assert.That(result, Is.EqualTo("source_1"));

        // add junction
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO folderjunctiontable VALUES ('source_1', 'source_1_localized')");

        result = await queryManager.GetLocalizedPathAsync("\\source_1\\");
        Assert.That(result, Is.EqualTo("source_1_localized"));
    }

    [Test]
    public async Task GetNumberOfFilesAsyncTest()
    {
        var result = await queryManager.GetNumberOfFilesAsync();
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public async Task GetTotalFileSizeAsyncTest()
    {
        var result = await queryManager.GetTotalFileSizeAsync();
        Assert.That(result, Is.EqualTo(300d));
    }
}

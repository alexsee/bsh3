// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Types;
using NUnit.Framework;

namespace BSH.Test.Repo;

public class MapperTests
{
    [Test]
    public void VersionDetailsMapper_FromReader_MapsCoreFields()
    {
        using var reader = CreateVersionReader(
            versionId: 7,
            versionDate: "15-03-2024 10-30-00",
            title: "Daily",
            description: "Morning backup",
            stable: true,
            sources: @"C:\Data",
            versionSize: null);

        Assert.That(reader.Read(), Is.True);

        var version = VersionDetailsMapper.FromReader(reader);

        Assert.That(version.Id, Is.EqualTo("7"));
        Assert.That(version.CreationDate, Is.EqualTo(new DateTime(2024, 3, 15, 10, 30, 0)));
        Assert.That(version.Title, Is.EqualTo("Daily"));
        Assert.That(version.Description, Is.EqualTo("Morning backup"));
        Assert.That(version.Stable, Is.True);
        Assert.That(version.Sources, Is.EqualTo(@"C:\Data"));
        Assert.That(version.Size, Is.EqualTo(0));
    }

    [Test]
    public void VersionDetailsMapper_FromReaderDetailed_MapsSizeWhenPresent()
    {
        using var reader = CreateVersionReader(
            versionId: 3,
            versionDate: "01-01-2021 00-00-00",
            title: "Full",
            description: "Initial",
            stable: false,
            sources: @"D:\Docs",
            versionSize: 12345.0);

        Assert.That(reader.Read(), Is.True);

        var version = VersionDetailsMapper.FromReaderDetailed(reader);

        Assert.That(version.Id, Is.EqualTo("3"));
        Assert.That(version.Stable, Is.False);
        Assert.That(version.Size, Is.EqualTo(12345L));
    }

    [Test]
    public void VersionDetailsMapper_FromReaderDetailed_SetsSizeZeroWhenColumnMissing()
    {
        using var reader = CreateVersionReader(
            versionId: 1,
            versionDate: "01-01-2021 00-00-00",
            title: "Full",
            description: "Initial",
            stable: true,
            sources: @"D:\Docs",
            versionSize: null);

        Assert.That(reader.Read(), Is.True);

        var version = VersionDetailsMapper.FromReaderDetailed(reader);

        Assert.That(version.Size, Is.EqualTo(0));
    }

    [Test]
    public void FileTableRowMapper_FromReaderFileVersion_MapsFields()
    {
        var created = new DateTime(2021, 1, 1, 8, 0, 0);
        var modified = new DateTime(2021, 1, 2, 9, 0, 0);

        using var reader = CreateFileVersionReader(
            fileId: 11,
            fileName: "notes.txt",
            filePath: @"\source_1\",
            fileSize: 2048,
            fileDateCreated: created,
            fileDateModified: modified,
            filePackage: 2,
            fileType: 1,
            fileStatus: 0,
            versionDate: "02-01-2021 00-00-00",
            versionStatus: 0);

        Assert.That(reader.Read(), Is.True);

        var row = FileTableRowMapper.FromReaderFileVersion(reader);

        Assert.That(row.FileId, Is.EqualTo("11"));
        Assert.That(row.FileName, Is.EqualTo("notes.txt"));
        Assert.That(row.FilePath, Is.EqualTo(@"\source_1\"));
        Assert.That(row.FileSize, Is.EqualTo(2048));
        Assert.That(row.FileDateCreated, Is.EqualTo(created));
        Assert.That(row.FileDateModified, Is.EqualTo(modified));
        Assert.That(row.FilePackage, Is.EqualTo("2"));
        Assert.That(row.FileType, Is.EqualTo("1"));
        Assert.That(row.FileStatus, Is.EqualTo("0"));
        Assert.That(row.FileVersionDate, Is.EqualTo(new DateTime(2021, 1, 2, 0, 0, 0)));
        Assert.That(row.VersionStatus, Is.EqualTo("0"));
    }

    [Test]
    public void FileTableRowMapper_FromReaderFile_MapsFieldsIncludingLongFileName()
    {
        var created = new DateTime(2022, 5, 1, 12, 0, 0);
        var modified = new DateTime(2022, 5, 2, 13, 0, 0);

        using var reader = CreateFileReader(
            versionId: 5,
            fileId: 22,
            fileName: "report.pdf",
            filePath: @"\docs\",
            fileSize: 4096,
            fileDateCreated: created,
            fileDateModified: modified,
            filePackage: 1,
            fileType: 2,
            fileStatus: 1,
            versionDate: "05-05-2022 14-15-16",
            longFileName: @"\\?\C:\docs\report.pdf");

        Assert.That(reader.Read(), Is.True);

        var row = FileTableRowMapper.FromReaderFile(reader);

        Assert.That(row.VersionId, Is.EqualTo("5"));
        Assert.That(row.FileId, Is.EqualTo("22"));
        Assert.That(row.FileName, Is.EqualTo("report.pdf"));
        Assert.That(row.FilePath, Is.EqualTo(@"\docs\"));
        Assert.That(row.FileSize, Is.EqualTo(4096));
        Assert.That(row.FilePackage, Is.EqualTo("1"));
        Assert.That(row.FileType, Is.EqualTo("2"));
        Assert.That(row.FileStatus, Is.EqualTo("1"));
        Assert.That(row.FileVersionDate, Is.EqualTo(new DateTime(2022, 5, 5, 14, 15, 16)));
        Assert.That(row.FileLongFileName, Is.EqualTo(@"\\?\C:\docs\report.pdf"));
    }

    [TestCase(null, @"\folder\", "file.txt", @"\folder\file.txt")]
    [TestCase(@"C:\Root", "", "file.txt", @"C:\Root\file.txt")]
    [TestCase(@"C:\Root", "\\", "file.txt", @"C:\Root\file.txt")]
    [TestCase(@"C:\Root", "sub", "file.txt", @"C:\Root\sub\file.txt")]
    public void FileTableRow_FileNamePath_CombinesRootPathAndName(string fileRoot, string filePath, string fileName, string expected)
    {
        var row = new FileTableRow
        {
            FileRoot = fileRoot,
            FilePath = filePath,
            FileName = fileName
        };

        Assert.That(row.FileNamePath(), Is.EqualTo(expected));
    }

    private static IDataReader CreateVersionReader(
        int versionId,
        string versionDate,
        string title,
        string description,
        bool stable,
        string sources,
        double? versionSize)
    {
        var table = new DataTable();
        table.Columns.Add("versionID", typeof(int));
        table.Columns.Add("versionDate", typeof(string));
        table.Columns.Add("versionTitle", typeof(string));
        table.Columns.Add("versionDescription", typeof(string));
        table.Columns.Add("versionStable", typeof(bool));
        table.Columns.Add("versionSources", typeof(string));

        if (versionSize.HasValue)
        {
            table.Columns.Add("versionSize", typeof(double));
            table.Rows.Add(versionId, versionDate, title, description, stable, sources, versionSize.Value);
        }
        else
        {
            table.Rows.Add(versionId, versionDate, title, description, stable, sources);
        }

        return table.CreateDataReader();
    }

    private static IDataReader CreateFileVersionReader(
        int fileId,
        string fileName,
        string filePath,
        double fileSize,
        DateTime fileDateCreated,
        DateTime fileDateModified,
        int filePackage,
        int fileType,
        int fileStatus,
        string versionDate,
        int versionStatus)
    {
        var table = new DataTable();
        table.Columns.Add("fileID", typeof(int));
        table.Columns.Add("fileName", typeof(string));
        table.Columns.Add("filePath", typeof(string));
        table.Columns.Add("fileSize", typeof(double));
        table.Columns.Add("fileDateCreated", typeof(DateTime));
        table.Columns.Add("fileDateModified", typeof(DateTime));
        table.Columns.Add("filePackage", typeof(int));
        table.Columns.Add("fileType", typeof(int));
        table.Columns.Add("fileStatus", typeof(int));
        table.Columns.Add("versionDate", typeof(string));
        table.Columns.Add("versionStatus", typeof(int));
        table.Rows.Add(
            fileId,
            fileName,
            filePath,
            fileSize,
            fileDateCreated,
            fileDateModified,
            filePackage,
            fileType,
            fileStatus,
            versionDate,
            versionStatus);

        return table.CreateDataReader();
    }

    private static IDataReader CreateFileReader(
        int versionId,
        int fileId,
        string fileName,
        string filePath,
        double fileSize,
        DateTime fileDateCreated,
        DateTime fileDateModified,
        int filePackage,
        int fileType,
        int fileStatus,
        string versionDate,
        string longFileName)
    {
        var table = new DataTable();
        table.Columns.Add("versionID", typeof(int));
        table.Columns.Add("fileID", typeof(int));
        table.Columns.Add("fileName", typeof(string));
        table.Columns.Add("filePath", typeof(string));
        table.Columns.Add("fileSize", typeof(double));
        table.Columns.Add("fileDateCreated", typeof(DateTime));
        table.Columns.Add("fileDateModified", typeof(DateTime));
        table.Columns.Add("filePackage", typeof(int));
        table.Columns.Add("fileType", typeof(int));
        table.Columns.Add("fileStatus", typeof(int));
        table.Columns.Add("versionDate", typeof(string));
        table.Columns.Add("longfilename", typeof(string));
        table.Rows.Add(
            versionId,
            fileId,
            fileName,
            filePath,
            fileSize,
            fileDateCreated,
            fileDateModified,
            filePackage,
            fileType,
            fileStatus,
            versionDate,
            longFileName);

        return table.CreateDataReader();
    }
}

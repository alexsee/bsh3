// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Fakes;
using BSH.Test.Helpers;
using NUnit.Framework;

namespace BSH.Test;

public class RestoreFileSafetyTests
{
    private string root;
    private string destination;

    [SetUp]
    public void SetUp()
    {
        root = Path.Combine(Path.GetTempPath(), "bsh-restore-safety-" + Guid.NewGuid().ToString("N"));
        destination = Path.Combine(root, "destination");
        Directory.CreateDirectory(destination);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(root, true);

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task WrongPasswordPreservesDestinationAndCleansTemporaryFiles(bool remote, bool destinationExists)
    {
        var source = Path.Combine(root, "source.txt");
        var encrypted = Path.Combine(root, "secret.txt.enc");
        await File.WriteAllTextAsync(source, "backed up contents");
        Assert.That(new Encryption().Encode(source, encrypted, "correct-password"), Is.True);
        var target = Path.Combine(destination, "secret.txt");
        if (destinationExists)
        {
            await File.WriteAllTextAsync(target, "original contents");
        }

        await using var server = remote ? new VersionMarkerFtpServer(await File.ReadAllBytesAsync(encrypted)) : null;
        var configuration = new FakeConfigurationManager { BackupFolder = root };
        using var storage = CreateStorage(configuration, server);
        storage.Open();
        using var job = CreateJob(storage, configuration, "wrong-password");
        using var table = CreateFileTable(remote ? 5 : 6);
        using var reader = table.CreateDataReader();
        Assert.That(await reader.ReadAsync(), Is.True);

        Assert.ThrowsAsync<FileNotProcessedException>(async () => await job.CopyFileFromDevice(storage, reader, destination));

        if (remote)
        {
            Assert.That(server.MarkerDownloaded, Is.True, "The restore must reach the encrypted FTP payload.");
        }
        if (destinationExists)
        {
            Assert.That(File.Exists(target), Is.True, "A failed restore must preserve the existing file.");
            Assert.That(await File.ReadAllTextAsync(target), Is.EqualTo("original contents"));
        }
        Assert.That(Directory.GetFileSystemEntries(destination),
            Is.EquivalentTo(destinationExists ? new[] { target } : Array.Empty<string>()));
    }

    [TestCase(false, 1)]
    [TestCase(false, 2)]
    [TestCase(false, 6)]
    [TestCase(true, 3)]
    [TestCase(true, 4)]
    [TestCase(true, 5)]
    public async Task SuccessfulRestoreReplacesDestinationAndCleansTemporaryFiles(bool remote, int fileType)
    {
        var source = Path.Combine(root, "secret.txt");
        await File.WriteAllTextAsync(source, "restored contents");
        var payload = source;
        if (fileType is 2 or 4)
        {
            payload = source + ".zip";
            await using var archive = await ZipFile.OpenAsync(payload, ZipArchiveMode.Create);
            await archive.CreateEntryFromFileAsync(source, "secret.txt");
        }
        else if (fileType is 5 or 6)
        {
            payload = source + ".enc";
            Assert.That(new Encryption().Encode(source, payload, "password"), Is.True);
        }
        var target = Path.Combine(destination, "secret.txt");
        await File.WriteAllTextAsync(target, "original contents");
        await using var server = remote ? new VersionMarkerFtpServer(await File.ReadAllBytesAsync(payload)) : null;
        var configuration = new FakeConfigurationManager { BackupFolder = root };
        using var storage = CreateStorage(configuration, server);
        storage.Open();
        using var job = CreateJob(storage, configuration, "password");
        using var table = CreateFileTable(fileType);
        using var reader = table.CreateDataReader();
        Assert.That(await reader.ReadAsync(), Is.True);

        await job.CopyFileFromDevice(storage, reader, destination);

        Assert.That(await File.ReadAllTextAsync(target), Is.EqualTo("restored contents"));
        Assert.That(Directory.GetFileSystemEntries(destination), Is.EquivalentTo(new[] { target }));
        Assert.That(File.GetLastWriteTime(target), Is.EqualTo(new DateTime(2021, 1, 2)));
    }

    [Test]
    public async Task FailedReplacementPreservesDestinationAndCleansDecodedFile()
    {
        var source = Path.Combine(root, "secret.txt");
        var target = Path.Combine(destination, "secret.txt");
        await File.WriteAllTextAsync(source, "restored contents");
        await File.WriteAllTextAsync(target, "original contents");
        var configuration = new FakeConfigurationManager { BackupFolder = root };
        using var storage = new FileSystemStorage(configuration);
        using var job = CreateJob(storage, configuration, "");
        using var table = CreateFileTable(1);
        using var reader = table.CreateDataReader();
        Assert.That(await reader.ReadAsync(), Is.True);

        using (var lockedFile = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAsync<FileNotProcessedException>(async () => await job.CopyFileFromDevice(storage, reader, destination));
        }

        Assert.That(await File.ReadAllTextAsync(target), Is.EqualTo("original contents"));
        Assert.That(Directory.GetFileSystemEntries(destination), Is.EquivalentTo(new[] { target }));
    }

    private static IStorageProvider CreateStorage(FakeConfigurationManager configuration, VersionMarkerFtpServer server)
    {
        return server is null
            ? new FileSystemStorage(configuration)
            : new FtpStorage("127.0.0.1", server.Port, "user", "pass", "/backups", "UTF-8", false, 0);
    }

    private static RestoreJob CreateJob(IStorageProvider storage, FakeConfigurationManager configuration, string password)
    {
        var db = new DbClientFactory();
        return new RestoreJob(storage, db,
            new QueryManager(db, configuration, new StorageFactory(configuration)),
            configuration, new VersionQueryRepository())
        {
            Password = password,
            FileOverwrite = FileOverwrite.Overwrite
        };
    }

    private static DataTable CreateFileTable(int fileType)
    {
        var table = new DataTable();
        table.Columns.Add("fileName", typeof(string));
        table.Columns.Add("filePath", typeof(string));
        table.Columns.Add("versionDate", typeof(string));
        table.Columns.Add("longfilename", typeof(string));
        table.Columns.Add("fileType", typeof(int));
        table.Columns.Add("fileDateCreated", typeof(DateTime));
        table.Columns.Add("fileDateModified", typeof(DateTime));
        table.Rows.Add("secret.txt", "", "", "", fileType, new DateTime(2021, 1, 1), new DateTime(2021, 1, 2));
        return table;
    }
}

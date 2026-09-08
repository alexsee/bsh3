// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using BSH.MainApp.Core.Services;
using NUnit.Framework;

namespace BSH.Test;

public class FileServiceTests
{
    [Test]
    public void SaveAndReadRoundTripsSystemTextJson()
    {
        var folder = Path.Combine(Path.GetTempPath(), "bsh-fileservice-" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new FileService();
            service.Save(folder, "settings.json", new Dictionary<string, string> { ["theme"] = "dark" });

            var loaded = service.Read<Dictionary<string, string>>(folder, "settings.json");

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded["theme"], Is.EqualTo("dark"));
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }

    [Test]
    public void ReadReturnsDefaultWhenFileIsMissing()
    {
        var service = new FileService();

        var loaded = service.Read<Dictionary<string, string>>(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");

        Assert.That(loaded, Is.Null);
    }
}

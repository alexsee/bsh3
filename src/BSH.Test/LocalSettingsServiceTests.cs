// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using BSH.MainApp.Core.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace BSH.Test;

public class LocalSettingsServiceTests
{
    [Test]
    public async Task SaveAndReadSettingRoundTripsThroughSystemTextJson()
    {
        var folderName = Path.Combine("BSH.Test", Guid.NewGuid().ToString("N"));
        var fullFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
        try
        {
            var service = new LocalSettingsService(
                new FileService(),
                Options.Create(new LocalSettingsOptions
                {
                    ApplicationDataFolder = folderName,
                    LocalSettingsFile = "LocalSettings.json"
                }));

            await service.SaveSettingAsync("theme", "dark");
            var value = await service.ReadSettingAsync<string>("theme");

            Assert.That(value, Is.EqualTo("dark"));
        }
        finally
        {
            if (Directory.Exists(fullFolder))
            {
                Directory.Delete(fullFolder, true);
            }
        }
    }
}

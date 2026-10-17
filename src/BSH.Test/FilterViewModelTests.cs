// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;
using BSH.MainApp.ViewModels.Windows;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class FilterViewModelTests
{
    [Test]
    public void LegacyMegabyteLimit_IsShownInMegabytes()
    {
        var configuration = new FakeConfigurationManager { ExcludeFileBigger = "100" };

        var viewModel = new FilterViewModel(configuration);

        Assert.That(viewModel.ExcludeFilesBigger, Is.True);
        Assert.That(viewModel.MaxFileSize, Is.EqualTo(100));
        Assert.That(viewModel.MaxFileSizeUnit, Is.EqualTo(FileSizeUnit.MB));
    }

    [Test]
    public void WholeGigabyteLimit_IsShownInGigabytes()
    {
        var configuration = new FakeConfigurationManager { ExcludeFileBigger = "4096" };

        var viewModel = new FilterViewModel(configuration);

        Assert.That(viewModel.MaxFileSize, Is.EqualTo(4));
        Assert.That(viewModel.MaxFileSizeUnit, Is.EqualTo(FileSizeUnit.GB));
    }

    [Test]
    public void SavingGigabyteLimit_StoresMegabytes()
    {
        var configuration = new FakeConfigurationManager();
        var viewModel = new FilterViewModel(configuration)
        {
            ExcludeFilesBigger = true,
            MaxFileSize = 2,
            MaxFileSizeUnit = FileSizeUnit.GB,
        };

        viewModel.SaveToConfiguration();

        Assert.That(configuration.ExcludeFileBigger, Is.EqualTo("2048"));
    }

    [Test]
    public void DisabledLimit_ClearsSetting()
    {
        var configuration = new FakeConfigurationManager { ExcludeFileBigger = "100" };
        var viewModel = new FilterViewModel(configuration) { ExcludeFilesBigger = false };

        viewModel.SaveToConfiguration();

        Assert.That(configuration.ExcludeFileBigger, Is.Empty);
    }
}

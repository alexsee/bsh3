// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Linq;
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

    [Test]
    public void AllExclusionKinds_AreShownInOneList()
    {
        var configuration = new FakeConfigurationManager
        {
            ExcludeFolder = @"C:\Data\Cache",
            ExcludeFile = @"C:\Data\big.iso",
            ExcludeFileTypes = "tmp",
            ExcludeMask = @"\.bak$",
        };

        var viewModel = new FilterViewModel(configuration);

        Assert.That(viewModel.HasNoRules, Is.False);
        Assert.That(
            viewModel.Rules.Select(rule => (rule.Kind, rule.DisplayValue)),
            Is.EqualTo(new[]
            {
                (ExclusionRuleKind.Folder, @"C:\Data\Cache"),
                (ExclusionRuleKind.File, @"C:\Data\big.iso"),
                (ExclusionRuleKind.FileType, ".tmp"),
                (ExclusionRuleKind.Pattern, @"\.bak$"),
            }));
    }

    [Test]
    public void RemovingARule_RemovesItFromTheSavedSettings()
    {
        var configuration = new FakeConfigurationManager { ExcludeFileTypes = "tmp|iso" };
        var viewModel = new FilterViewModel(configuration);

        viewModel.Rules.Single(rule => rule.Value == "tmp").RemoveCommand.Execute(null);
        viewModel.SaveToConfiguration();

        Assert.That(viewModel.Rules.Select(rule => rule.Value), Is.EqualTo(new[] { "iso" }));
        Assert.That(configuration.ExcludeFileTypes, Is.EqualTo("iso"));
    }

    [Test]
    public void AddingAFileType_AddsARuleAndClosesTheEditor()
    {
        var viewModel = new FilterViewModel(new FakeConfigurationManager());
        viewModel.ShowAddFileTypeCommand.Execute(null);
        Assert.That(viewModel.IsAddingFileType, Is.True);

        viewModel.AddFileTypeCommand.Execute(".log");

        Assert.That(viewModel.Rules.Single().DisplayValue, Is.EqualTo(".log"));
        Assert.That(viewModel.IsAddingFileType, Is.False);
        Assert.That(viewModel.FileTypeInputText, Is.Null);
    }

    [Test]
    public void InvalidPattern_CannotBeAdded()
    {
        var viewModel = new FilterViewModel(new FakeConfigurationManager());
        viewModel.ShowAddPatternCommand.Execute(null);

        Assert.That(viewModel.AddRegexCommand.CanExecute("(unclosed"), Is.False);
        Assert.That(viewModel.IsAddingPattern, Is.True);
        Assert.That(viewModel.HasNoRules, Is.True);
    }
}

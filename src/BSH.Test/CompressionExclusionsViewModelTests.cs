// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class CompressionExclusionsViewModelTests
{
    [Test]
    public void SaveCommand_NormalizesAndPersistsEditedExtensions()
    {
        var configuration = new FakeConfigurationManager
        {
            ExcludeCompression = ".zip|RAR"
        };
        var viewModel = new CompressionExclusionsViewModel(configuration);

        viewModel.AddFileTypeCommand.Execute(" *.7Z ");
        viewModel.AddFileTypeCommand.Execute("ZIP");
        viewModel.AddFileTypeCommand.Execute("foo/bar");
        viewModel.SelectedFileType = ".rar";
        viewModel.RemoveFileTypeCommand.Execute(null);
        viewModel.SaveCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(configuration.ExcludeCompression, Is.EqualTo(".zip|.7z"));
            Assert.That(viewModel.TaskCompletionSource.Task.Result, Is.True);
            Assert.That(viewModel.AddFileTypeCommand.CanExecute("foo/bar"), Is.False);
        });
    }

    [Test]
    public void CancelCommand_DoesNotPersistEditedExtensions()
    {
        var configuration = new FakeConfigurationManager
        {
            ExcludeCompression = ".zip"
        };
        var viewModel = new CompressionExclusionsViewModel(configuration);

        viewModel.AddFileTypeCommand.Execute("rar");
        viewModel.CancelCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(configuration.ExcludeCompression, Is.EqualTo(".zip"));
            Assert.That(viewModel.TaskCompletionSource.Task.Result, Is.False);
        });
    }
}

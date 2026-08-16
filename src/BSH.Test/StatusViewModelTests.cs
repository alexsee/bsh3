// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using BSH.MainApp.ViewModels.Windows;
using NUnit.Framework;

namespace BSH.Test;

public class StatusViewModelTests
{
    [Test]
    public void ReportProgressDoesNotSetZeroMaximum()
    {
        var viewModel = new StatusViewModel(dispatcherQueue: null);

        viewModel.ReportProgress(0, 0);

        Assert.That(viewModel.TotalProgress, Is.EqualTo(1));
        Assert.That(viewModel.CurrentProgress, Is.EqualTo(0));
    }

    [Test]
    public void ReportProgressNeverLeavesValueAboveMaximum()
    {
        var viewModel = new StatusViewModel(dispatcherQueue: null);
        viewModel.ReportProgress(1000, 800);

        var observedPairs = new List<(int Maximum, int Value)>();
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(StatusViewModel.TotalProgress) or nameof(StatusViewModel.CurrentProgress))
            {
                observedPairs.Add((viewModel.TotalProgress, viewModel.CurrentProgress));
            }
        };

        viewModel.ReportProgress(10, 2);

        Assert.That(observedPairs, Is.Not.Empty);
        Assert.That(observedPairs.TrueForAll(pair => pair.Value <= pair.Maximum));
        Assert.That(viewModel.TotalProgress, Is.EqualTo(10));
        Assert.That(viewModel.CurrentProgress, Is.EqualTo(2));
    }

    [Test]
    public void DetachIgnoresFurtherProgressUpdates()
    {
        var viewModel = new StatusViewModel(dispatcherQueue: null);
        viewModel.ReportProgress(50, 20);
        viewModel.Detach();

        viewModel.ReportProgress(80, 40);
        viewModel.ReportFileProgress(@"C:\tmp\file.txt");
        viewModel.ReportStatus("gone", "gone");

        Assert.That(viewModel.TotalProgress, Is.EqualTo(50));
        Assert.That(viewModel.CurrentProgress, Is.EqualTo(20));
        Assert.That(viewModel.CurrentFilePath, Is.Empty);
        Assert.That(viewModel.StatusTitle, Is.Empty);
    }
}

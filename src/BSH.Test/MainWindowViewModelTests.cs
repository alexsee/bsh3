// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using NUnit.Framework;

namespace BSH.Test;

public class MainWindowViewModelTests
{
    [Test]
    public void SetSetupModeDisablesShellNavigationAndClearsSelection()
    {
        var viewModel = new MainWindowViewModel(buildNavigationItems: false);

        viewModel.SetSetupMode(true);

        Assert.That(viewModel.IsShellNavigationEnabled, Is.False);
        Assert.That(viewModel.CurrentPage, Is.Null);
    }

    [Test]
    public void SetSetupModeRestoresShellNavigationAfterSetup()
    {
        var viewModel = new MainWindowViewModel(buildNavigationItems: false);
        viewModel.SetSetupMode(true);

        viewModel.SetSetupMode(false);

        Assert.That(viewModel.IsShellNavigationEnabled, Is.True);
    }

    [Test]
    public void HandleSupportActionIsIgnoredDuringSetup()
    {
        var viewModel = new MainWindowViewModel(buildNavigationItems: false);
        viewModel.SetSetupMode(true);

        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.CheckForUpdates), Is.False);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.About), Is.False);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ResetConfiguration), Is.False);
    }
}

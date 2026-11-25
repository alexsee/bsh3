// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using BSH.Test.Fakes;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using NUnit.Framework;

namespace BSH.Test;

public class ActivationServiceTests
{
    private const string LaunchArguments = "--startup";

    [TestCase("1", true, typeof(MainViewModel), false)]
    [TestCase("0", false, typeof(SetupViewModel), true)]
    public void EmptyFrameRoutesBeforeApplyingLaunchVisibility(
        string configured, bool showWindow, Type expectedPage, bool clearNavigation)
    {
        var shell = new TestShell(configured);

        shell.Activation.ActivateForLaunch(
            LaunchArguments, showWindow, ExtendedActivationKind.Launch, UnexpectedNotificationArgument);

        Assert.That(shell.Calls, Is.EqualTo(showWindow
            ? new[] { "check frame", "navigate", "show window" }
            : new[] { "check frame", "navigate" }));
        Assert.That(shell.Navigation, Is.EqualTo((expectedPage.FullName, (object)LaunchArguments, clearNavigation)));
    }

    [Test]
    public void ProductionConstructorRoutesAnEmptyNavigationFrameWithoutShowingTheWindow()
    {
        var shell = new TestShell("1");
        var activation = new ActivationService(shell, shell, shell.SetupRouting);

        activation.ActivateForLaunch(
            LaunchArguments, false, ExtendedActivationKind.Launch, UnexpectedNotificationArgument);

        Assert.That(shell.Calls, Is.EqualTo(new[] { "navigate" }));
        Assert.That(shell.Navigation, Is.EqualTo((typeof(MainViewModel).FullName, (object)LaunchArguments, false)));
    }

    [Test]
    public void PopulatedFrameKeepsItsCurrentPage()
    {
        var shell = new TestShell("1") { HasPage = true };

        shell.Activation.ActivateForLaunch(
            LaunchArguments, true, null, UnexpectedNotificationArgument);

        Assert.That(shell.Calls, Is.EqualTo(new[] { "check frame", "show window" }));
    }

    [TestCase("action=open-browser")]
    [TestCase(null)]
    public void NotificationIsForwardedBeforeRoutingAndShowingTheWindow(string? argument)
    {
        var shell = new TestShell("1");

        shell.Activation.ActivateForLaunch(
            LaunchArguments, true, ExtendedActivationKind.AppNotification, () => argument);

        Assert.That(shell.Calls, Is.EqualTo(new[] { "notify", "check frame", "navigate", "show window" }));
        Assert.That(shell.NotificationArgument, Is.EqualTo(argument));
        Assert.That(shell.Navigation, Is.EqualTo((typeof(MainViewModel).FullName, (object)LaunchArguments, false)));
    }

    [Test]
    public async Task TrayMainWindowRoutesToSetupBeforeShowing()
    {
        var shell = new TestShell("0");
        var presentation = new PresentationService(shell.Activation, shell.SetupRouting, shell);

        await presentation.ShowMainWindowAsync();

        Assert.That(shell.Calls, Is.EqualTo(new[] { "navigate", "show window" }));
        Assert.That(shell.Navigation, Is.EqualTo((typeof(SetupViewModel).FullName, (object?)null, true)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task TrayBackupBrowserShowsBeforeCheckingWhetherNavigationIsEnabled(bool navigationEnabled)
    {
        var shell = new TestShell("1");
        var presentation = new PresentationService(shell.Activation, shell.SetupRouting, shell, () =>
        {
            shell.Calls.Add("check shell navigation");
            return navigationEnabled;
        });

        await presentation.ShowBackupBrowserWindowAsync();

        Assert.That(shell.Calls, Is.EqualTo(navigationEnabled
            ? new[] { "show window", "check shell navigation", "navigate" }
            : new[] { "show window", "check shell navigation" }));
        if (navigationEnabled)
        {
            Assert.That(shell.Navigation, Is.EqualTo((typeof(BrowserViewModel).FullName, (object?)null, false)));
        }
    }

    private static string UnexpectedNotificationArgument() =>
        throw new InvalidOperationException("Only notification activations should read the notification argument.");

    private sealed class TestShell : INavigationService, IAppNotificationService
    {
        public List<string> Calls { get; } = new();
        public bool HasPage { get; init; }
        public (string Page, object? Parameter, bool ClearNavigation) Navigation { get; private set; }
        public string? NotificationArgument { get; private set; }
        public SetupRouting SetupRouting { get; }
        public ActivationService Activation { get; }
        public Frame? Frame { get; set; }

        public TestShell(string configured)
        {
            SetupRouting = new SetupRouting(new FakeConfigurationManager { IsConfigured = configured }, this);
            Activation = new ActivationService(this, SetupRouting, HasFrameContent, () => Calls.Add("show window"));
        }

        private bool HasFrameContent()
        {
            Calls.Add("check frame");
            return HasPage;
        }

        public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
        {
            Calls.Add("navigate");
            Navigation = (pageKey, parameter, clearNavigation);
            return true;
        }

        public void Activate(string? arguments)
        {
            Calls.Add("notify");
            NotificationArgument = arguments;
        }

        public void Initialize() => throw new NotSupportedException();
        public bool Show(string payload) => throw new NotSupportedException();
        public void Unregister() => throw new NotSupportedException();
    }
}

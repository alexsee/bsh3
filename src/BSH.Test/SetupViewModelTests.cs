// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class SetupViewModelTests
{
    [Test]
    public void NewFtpConfiguration_DefaultsToUtf8NoBomEncoding()
    {
        var viewModel = new SetupViewModel(
            new FakeConfigurationManager(),
            setupService: null!,
            orchestrationService: null!,
            jobService: null!,
            navigationService: null!,
            presentationService: null!);

        Assert.That(viewModel.FtpEncoding, Is.EqualTo("UTF-8"));
        Assert.That(viewModel.ImportFtpEncoding, Is.EqualTo("UTF-8"));
    }
}

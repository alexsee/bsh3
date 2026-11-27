// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;
using BSH.MainApp.ViewModels.Windows;
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

    [Test]
    public void ChangingFtpEndpointsClearsTheCorrespondingCertificateException()
    {
        var setup = new SetupViewModel(new FakeConfigurationManager(), null!, null!, null!, null!, null!);
        var switchStorage = new SwitchStorageViewModel(null!);

        setup.FtpAllowUntrustedCertificate = true;
        setup.ImportFtpAllowUntrustedCertificate = true;
        setup.FtpHost = "backup.example";
        Assert.That(setup.FtpAllowUntrustedCertificate, Is.False);
        Assert.That(setup.ImportFtpAllowUntrustedCertificate, Is.True);

        setup.ImportFtpHost = "import.example";
        Assert.That(setup.ImportFtpAllowUntrustedCertificate, Is.False);

        setup.FtpAllowUntrustedCertificate = true;
        setup.ImportFtpAllowUntrustedCertificate = true;
        setup.FtpPort = "1990";
        setup.ImportFtpPort = "1991";
        Assert.That(setup.FtpAllowUntrustedCertificate, Is.False);
        Assert.That(setup.ImportFtpAllowUntrustedCertificate, Is.False);

        switchStorage.FtpAllowUntrustedCertificate = true;
        switchStorage.FtpHost = "switch.example";
        Assert.That(switchStorage.FtpAllowUntrustedCertificate, Is.False);
        switchStorage.FtpAllowUntrustedCertificate = true;
        switchStorage.FtpPort = "1990";
        Assert.That(switchStorage.FtpAllowUntrustedCertificate, Is.False);
    }
}

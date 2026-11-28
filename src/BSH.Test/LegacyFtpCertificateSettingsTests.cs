// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

[Apartment(ApartmentState.STA)]
public class LegacyFtpCertificateSettingsTests
{
    [Test]
    public void ChangingEndpointClearsCertificateExceptionInEachFtpForm()
    {
        using var settings = new ucConfig();
        using var changeMedia = new frmChangeMedia();
        using var wizard = new ucDoConfigure();

        AssertCertificateExceptionClears(settings, "");
        AssertCertificateExceptionClears(changeMedia, "");
        AssertCertificateExceptionClears(wizard, "");
        AssertCertificateExceptionClears(wizard, "2");
    }

    private static void AssertCertificateExceptionClears(Control root, string suffix)
    {
        var exception = (CheckBox)root.Controls.Find("chkFtpAllowUntrustedCertificate" + suffix, true).Single();
        var host = (TextBox)root.Controls.Find("txtFTPServer" + suffix, true).Single();
        var port = (TextBox)root.Controls.Find("txtFTPPort" + suffix, true).Single();

        exception.Checked = true;
        host.Text = "different.example";
        Assert.That(exception.Checked, Is.False);

        exception.Checked = true;
        port.Text = "1990";
        Assert.That(exception.Checked, Is.False);
    }
}

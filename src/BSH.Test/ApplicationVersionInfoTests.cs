// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Services;
using NUnit.Framework;

namespace BSH.Test;

public class ApplicationVersionInfoTests
{
    [Test]
    public void GetDisplayVersion_PreservesPrereleaseLabelAndRemovesBuildMetadata()
    {
        var result = ApplicationVersionInfo.GetDisplayVersion("4.0.0-beta2+abcdef", "4.0.0.0");

        Assert.That(result, Is.EqualTo("4.0.0-beta2"));
    }

    [Test]
    public void GetDisplayVersion_FallsBackToAssemblyVersion()
    {
        var result = ApplicationVersionInfo.GetDisplayVersion(null, "4.0.0.0");

        Assert.That(result, Is.EqualTo("4.0.0.0"));
    }
}


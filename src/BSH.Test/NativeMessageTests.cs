// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using BSH.MainApp;
using NUnit.Framework;

namespace BSH.Test;

public class NativeMessageTests
{
    [Test]
    public void ShowUsesTheOverrideAndSurvivesAFailure()
    {
        string caption = null;
        string text = null;
        NativeMessage.ShowOverride = (shownCaption, shownText) =>
        {
            caption = shownCaption;
            text = shownText;
        };

        try
        {
            NativeMessage.Show("Preview", "Busy");
            Assert.That(caption, Is.EqualTo("Preview"));
            Assert.That(text, Is.EqualTo("Busy"));

            NativeMessage.ShowOverride = (_, _) => throw new InvalidOperationException("dialog failed");
            Assert.DoesNotThrow(() => NativeMessage.Show("Preview", "Busy"));
        }
        finally
        {
            NativeMessage.ShowOverride = null;
        }
    }
}

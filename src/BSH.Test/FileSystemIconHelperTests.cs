// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using BSH.MainApp.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using NUnit.Framework;

namespace BSH.Test;

public class FileSystemIconHelperTests
{
    [Test]
    public void IconCacheKey_NormalizesExtensionCaseAndRejectsMissingExtension()
    {
        Assert.That(FileSystemIconHelpers.GetIconCacheKey("Notes.TXT", 16), Is.EqualTo(".txt_16"));
        Assert.That(FileSystemIconHelpers.GetIconCacheKey(@"D:\Docs\other.txt", 16), Is.EqualTo(".txt_16"));
        Assert.That(FileSystemIconHelpers.GetIconCacheKey("other.txt", 64), Is.EqualTo(".txt_64"));
        Assert.That(FileSystemIconHelpers.GetIconCacheKey("archive.tar.GZ", 32), Is.EqualTo(".gz_32"));
        Assert.That(FileSystemIconHelpers.GetIconCacheKey("README", 16), Is.Null);
        Assert.That(FileSystemIconHelpers.GetIconCacheKey("README.", 16), Is.Null);
        Assert.That(FileSystemIconHelpers.GetIconCacheKey(null, 16), Is.Null);
        Assert.That(FileSystemIconHelpers.GetIconCacheKey(string.Empty, 16), Is.Null);
        FileSystemIconHelpers.ClearIconCache();
    }

    [Test]
    public async Task GetFileIconAsync_WithoutExtension_ReturnsNull()
    {
        Assert.That(await FileSystemIconHelpers.GetFileIconAsync("README"), Is.Null);
        Assert.That(await FileSystemIconHelpers.GetFileIconAsync("README."), Is.Null);
        Assert.That(await FileSystemIconHelpers.GetFileIconAsync(string.Empty), Is.Null);
    }

    [Test]
    public void ToDevicePixels_ScalesAbove96Dpi()
    {
        Assert.That(FileSystemIconHelpers.ToDevicePixels(16, 96), Is.EqualTo(16));
        Assert.That(FileSystemIconHelpers.ToDevicePixels(16, 0), Is.EqualTo(16));
        Assert.That(FileSystemIconHelpers.ToDevicePixels(16, 192), Is.EqualTo(32));
        Assert.That(FileSystemIconHelpers.ToDevicePixels(16, 144), Is.EqualTo(24));
        Assert.That(FileSystemIconHelpers.ToDevicePixels(64, 144), Is.EqualTo(96));
        Assert.That(FileSystemIconHelpers.ToDevicePixels(0, 192), Is.EqualTo(0));
        Assert.That(FileSystemIconHelpers.ToDevicePixels(uint.MaxValue, 192), Is.EqualTo(uint.MaxValue));
    }

    [Test]
    public void SelectIconTier_PicksSmallestListThatCoversTheSize()
    {
        Assert.That(FileSystemIconHelpers.SelectIconTier(16), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.Small));
        Assert.That(FileSystemIconHelpers.SelectIconTier(17), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.Large));
        Assert.That(FileSystemIconHelpers.SelectIconTier(32), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.Large));
        Assert.That(FileSystemIconHelpers.SelectIconTier(33), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.ExtraLarge));
        Assert.That(FileSystemIconHelpers.SelectIconTier(48), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.ExtraLarge));
        Assert.That(FileSystemIconHelpers.SelectIconTier(49), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.Jumbo));
        Assert.That(FileSystemIconHelpers.SelectIconTier(64), Is.EqualTo(FileSystemIconHelpers.ShellIconTier.Jumbo));
    }

    [Test]
    public void ApplyStraightAlpha_PremultipliesAndUsesMaskWhenColorAlphaIsMissing()
    {
        var straight = new byte[] { 10, 20, 30, 128 };
        FileSystemIconHelpers.ApplyStraightAlpha(straight, maskPixels: null);
        Assert.That(straight, Is.EqualTo(new byte[] { 5, 10, 15, 128 }));

        var opaque = new byte[] { 10, 20, 30, 255 };
        FileSystemIconHelpers.ApplyStraightAlpha(opaque, maskPixels: null);
        Assert.That(opaque, Is.EqualTo(new byte[] { 10, 20, 30, 255 }));

        var masked = new byte[] { 10, 20, 30, 0, 10, 20, 30, 0 };
        var mask = new byte[] { 255, 255, 255, 0, 0, 0, 0, 255 };
        FileSystemIconHelpers.ApplyStraightAlpha(masked, mask);
        Assert.That(masked, Is.EqualTo(new byte[] { 0, 0, 0, 0, 10, 20, 30, 255 }));

        var shortMask = new byte[] { 1, 2, 3, 0 };
        FileSystemIconHelpers.ApplyStraightAlpha(shortMask, new byte[] { 9 });
        Assert.That(shortMask, Is.EqualTo(new byte[] { 1, 2, 3, 255 }));
    }

    [Test]
    public async Task GetFileIconAsync_DoesNotCreateOrDeleteExtensionNamedAppDataFile()
    {
        var controller = DispatcherQueueController.CreateOnDedicatedThread();
        try
        {
            await InvokeAsync(controller.DispatcherQueue, ProbeAppDataFile);
        }
        finally
        {
            FileSystemIconHelpers.ClearIconCache();
            await controller.ShutdownQueueAsync();
        }
    }

    private static void ProbeAppDataFile()
    {
        using var xaml = WindowsXamlManager.InitializeForCurrentThread();
        FileSystemIconHelpers.ClearIconCache();

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var sentinel = Path.Combine(appData, ".txt");
        var prefixed = Path.Combine(appData, "fileicon_cache.txt");
        var currentDirectoryFile = Path.Combine(Environment.CurrentDirectory, ".txt");
        var extension = ".bsh676" + Guid.NewGuid().ToString("N");
        var unexpected = Path.Combine(appData, extension);
        var currentDirectoryExisted = File.Exists(currentDirectoryFile);
        var currentDirectoryBytes = currentDirectoryExisted ? File.ReadAllBytes(currentDirectoryFile) : null;
        var createdSentinel = false;
        byte[] originalSentinel = null;
        var prefixedExisted = File.Exists(prefixed);
        byte[] originalPrefixed = prefixedExisted ? File.ReadAllBytes(prefixed) : null;

        try
        {
            if (File.Exists(sentinel))
            {
                originalSentinel = File.ReadAllBytes(sentinel);
            }
            else
            {
                File.WriteAllBytes(sentinel, Encoding.UTF8.GetBytes("bsh-issue-676-sentinel"));
                createdSentinel = true;
                originalSentinel = File.ReadAllBytes(sentinel);
            }

            Assert.That(File.Exists(unexpected), Is.False);

            var icon = FileSystemIconHelpers.LoadFileIcon("notes.txt", 16);
            var sameCase = FileSystemIconHelpers.LoadFileIcon("NOTES.TXT", 16);
            var larger = FileSystemIconHelpers.LoadFileIcon("notes.txt", 64);
            var unknown = FileSystemIconHelpers.LoadFileIcon("sample" + extension, 16);
            var wrapped = FileSystemIconHelpers.GetFileIconAsync("notes.txt", 16).GetAwaiter().GetResult();

            Assert.That(File.Exists(sentinel), Is.True);
            Assert.That(File.ReadAllBytes(sentinel), Is.EqualTo(originalSentinel));
            Assert.That(File.Exists(unexpected), Is.False);
            Assert.That(File.Exists(currentDirectoryFile), Is.EqualTo(currentDirectoryExisted));
            if (currentDirectoryExisted)
            {
                Assert.That(File.ReadAllBytes(currentDirectoryFile), Is.EqualTo(currentDirectoryBytes));
            }

            Assert.That(File.Exists(prefixed), Is.EqualTo(prefixedExisted));
            if (prefixedExisted)
            {
                Assert.That(File.ReadAllBytes(prefixed), Is.EqualTo(originalPrefixed));
            }

            Assert.That(icon, Is.Not.Null);
            Assert.That(HasVisiblePixel(icon), Is.True);
            Assert.That(sameCase, Is.SameAs(icon));
            Assert.That(wrapped, Is.SameAs(icon));
            Assert.That(larger, Is.Not.Null);
            Assert.That(larger, Is.Not.SameAs(icon));
            Assert.That(unknown, Is.Not.Null);
            Assert.That(FileSystemIconHelpers.LoadFileIcon("README"), Is.Null);
        }
        finally
        {
            if (createdSentinel)
            {
                if (File.Exists(sentinel))
                {
                    File.Delete(sentinel);
                }
            }
            else if (originalSentinel != null &&
                     (!File.Exists(sentinel) || !BytesEqual(File.ReadAllBytes(sentinel), originalSentinel)))
            {
                File.WriteAllBytes(sentinel, originalSentinel);
            }
        }
    }

    private static bool HasVisiblePixel(BitmapSource icon)
    {
        var bitmap = (WriteableBitmap)icon;
        using var stream = bitmap.PixelBuffer.AsStream();
        stream.Position = 0;
        var buffer = new byte[stream.Length];
        var read = stream.Read(buffer, 0, buffer.Length);
        for (var i = 3; i < read; i += 4)
        {
            if (buffer[i] != 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool BytesEqual(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }

    private static Task InvokeAsync(DispatcherQueue queue, Action action)
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!queue.TryEnqueue(() =>
            {
                try
                {
                    action();
                    source.SetResult();
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            }))
        {
            source.SetException(new InvalidOperationException("Could not enqueue the icon probe."));
        }

        return source.Task;
    }
}

// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

[Apartment(ApartmentState.STA)]
public class LegacyListLifetimeTests
{
    [Test]
    public void RepeatedRedrawThenClickNavigatesOnce()
    {
        using var list = new aVersionList();
        var item = new ClickableVersionItem();
        list.Items.Add(item);
        var navigations = 0;
        list.ItemClick += _ =>
        {
            navigations++;
            return Task.CompletedTask;
        };

        for (var i = 0; i < 5; i++)
        {
            list.DrawItems();
        }
        item.ClickItem();

        Assert.That(navigations, Is.EqualTo(1));
        Assert.That(item.IsDisposed, Is.False);
    }

    [Test]
    public void ReplacementDisposesRemovedVersionAndRetainsLiveVersion()
    {
        using var list = new aVersionList();
        var removed = new ClickableVersionItem();
        var retained = new ClickableVersionItem();
        list.Items.Add(removed);
        list.Items.Add(retained);
        list.DrawItems();
        var navigations = 0;
        list.ItemClick += _ =>
        {
            navigations++;
            return Task.CompletedTask;
        };

        list.Items.Remove(removed);
        list.DrawItems();
        removed.ClickItem();
        Assert.That(navigations, Is.Zero);
        retained.ClickItem();

        Assert.That(removed.IsDisposed, Is.True);
        Assert.That(retained.IsDisposed, Is.False);
        Assert.That(navigations, Is.EqualTo(1));
        list.Dispose();
        Assert.That(retained.IsDisposed, Is.True);
        retained.ClickItem();
        Assert.That(navigations, Is.EqualTo(1));
    }

    [Test]
    public void ReplacingBreadcrumbsDisposesPreviousControls()
    {
        using var navigation = new ucNavigation
        {
            Path = @"Documents\Photos",
            PathLocalized = @"Documents\Photos"
        };
        navigation.CreateNavi(@"C:\Documents\Photos");
        var panel = navigation.Controls.Find("flpNavi", true).Single();
        var previousItems = panel.Controls.Cast<ucNavigationItem>().ToArray();

        navigation.Path = "Documents";
        navigation.PathLocalized = "Documents";
        navigation.CreateNavi(@"C:\Documents");

        Assert.That(previousItems, Has.Length.EqualTo(2));
        Assert.That(previousItems.All(item => item.IsDisposed), Is.True);
        Assert.That(panel.Controls.Count, Is.EqualTo(1));
        Assert.That(panel.Controls[0].IsDisposed, Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task VersionClickAwaitsSubscribersInOrder(bool selectItem)
    {
        using var list = new aVersionList();
        var item = new ClickableVersionItem { VersionID = "version" };
        list.Items.Add(item);
        list.DrawItems();
        var releaseFirst = new TaskCompletionSource();
        var releaseSecond = new TaskCompletionSource();
        var secondEntered = new TaskCompletionSource();
        var completed = new TaskCompletionSource();
        var firstFinished = false;
        var secondStarted = false;
        list.ItemClick += async _ =>
        {
            await releaseFirst.Task;
            firstFinished = true;
        };
        list.ItemClick += async _ =>
        {
            secondStarted = true;
            secondEntered.SetResult();
            await releaseSecond.Task;
            completed.SetResult();
        };

        Task selection = null;
        if (selectItem)
        {
            selection = list.SelectItemAsync(item.VersionID);
        }
        else
        {
            item.ClickItem();
        }

        var startedBeforeFirstFinished = secondStarted;
        var selectionCompletedBeforeFirst = selection?.IsCompleted;
        releaseFirst.SetResult();
        await secondEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var selectionCompletedBeforeSecond = selection?.IsCompleted;
        releaseSecond.SetResult();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (selection != null)
        {
            await selection.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.That(selectionCompletedBeforeFirst, Is.False);
            Assert.That(selectionCompletedBeforeSecond, Is.False);
        }

        Assert.That(startedBeforeFirstFinished, Is.False);
        Assert.That(firstFinished, Is.True);
    }

    [Test]
    public void HighlightingVersionDoesNotNavigate()
    {
        using var list = new aVersionList();
        var item = new ClickableVersionItem { VersionID = "version" };
        list.Items.Add(item);
        list.DrawItems();
        var navigations = 0;
        list.ItemClick += _ =>
        {
            navigations++;
            return Task.CompletedTask;
        };

        list.HighlightItem(item.VersionID);

        Assert.That(navigations, Is.Zero);
    }

    private sealed class ClickableVersionItem : aVersionListItem
    {
        public void ClickItem() => OnClick(EventArgs.Empty);
    }
}

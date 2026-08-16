// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Brightbits.BSH.Engine.Jobs;
using BSH.MainApp.ViewModels.Windows;
using BSH.MainApp.Windows;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class ModalDialogViewModelTests
{
    [Test]
    public async Task OverwriteCommand_CompletesWaitWithOverwrite()
    {
        var viewModel = new RequestFileOverwriteViewModel();

        viewModel.OverwriteFileCommand.Execute(null);

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.EqualTo(RequestOverwriteResult.Overwrite));
    }

    [Test]
    public async Task SkipCommand_CompletesWaitWithNoOverwrite()
    {
        var viewModel = new RequestFileOverwriteViewModel();

        viewModel.SkipFileCommand.Execute(null);

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.EqualTo(RequestOverwriteResult.NoOverwrite));
    }

    [Test]
    public async Task CancelCommand_CompletesWaitWithNone()
    {
        var viewModel = new RequestFileOverwriteViewModel();

        viewModel.CancelCommand.Execute(null);

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.EqualTo(RequestOverwriteResult.None));
    }

    [Test]
    public async Task OverwriteWindowClosed_HasSameOutcomeAsCancel()
    {
        var viewModel = new RequestFileOverwriteViewModel();

        viewModel.OnWindowClosed();

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.EqualTo(RequestOverwriteResult.None));
    }

    [Test]
    public async Task OverwriteWindowClosed_AfterCancel_StillCompletesWait()
    {
        var viewModel = new RequestFileOverwriteViewModel();

        viewModel.CancelCommand.Execute(null);
        viewModel.OnWindowClosed();

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.EqualTo(RequestOverwriteResult.None));
    }

    [Test]
    public async Task NewBackupWindowClosed_CompletesWaitAsCanceled()
    {
        var viewModel = new NewBackupViewModel();

        viewModel.OnWindowClosed();

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.False);
    }

    [Test]
    public async Task NewBackupWindowClosed_AfterCancel_StillCompletesWait()
    {
        var viewModel = new NewBackupViewModel();

        viewModel.CancelCommand.Execute(null);
        viewModel.OnWindowClosed();

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.False);
    }

    [Test]
    public async Task EditBackupWindowClosed_CompletesWaitAsCanceled()
    {
        var viewModel = new EditBackupViewModel();

        viewModel.OnWindowClosed();

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.False);
    }

    [Test]
    public async Task EditBackupCancel_CompletesWaitOnTheInstanceThatHoldsEditedFields()
    {
        var viewModel = new EditBackupViewModel();
        viewModel.Title = "Edited title";
        viewModel.Description = "Edited description";

        viewModel.CancelCommand.Execute(null);

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.False);
        Assert.That(viewModel.Title, Is.EqualTo("Edited title"));
        Assert.That(viewModel.Description, Is.EqualTo("Edited description"));
    }

    [Test]
    public async Task FilterWindowClosed_CompletesWaitAsCanceled()
    {
        var viewModel = new FilterViewModel(new FakeConfigurationManager());

        viewModel.OnWindowClosed();

        Assert.That(await viewModel.TaskCompletionSource.Task, Is.False);
    }

    [Test]
    public async Task DismissingDialog_CompletesWaitWithoutClosingTwice()
    {
        var handler = new ModalCloseHandler();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeCalled = false;

        var wait = handler.AwaitThenCloseAsync(completion.Task, () => closeCalled = true);
        handler.HandleClosed(() => completion.TrySetResult(false));

        Assert.That(await wait, Is.False);
        Assert.That(closeCalled, Is.False);
    }

    [Test]
    public async Task ConfirmingDialog_ClosesWindowAfterWaitCompletes()
    {
        var handler = new ModalCloseHandler();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeCalled = false;

        var wait = handler.AwaitThenCloseAsync(completion.Task, () => closeCalled = true);
        completion.TrySetResult(true);

        Assert.That(await wait, Is.True);
        Assert.That(closeCalled, Is.True);
    }
}

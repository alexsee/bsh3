// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels.Windows;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class ScheduleEditorViewModelTests
{
    private static readonly DateTime Now = new(2026, 10, 17, 12, 30, 0);

    private static (ScheduleEditorViewModel ViewModel, FakeConfigurationManager Configuration, InMemoryScheduleRepository Repository) Create(params ScheduleEntry[] entries)
    {
        var configuration = new FakeConfigurationManager();
        var repository = new InMemoryScheduleRepository(entries);
        var viewModel = new ScheduleEditorViewModel(
            new ScheduleSettingsService(configuration, repository),
            configuration,
            new NoOpOrchestrationService(),
            () => Now);
        return (viewModel, configuration, repository);
    }

    [Test]
    public async Task EmptySchedule_WarnsThatNoBackupsWillRun()
    {
        var (viewModel, _, _) = Create();

        await viewModel.InitializeAsync();

        Assert.That(viewModel.HasNoEntries, Is.True);
        Assert.That(viewModel.HasScheduleWarning, Is.True);
    }

    [Test]
    public async Task AddingADailyTime_ClearsTheWarning()
    {
        var (viewModel, _, _) = Create();
        await viewModel.InitializeAsync();

        viewModel.SelectedScheduleKind = ScheduleEntryKind.Daily;
        viewModel.StartTime = new TimeSpan(14, 0, 0);
        viewModel.AddScheduleCommand.Execute(null);

        Assert.That(viewModel.Entries, Has.Count.EqualTo(1));
        Assert.That(viewModel.HasScheduleWarning, Is.False);
    }

    [Test]
    public async Task OnlyPastOneTimeEntries_StillWarn()
    {
        var (viewModel, _, _) = Create(new ScheduleEntry { Type = (int)ScheduleEntryKind.Once, Date = Now.AddDays(-1) });

        await viewModel.InitializeAsync();

        Assert.That(viewModel.HasNoEntries, Is.False);
        Assert.That(viewModel.HasScheduleWarning, Is.True);
    }

    [Test]
    public async Task RemovingARow_DeletesThatEntryOnSave()
    {
        var keep = new ScheduleEntry { Type = (int)ScheduleEntryKind.Daily, Date = new DateTime(2020, 1, 1, 9, 0, 0) };
        var remove = new ScheduleEntry { Type = (int)ScheduleEntryKind.Weekly, Date = new DateTime(2026, 10, 12, 8, 0, 0) };
        var (viewModel, _, repository) = Create(keep, remove);
        await viewModel.InitializeAsync();

        viewModel.Entries.Single(entry => entry.Entry == remove).DeleteCommand.Execute(null);
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.That(repository.Entries, Is.EqualTo(new[] { keep }));
    }

    [Test]
    public async Task RetentionChoice_IsSavedFromTheRadioOptions()
    {
        var (viewModel, configuration, _) = Create();
        await viewModel.InitializeAsync();
        Assert.That(viewModel.IsKeepAll, Is.True);

        viewModel.IsKeepInterval = true;
        viewModel.RetentionInterval = 3;
        viewModel.RetentionIntervalUnit = ScheduleRetentionIntervalUnit.Week;
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.That(viewModel.IsKeepAll, Is.False);
        Assert.That(configuration.IntervallDelete, Is.EqualTo("week|3"));
    }

    [Test]
    public async Task UncheckingARadioOption_DoesNotChangeTheRetention()
    {
        var (viewModel, _, _) = Create();
        await viewModel.InitializeAsync();
        viewModel.IsKeepAutomatic = true;

        viewModel.IsKeepAll = false;

        Assert.That(viewModel.RetentionMode, Is.EqualTo(ScheduleRetentionMode.Automatic));
    }

    private sealed class InMemoryScheduleRepository : IScheduleRepository
    {
        public InMemoryScheduleRepository(IEnumerable<ScheduleEntry> entries)
        {
            Entries = entries.ToList();
        }

        public List<ScheduleEntry> Entries { get; private set; }

        public Task<bool> HasScheduleEntriesAsync() => Task.FromResult(Entries.Count > 0);

        public Task<IReadOnlyList<ScheduleEntry>> GetSchedulesAsync() => Task.FromResult<IReadOnlyList<ScheduleEntry>>(Entries.ToList());

        public Task ReplaceSchedulesAsync(IEnumerable<ScheduleEntry> schedules)
        {
            Entries = schedules.ToList();
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpOrchestrationService : IOrchestrationService
    {
        public Task InitializeAsync() => Task.CompletedTask;
        public Task StartAsync(bool turnOn = false) => Task.CompletedTask;
        public Task StopAsync(bool turnOff = false) => Task.CompletedTask;
        public Task RefreshAutomationAsync() => Task.CompletedTask;
    }
}

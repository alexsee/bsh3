using System.Windows.Input;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware, IStatusReport
{
    private readonly IStatusService statusService;
    private readonly IQueryManager queryManager;
    private readonly IJobService jobService;
    private readonly IScheduledBackupService scheduledBackupService;
    private readonly DispatcherQueue dispatcherQueue;
    [ObservableProperty]
    private string? lastBackupDate;

    [ObservableProperty]
    private DateTime? nextBackupDate;

    [ObservableProperty]
    private Visibility nextBackupGridVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility progressGridVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private string? currentProgressStatusTitle;

    [ObservableProperty]
    private string? currentProgressStatusText;

    [ObservableProperty]
    private int? currentProgressValue;

    [ObservableProperty]
    private int? currentProgressMax;

    public ICommand StartManualBackupCommand
    {
        get;
    }

    public MainViewModel(
        IStatusService statusService,
        IQueryManager queryManager,
        IJobService jobService,
        IScheduledBackupService scheduledBackupService,
        DispatcherQueue dispatcherQueue)
    {
        this.statusService = statusService;
        this.queryManager = queryManager;
        this.jobService = jobService;
        this.scheduledBackupService = scheduledBackupService;
        this.dispatcherQueue = dispatcherQueue;
        this.statusService.AddObserver(this, true);

        // init commands
        StartManualBackupCommand = new AsyncRelayCommand(StartManualBackupCommandAsync);
    }

    public async void OnNavigatedTo(object parameter)
    {
        LastBackupDate = (await queryManager.GetLastBackupAsync()).CreationDate.ToLongDateString();
        NextBackupDate = scheduledBackupService.GetNextBackupDate();
    }

    public void OnNavigatedFrom()
    {
        this.statusService.RemoveObserver(this);
    }

    private async Task StartManualBackupCommandAsync()
    {
        await jobService.CreateBackupAsync("MainView.BtnCreateBackup.Title", "", true);
    }

    public void ReportAction(ActionType action, bool silent)
    {
    }

    public void ReportState(JobState jobState)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            if (jobState == JobState.RUNNING)
            {
                NextBackupGridVisibility = Visibility.Collapsed;
                ProgressGridVisibility = Visibility.Visible;
                return;
            }

            NextBackupGridVisibility = Visibility.Visible;
            ProgressGridVisibility = Visibility.Collapsed;
        });
    }

    public void ReportStatus(string title, string text)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            CurrentProgressStatusTitle = title;
            CurrentProgressStatusText = text;
        });
    }

    public void ReportProgress(int total, int current)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            CurrentProgressMax = total;
            CurrentProgressValue = current;
        });
    }

    public void ReportFileProgress(string file)
    {
    }

    public void ReportSystemStatus(SystemStatus systemStatus)
    {
    }
}

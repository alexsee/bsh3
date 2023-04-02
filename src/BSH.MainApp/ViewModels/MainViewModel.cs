using Brightbits.BSH.Engine.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BSH.MainApp.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly IStatusService statusService;
    private readonly IQueryManager queryManager;

    [ObservableProperty]
    private string? lastBackupDate;

    public MainViewModel(IStatusService statusService, IQueryManager queryManager)
    {
        this.statusService = statusService;
        this.queryManager = queryManager;
    }

    public async void OnNavigatedTo(object parameter)
    {
        LastBackupDate = (await queryManager.GetLastBackupAsync()).CreationDate.ToLongDateString();
    }

    public void OnNavigatedFrom()
    {
    }
}

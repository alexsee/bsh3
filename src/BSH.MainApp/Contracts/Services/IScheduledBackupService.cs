namespace BSH.MainApp.Contracts.Services;

public interface IScheduledBackupService
{
    Task InitializeAsync();

    Task StartAsync();

    void Stop();

    DateTime GetNextBackupDate();
}
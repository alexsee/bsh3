namespace BSH.MainApp.Contracts.Services;

public interface IOrchestrationService
{
    Task InitializeAsync();
    Task StartAsync(bool turnOn = false);
    Task StopAsync(bool turnOff = false);
}
namespace BSH.MainApp.Contracts.Services;

public interface IWaitForMediaService
{
    Task<bool> ExecuteAsync(bool silent, CancellationTokenSource cancellationTokenSource);
}
namespace BSH.MainApp.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}

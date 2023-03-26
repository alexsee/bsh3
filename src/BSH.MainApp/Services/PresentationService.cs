using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public class PresentationService : IPresentationService
{
    public void ShowStatusWindow()
    {
    }

    public TaskCompleteAction CloseStatusWindow()
    {
        return TaskCompleteAction.NoAction;
    }

    public void ShowMainWindow()
    {
    }

    public void CloseMainWindow()
    {
    }

    public void ShowBackupBrowserWindow()
    {
    }

    public void CloseBackupBrowserWindow()
    {
    }


    public void ShowAboutWindow()
    {
    }

    public (string password, bool persist) RequestPassword()
    {
        return (null, false);
    }

    public void ShowErrorInsufficientDiskSpace()
    {
    }

    public async Task ShowCreateBackupWindow()
    {
    }

    public async Task<IUICommand> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
    {
        if (commands != null && commands.Count > 3)
            throw new InvalidOperationException("A maximum of 3 commands can be specified");

        IUICommand defaultCommand = new UICommand("OK");
        IUICommand? secondaryCommand = null;
        IUICommand? cancelCommand = null;
        if (commands != null)
        {
            defaultCommand = commands.Count > defaultCommandIndex ? commands[(int)defaultCommandIndex] : commands.FirstOrDefault() ?? defaultCommand;
            cancelCommand = commands.Count > cancelCommandIndex ? commands[(int)cancelCommandIndex] : null;
            secondaryCommand = commands.FirstOrDefault(c => c != defaultCommand && c != cancelCommand);
        }
        var dialog = new ContentDialog();
        dialog.Content = new TextBlock() { Text = content };
        dialog.Title = title;
        dialog.PrimaryButtonText = defaultCommand.Label;
        if (secondaryCommand != null)
        {
            dialog.SecondaryButtonText = secondaryCommand.Label;
        }
        if (cancelCommand != null)
        {
            dialog.CloseButtonText = cancelCommand.Label;
        }
        var result = await dialog.ShowAsync();
        switch (result)
        {
            case ContentDialogResult.Primary:
                return defaultCommand;
            case ContentDialogResult.Secondary:
                return secondaryCommand!;
            case ContentDialogResult.None:
            default:
                return cancelCommand ?? new UICommand();
        }
    }
}

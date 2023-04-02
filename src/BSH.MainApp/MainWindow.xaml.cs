using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;

namespace BSH.MainApp;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/app_ico.ico"));
        Title = "AppDisplayName".GetLocalized();
    }

    private void MainNavigation_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer == nviOverviewPage)
        {
            App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.MainViewModel");
        }
        else if (args.InvokedItemContainer == nviBackupBrowser)
        {
            App.GetService<INavigationService>().NavigateTo("BSH.MainApp.ViewModels.BrowserViewModel");
        }
    }
}

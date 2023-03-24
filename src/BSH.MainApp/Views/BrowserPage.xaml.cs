using BSH.MainApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views;

public sealed partial class BrowserPage : Page
{
    public BrowserViewModel ViewModel
    {
        get;
    }

    public BrowserPage()
    {
        ViewModel = App.GetService<BrowserViewModel>();
        InitializeComponent();
    }
}

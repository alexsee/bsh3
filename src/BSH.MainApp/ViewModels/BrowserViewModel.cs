using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BSH.MainApp.ViewModels;

public class BrowserViewModel : ObservableRecipient
{
    public VersionDetails CurrentVersion { get; } = null;

    public ObservableCollection<FileOrFolderItem> CurrentFolderPath { get; } = new();

    public ObservableCollection<string> Favorites { get; } = new();

    public ObservableCollection<FileOrFolderItem> Items { get; } = new();

    public ObservableCollection<VersionDetails> Versions { get; } = new();

    public BrowserViewModel()
    {

    }
}

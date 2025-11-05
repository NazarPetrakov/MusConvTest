using CommunityToolkit.Mvvm.ComponentModel;

namespace AmazonMusicParser.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome";
}

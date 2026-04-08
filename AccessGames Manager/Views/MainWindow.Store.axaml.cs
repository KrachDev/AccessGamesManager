using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace AccessGames_Manager.Views
{
    public partial class MainWindow
    {
        private async void NavStore_Click(object? sender, RoutedEventArgs e)
        {
            SetNav(NavStore, PageStore);
            await StoreViewControl.LoadStoreAsync();
        }
    }
}

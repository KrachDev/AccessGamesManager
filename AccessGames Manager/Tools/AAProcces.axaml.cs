using AccessGamesManager.Misc;
using HandyControl.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AccessGamesManager.Tools
{
    public partial class AAProcces : UserControl
    {
        public Account? EditAccount = null;
        private readonly SteamData steamData = new SteamData();

        public AAProcces()
        {
            InitializeComponent();
            if (EditAccount != null)
            {
                USerNameBox.Text = EditAccount.Username;
                PassBox.Text     = EditAccount.Password;
            }
        }

        private void GameADDBTN_Click(object? sender, RoutedEventArgs e)
        {
            GamesListBox.Items.Add(MakeItem(GameBox.Text ?? ""));
            GameBox.Text = string.Empty;
        }

        private ListBoxItem MakeItem(string name)
        {
            var item = new ListBoxItem { Content = name };
            item.DoubleTapped += (s, e) => GamesListBox.Items.Remove(item);
            return item;
        }

        // Fetches owned games via Steam Web API (paste profile/vanity URL in GameBox)
        private async void GameScanBTN_Click(object? sender, RoutedEventArgs e)
        {
            var games = await steamData.GetOwnedGameNames(GameBox.Text ?? "");
            foreach (var game in games)
                GamesListBox.Items.Add(MakeItem(game));
        }

        private async void AccCheckBTN_Click(object? sender, RoutedEventArgs e)
        {
            await steamData.LogOff();

            steamData.username           = USerNameBox.Text;
            steamData.password           = PassBox.Text;
            steamData.LaunchSteamAccount = OpenSteamCheck.IsChecked ?? false;

            await steamData.LogINAcc();

            GameBox.Text = steamData.steamID;
            Growl.Info(steamData.steamID ?? "No Steam ID retrieved.");

            await steamData.LogOff();
        }
    }
}

using AccessGamesManager.Misc;
using HandyControl.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace AccessGamesManager.Tools
{
    public partial class MiniAcc : UserControl
    {
        private readonly SteamData _data = new SteamData();
        private readonly Game AccGame;

        /// <summary>Parameterless constructor required by Avalonia XAML loader.</summary>
        public MiniAcc() : this(new Game()) { }

        public MiniAcc(Game game)
        {
            InitializeComponent();
            AccGame = game;
        }

        private void UserLabl_MouseDoubleClick(object? sender, TappedEventArgs e)
        {
            string text = UserLabl.Content?.ToString()?.Replace("Username: ", "").Trim() ?? "";
            if (!string.IsNullOrEmpty(text))
            {
                try { System.Windows.Clipboard.SetText(text); } catch { }
                Growl.Success("Copied " + text);
            }
            else Growl.Warning("No username to copy.");
        }

        private void PassLabl_MouseDoubleClick(object? sender, TappedEventArgs e)
        {
            string text = PassLabl.Content?.ToString()?.Replace("Password: ", "").Trim() ?? "";
            if (!string.IsNullOrEmpty(text))
            {
                try { System.Windows.Clipboard.SetText(text); } catch { }
                Growl.Success("Copied " + text);
            }
            else Growl.Warning("No password to copy.");
        }

        private void SteamLOGIN_Click(object? sender, RoutedEventArgs e)
        {
            _data.username = UserLabl.Content?.ToString();
            _data.password = PassLabl.Content?.ToString();
            _ = _data.LogINAcc();   // fire-and-forget intentionally
        }

        private void GFNlogin_Click(object? sender, RoutedEventArgs e)
        {
            _ = _data.GetAppIdByNameAsync(AccGame.Name ?? "");  // fire-and-forget intentionally
        }
    }
}

using HandyControl.Controls;
using AccessGamesManager.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AccessGamesManager.Misc
{
    public class LibraryLogic
    {
        static List<Account> accountsGets = new List<Account>();

        public LibraryLogic() { }

        public Dictionary<string, Avalonia.Media.Imaging.Bitmap> ImageCache = new();

        public void LibGamesList(WrapPanel wrapPanel, StackPanel Spanel, List<Account> accountsGet)
        {
            accountsGets = accountsGet;
            var gameList = GetUniqueGames().OrderBy(game => game.Name).ToList();
            wrapPanel.Children.Clear();
            foreach (var item in gameList)
                Gmalib(wrapPanel, item, Spanel);
        }

        public void Gmalib(WrapPanel wrapPanel, Game game, StackPanel Spanel)
        {
            var libGame = new LibGame
            {
                Tag    = game,
                Margin = new Avalonia.Thickness(8)
            };
            libGame.GameNameTXT.Text = game.Name;

            // Use IsCheckedChanged instead of obsolete Checked
            libGame.RBgame.IsCheckedChanged += (sender, e) =>
            {
                if (libGame.RBgame.IsChecked != true) return;

                Spanel.Children.Clear();

                var accounts = GetGameAccountsForGame(game.Name ?? "");
                foreach (var account in accounts)
                {
                    var miniAcc = new MiniAcc(game)
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Width = Spanel.Width
                    };
                    miniAcc.UserLabl.Content = account.Username;
                    miniAcc.PassLabl.Content = account.Password;
                    Spanel.Children.Add(miniAcc);
                }
            };

            wrapPanel.Children.Add(libGame);
        }

        public List<GameAccount> GetGameAccountsForGame(string gameName)
        {
            var gameAccounts = new List<GameAccount>();
            var filteredGames = GetAllGames()
                .Where(g => (g.Name ?? "").Equals(gameName, StringComparison.OrdinalIgnoreCase));

            foreach (var game in filteredGames)
                gameAccounts.Add(new GameAccount { Username = game.Username, Password = game.Password });

            return gameAccounts;
        }

        public List<Game> GetUniqueGames()
        {
            var accounts  = DataManager.LoadData();
            var seen      = new HashSet<string>();
            var unique    = new List<Game>();

            if (accounts == null || accounts.Count == 0)
            { Growl.Warning("No accounts found."); return unique; }

            foreach (var account in accounts)
            {
                if (account?.GamesList == null) continue;
                foreach (var game in account.GamesList)
                    if (game != null && seen.Add(game.Name ?? ""))
                        unique.Add(game);
            }
            return unique;
        }

        public List<Game> GetAllGames()
        {
            var accounts = DataManager.LoadData();
            var all      = new List<Game>();

            if (accounts == null || accounts.Count == 0)
            { Growl.Warning("No accounts found."); return all; }

            foreach (var account in accounts)
                if (account?.GamesList != null)
                    all.AddRange(account.GamesList);

            return all;
        }
    }
}

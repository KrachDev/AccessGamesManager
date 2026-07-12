using System;
using System.Net.Http;
using System.Threading.Tasks;
using AccessGamesManager.Misc;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Layout;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Localization = AccessGamesManager.Misc.Localization;

namespace AccessGames_Manager.Views
{
    public partial class MainWindow : Window
    {
        SteamData steamData = new SteamData();
        List<SteamUserEntry> usersList = new List<SteamUserEntry>();
        List<SteamGame> installedGames = new List<SteamGame>();

        private Button? _activeNav;
        private bool _langSyncing = false;
        private Avalonia.Threading.DispatcherTimer? _carouselTimer;

        // ── CDN cover download ────────────────────────────────────────────────────
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private static readonly string _coverCacheDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "AccessGamesManager", "coverCache");

        /// <summary>
        /// Downloads the Steam portrait cover for <paramref name="appId"/> from the CDN
        /// and returns the local file path. Returns null on failure.
        /// </summary>
        private static async Task<string?> DownloadGameCoverAsync(string appId)
        {
            try
            {
                Directory.CreateDirectory(_coverCacheDir);
                string dest = Path.Combine(_coverCacheDir, $"{appId}.jpg");
                if (File.Exists(dest)) return dest;          // already cached

                // Try portrait cover first (600×900), then wide header (460×215)
                string[] urls =
                {
                    $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/library_600x900.jpg",
                    $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/header.jpg"
                };

                foreach (var url in urls)
                {
                    try
                    {
                        byte[] bytes = await _http.GetByteArrayAsync(url);
                        if (bytes.Length > 1024)             // ignore tiny error pages
                        {
                            await File.WriteAllBytesAsync(dest, bytes);
                            return dest;
                        }
                    }
                    catch { /* try next url */ }
                }
            }
            catch { /* ignore download failures silently */ }
            return null;
        }

        public MainWindow()
        {
            InitializeComponent();
            steamData.mainWindow = this;

            var savedLang = AccountConfigManager.GetLanguage();
            Localization.SetLanguage(savedLang);

            SetNav(NavGames, PageGames);
            RefreshFirewallStatus();
            SyncSettingsToggles();
            SyncLanguageDropdowns();
            ApplyLanguage();
            LoadLauncherPaths();
            LoadGamesTab();

#if DEBUG
            NavDebug.IsVisible = true;
#endif
        }

        private void LoadLauncherPaths()
        {
            var paths = AccountConfigManager.GetLauncherPaths();
            UbiPathBox.Text = paths.UbisoftConnectPath;
            EpicPathBox.Text = paths.EpicGamesPath;
            EaPathBox.Text = paths.EAPlayPath;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            HandyControl.Controls.Growl.NotificationManager =
                new Avalonia.Controls.Notifications.WindowNotificationManager(this)
                {
                    Position = Avalonia.Controls.Notifications.NotificationPosition.BottomRight,
                    MaxItems = 4
                };

            // Check for updates in the background â€” never blocks startup
            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            var update = await AccessGamesManager.Misc.AutoUpdater.CheckAsync();
            if (update == null) return;

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dlg = new AccessGamesManager.Misc.UpdateDialog(update);
                await dlg.ShowDialog(this);
            });
        }

        // â”€â”€â”€ LANGUAGE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void SyncLanguageDropdowns()
        {
            _langSyncing = true;
            int idx = Localization.Current switch
            {
                AppLanguage.French  => 1,
                AppLanguage.Darija  => 2,
                AppLanguage.Arabic  => 3,
                _                   => 0
            };
            LanguageDropdown.SelectedIndex         = idx;
            LanguageDropdownSettings.SelectedIndex = idx;
            _langSyncing = false;
        }

        private void LanguageDropdown_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_langSyncing) return;
            ApplyLanguageFromDropdown(LanguageDropdown.SelectedIndex);
        }

        private void LanguageDropdownSettings_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_langSyncing) return;
            ApplyLanguageFromDropdown(LanguageDropdownSettings.SelectedIndex);
        }

        private void ApplyLanguageFromDropdown(int idx)
        {
            var lang = idx switch { 1 => AppLanguage.French, 2 => AppLanguage.Darija, 3 => AppLanguage.Arabic, _ => AppLanguage.English };
            Localization.SetLanguage(lang);
            AccountConfigManager.SetLanguage(lang);
            SyncLanguageDropdowns();
            ApplyLanguage();
            if (AccountsWrap.Children.Count > 0) LoadAccountCards();
            if (PageGames.IsVisible) LoadGamesTab();
        }

        private void ApplyLanguage()
        {
            var L   = Localization.Get;
            var rtl = Localization.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            ShowLoginBTN.Content = L("AddNewAccount");
            NavGames.Content    = L("NavGames");
            NavAccounts.Content = L("NavAccounts");
            NavLaunchers.Content = L("NavLaunchers");
            NavSettings.Content = L("NavSettings");

            LibraryTitleTXT.Text    = L("Library");
            RefreshGamesBtn.Content = L("RefreshGames");

            AccountsTitleTXT.Text = L("Accounts");
            RefreshAccBtn.Content = L("RefreshAccounts");

            SettingsTitleTXT.Text     = L("SettingsTitle");
            LangSectionTXT.Text       = L("LanguageSection");
            LangLabelTXT.Text         = L("LanguageLabel");
            NetworkSectionTXT.Text    = L("NetworkSection");
            FirewallTitleTXT.Text     = L("FirewallControl");
            BlockBTN.Content          = L("BlockSteam");
            UnblockBTN.Content        = L("AllowSteam");
            LaunchModeSectionTXT.Text = L("LaunchModeSection");
            ForceLaunchTitleTXT.Text  = L("ForceLaunchMode");
            ForceLaunchDescTXT.Text   = L("ForceLaunchDesc");
            LaunchModeAuto.Content    = L("LaunchAuto");
            LaunchModeOnline.Content  = L("LaunchForceOnline");
            LaunchModeOffline.Content = L("LaunchForceOffline");
            AboutSectionTXT.Text      = L("AboutSection");
            AboutDescTXT.Text         = L("AboutDesc");

            bool blocked = steamData.IsSteamNetworkBlocked();
            NetworkTXT.Text        = blocked ? L("Offline")  : L("Online");
            FirewallStatusTXT.Text = blocked ? L("OFFLINE")  : L("ONLINE");
            FirewallDescTXT.Text   = blocked ? L("FirewallDescOff") : L("FirewallDescOn");

            // ── Launchers page ──────────────────────────────────────────────
            LaunchersTitleTXT.Text              = L("LaunchersTitle");
            LaunchersFirewallSectionTXT.Text    = L("FirewallControlHeader");
            LaunchersSettingsSectionTXT.Text    = L("SettingsHeader");
            PathsConfigTitleTXT.Text            = L("PathsConfigTitle");
            UbiWarningTXT.Text                  = L("LauncherNotDetected");
            EpicWarningTXT.Text                 = L("LauncherNotDetected");
            EaWarningTXT.Text                   = L("LauncherNotDetected");
            EpicPathFoundTXT.Text               = L("LauncherFoundDefault");
            EaPathFoundTXT.Text                 = L("LauncherFoundDefault");
            BlockUbiBTN.Content                 = L("BtnBlock");
            UnblockUbiBTN.Content               = L("BtnAllow");
            BlockEpicBTN.Content                = L("BtnBlock");
            UnblockEpicBTN.Content              = L("BtnAllow");
            BlockEaBTN.Content                  = L("BtnBlock");
            UnblockEaBTN.Content                = L("BtnAllow");
            UbiBlockedBadgeTXT.Text             = L("BadgeBlocked");
            UbiAllowedBadgeTXT.Text             = L("BadgeAllowed");
            UbiFolderLabelTXT.Text              = L("UbiFolderLabel");
            EpicFolderLabelTXT.Text             = L("EpicFolderLabel");
            EaFolderLabelTXT.Text               = L("EaFolderLabel");
            UbiStep1Text.Text                   = L("UbiStep1");
            UbiStep3Text.Text                   = L("UbiStep3");

            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "2.5.2";
            AboutVersionTXT.Text = "v" + version;
            AppVersionTXT.Text = $"AccessGames Manager v{version}";

            ParentWindow.FlowDirection = rtl;
        }

        // â”€â”€â”€ NAV â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void SetNav(Button btn, Control page)
        {
            foreach (var b in new[] { NavGames, NavAccounts, NavSettings, NavLaunchers, NavDebug })
            {
                b.Classes.Remove("NavBtnActive");
                if (!b.Classes.Contains("NavBtn")) b.Classes.Add("NavBtn");
            }
            btn.Classes.Remove("NavBtn");
            if (!btn.Classes.Contains("NavBtnActive")) btn.Classes.Add("NavBtnActive");
            _activeNav = btn;

            PageGames.IsVisible    = page == PageGames;
            PageAccounts.IsVisible = page == PageAccounts;
            PageSettings.IsVisible = page == PageSettings;
            PageLaunchers.IsVisible = page == PageLaunchers;
            PageDebug.IsVisible    = page == PageDebug;
        }

        private void NavGames_Click(object? sender, RoutedEventArgs e)    => SetNav(NavGames, PageGames);
        private void NavAccounts_Click(object? sender, RoutedEventArgs e)
        {
            SetNav(NavAccounts, PageAccounts);
            if (AccountsWrap.Children.Count == 0) LoadAccountCards();
        }
        private void NavLaunchers_Click(object? sender, RoutedEventArgs e)
        {
            SetNav(NavLaunchers, PageLaunchers);
            RefreshLaunchersUI();
        }
        private void NavSettings_Click(object? sender, RoutedEventArgs e) => SetNav(NavSettings, PageSettings);
        private void NavDebug_Click(object? sender, RoutedEventArgs e) => SetNav(NavDebug, PageDebug);

        private void DebugFindGameBtn_Click(object? sender, RoutedEventArgs e)
        {
            string appId = DebugAppIdBox.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(appId))
            {
                DebugGameDirResult.Text = "Please enter an AppID.";
                return;
            }

            string? dir = steamData.GetGameDirectory(appId);
            if (!string.IsNullOrEmpty(dir))
            {
                DebugGameDirResult.Text = $"Found: {dir}";
            }
            else
            {
                DebugGameDirResult.Text = "Not found.";
            }
        }

        // â”€â”€â”€ NETWORK STATUS â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public void CheckSteamNetworkStatus(string status) { }

        public void RefreshFirewallStatus()
        {
            bool blocked = steamData.IsSteamNetworkBlocked();
            var onColor  = Color.FromRgb(68, 255, 136);
            var offColor = Color.FromRgb(255, 80, 80);
            var c = blocked ? offColor : onColor;

            NetworkDot.Fill       = new SolidColorBrush(c);
            NetworkTXT.Text       = blocked ? Localization.Get("Offline") : Localization.Get("Online");
            NetworkTXT.Foreground = new SolidColorBrush(c);

            BadgeDot.Fill                = new SolidColorBrush(c);
            FirewallStatusTXT.Text       = blocked ? Localization.Get("OFFLINE") : Localization.Get("ONLINE");
            FirewallStatusTXT.Foreground = new SolidColorBrush(c);
            BadgeBorder.Background       = blocked
                ? new SolidColorBrush(Color.FromRgb(42, 10, 10))
                : new SolidColorBrush(Color.FromRgb(10, 42, 10));
            FirewallDescTXT.Text = blocked ? Localization.Get("FirewallDescOff") : Localization.Get("FirewallDescOn");
            SetStatus(blocked ? Localization.Get("StatusBlocked") : Localization.Get("StatusOpen"));
        }

        private void SetStatus(string msg) => StatusTXT.Text = msg;

        // â”€â”€â”€ GAMES TAB â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void LoadGamesTab()
        {
            SetStatus(Localization.Get("StatusLoadingGames"));
            GamesWraper.Children.Clear();
            installedGames = steamData.GetInstalledGames().Where(g => g.isInstalled).OrderBy(g => g.Name).ToList();

            if (usersList.Count == 0)
                usersList = steamData.GetSteamUsers();

            var personalIds = usersList
                .Where(u => AccountConfigManager.IsPersonal(u.AccountID ?? ""))
                .Select(u => u.AccountID)
                .ToHashSet();

            foreach (var game in installedGames)
            {
                string ownerId = steamData.GetGameOwner(game.AppID);
                if (!string.IsNullOrEmpty(ownerId) && personalIds.Contains(ownerId)) continue;
                var card = BuildGameCard(game);
                if (card != null) GamesWraper.Children.Add(card);
            }

            int count = GamesWraper.Children.Count;
            GamesCountLBL.Text = $"{count} {Localization.Get("GamesCount")}";
            SetStatus(Localization.GetF("StatusLoadedGames", count));
        }

        // â”€â”€â”€ BUILD GAME CARD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private Control? BuildGameCard(SteamGame game)
        {
            try
            {
                var accessAccounts = usersList
                    .Where(u => !AccountConfigManager.IsPersonal(u.AccountID ?? ""))
                    .ToList();

                string? savedOverrideId  = AccountConfigManager.GetGameOwnerOverride(game.AppID);
                string  aclOwnerId       = steamData.GetGameOwner(game.AppID);
                string  activeOwnerId    = savedOverrideId ?? aclOwnerId;

                SteamUserEntry? activeOwner = accessAccounts.FirstOrDefault(u => u.AccountID == activeOwnerId)
                                           ?? accessAccounts.FirstOrDefault();

                bool multipleOwners = accessAccounts.Count >= 2;

                string coverPath  = steamData.GetGameImages(game.AppID, 5);
                string headerPath = steamData.GetGameImages(game.AppID, 1);
                string? imagePath = File.Exists(coverPath) ? coverPath
                                  : File.Exists(headerPath) ? headerPath : null;

                var card = new Border
                {
                    Width = 150, Margin = new Thickness(8),
                    CornerRadius    = new CornerRadius(10),
                    Background      = new SolidColorBrush(Color.FromRgb(18, 18, 28)),
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                    BorderThickness = new Thickness(1),
                    ClipToBounds    = true,
                    Cursor          = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                    [ToolTip.TipProperty] = $"AppID: {game.AppID}\n{Localization.Get("GameTooltipOwner")}: {activeOwner?.PersonaName ?? Localization.Get("UnknownOwner")}\n{Localization.Get("GameTooltipLaunch")}"
                };

                var coverBorder = new Border
                {
                    Height = 200,
                    CornerRadius = new CornerRadius(10, 10, 0, 0),
                    ClipToBounds = true,
                    FlowDirection = FlowDirection.LeftToRight
                };
                if (imagePath != null)
                {
                    try
                    {
                        var bmp = new Bitmap(imagePath);
                        coverBorder.Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
                    }
                    catch { coverBorder.Background = BuildPlaceholderBrush(); }
                }
                else
                {
                    // Show placeholder immediately, then load from CDN in the background
                    coverBorder.Background = BuildPlaceholderBrush();
                    coverBorder.Child = new TextBlock
                    {
                        Text = game.Name, Foreground = Brushes.White, FontSize = 11,
                        FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center,
                        Margin = new Thickness(8)
                    };

                    // Fire-and-forget CDN download — updates the card when done
                    _ = Task.Run(async () =>
                    {
                        string? cdnPath = await DownloadGameCoverAsync(game.AppID);
                        if (cdnPath == null) return;
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            try
                            {
                                var bmp = new Bitmap(cdnPath);
                                coverBorder.Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
                                coverBorder.Child = null;   // remove the text placeholder
                            }
                            catch { /* bitmap load failed, keep placeholder */ }
                        });
                    });
                }

                var info = new StackPanel
                {
                    Background = new SolidColorBrush(Color.FromRgb(18, 18, 28))
                };

                info.Children.Add(new TextBlock
                {
                    Text = game.Name, Foreground = Brushes.White, FontSize = 11,
                    FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(8, 8, 8, 4), MaxWidth = 134
                });

                // ── Action row: [▶ Play] [⋯] ────────────────────────────────────
                var playBtn = new Button
                {
                    Content             = Localization.Get("PlayBtn"),
                    FontSize            = 11,
                    FontWeight          = FontWeight.SemiBold,
                    Foreground          = Brushes.White,
                    Background          = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                    CornerRadius        = new CornerRadius(6),
                    Padding             = new Thickness(0, 6, 0, 6),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Cursor              = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                playBtn.Click += async (s, e) =>
                {
                    e.Handled = true;
                    string? overrideId = AccountConfigManager.GetGameOwnerOverride(game.AppID);
                    string  resolvedId = overrideId ?? steamData.GetGameOwner(game.AppID);
                    SteamUserEntry? owner = accessAccounts.FirstOrDefault(u => u.AccountID == resolvedId)
                                         ?? accessAccounts.FirstOrDefault();
                    await OnGameCardClick(game, owner, resolvedId);
                };

                // ── Options flyout: account selector + actions ─────────────────
                // Default account label
                string defaultName = steamData.GetGameOwner(game.AppID) is string defId && !string.IsNullOrEmpty(defId)
                    ? (accessAccounts.FirstOrDefault(u => u.AccountID == defId)?.PersonaName ?? defId)
                    : Localization.Get("UnknownOwner");

                var flyoutDefaultLbl = new TextBlock
                {
                    Text       = Localization.GetF("DefaultAccount", defaultName),
                    Foreground = new SolidColorBrush(Color.FromRgb(90, 90, 130)),
                    FontSize   = 9,
                    Margin     = new Thickness(10, 6, 10, 4),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                // Account selector label
                var flyoutAccLabel = new TextBlock
                {
                    Text       = Localization.Get("ChangeAccount"),
                    Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 180)),
                    FontSize   = 9,
                    FontWeight = FontWeight.SemiBold,
                    Margin     = new Thickness(10, 4, 10, 2)
                };

                // Account picker ComboBox
                var flyoutAccPicker = new ComboBox
                {
                    FontSize            = 10,
                    Margin              = new Thickness(6, 0, 6, 6),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    [ToolTip.TipProperty] = Localization.Get("ChangeAccount")
                };
                int flyoutSelIdx = 0;
                for (int i = 0; i < accessAccounts.Count; i++)
                {
                    var acc = accessAccounts[i];
                    flyoutAccPicker.Items.Add(acc.PersonaName ?? acc.AccountName ?? acc.AccountID ?? "?");
                    if (acc.AccountID == activeOwnerId) flyoutSelIdx = i;
                }
                flyoutAccPicker.SelectedIndex = flyoutSelIdx;
                flyoutAccPicker.SelectionChanged += (s, e) =>
                {
                    int idx = flyoutAccPicker.SelectedIndex;
                    if (idx < 0 || idx >= accessAccounts.Count) return;
                    var chosen = accessAccounts[idx];
                    AccountConfigManager.SetGameOwnerOverride(game.AppID, chosen.AccountID ?? "");
                    card[ToolTip.TipProperty] = $"AppID: {game.AppID}\n{Localization.Get("GameTooltipOwner")}: {chosen.PersonaName ?? Localization.Get("UnknownOwner")}\n{Localization.Get("GameTooltipLaunch")}";
                };
                flyoutAccPicker.PointerPressed += (s, e) => e.Handled = true;

                // Separator
                var flyoutSep = new Border
                {
                    Height     = 1,
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 65)),
                    Margin     = new Thickness(6, 4, 6, 4)
                };

                // Reset Playtime button
                var flyoutResetTime = new Button
                {
                    Content    = Localization.Get("ResetPlaytime"),
                    FontSize   = 11,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                    HorizontalAlignment        = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding    = new Thickness(10, 7, 10, 7),
                    CornerRadius = new CornerRadius(6)
                };

                // Reset Achievements button
                var flyoutResetAchv = new Button
                {
                    Content    = Localization.Get("ResetAchievements"),
                    FontSize   = 11,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                    HorizontalAlignment        = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding    = new Thickness(10, 7, 10, 7),
                    CornerRadius = new CornerRadius(6)
                };

                var flyoutPanel = new StackPanel
                {
                    Spacing  = 2,
                    Margin   = new Thickness(4),
                    MinWidth = 175
                };
                if (accessAccounts.Count >= 1) flyoutPanel.Children.Add(flyoutDefaultLbl);
                if (accessAccounts.Count >= 2)
                {
                    flyoutPanel.Children.Add(flyoutAccLabel);
                    flyoutPanel.Children.Add(flyoutAccPicker);
                }
                flyoutPanel.Children.Add(flyoutSep);
                flyoutPanel.Children.Add(flyoutResetTime);
                flyoutPanel.Children.Add(flyoutResetAchv);

                var optionsFlyout = new Avalonia.Controls.Flyout
                {
                    Content     = flyoutPanel,
                    Placement   = Avalonia.Controls.PlacementMode.Top
                };

                flyoutResetTime.Click += (s, e) =>
                {
                    e.Handled = true;
                    optionsFlyout.Hide();
                    steamData.ResetGameTime(game.AppID);
                };
                flyoutResetAchv.Click += (s, e) =>
                {
                    e.Handled = true;
                    optionsFlyout.Hide();
                    steamData.ResetAchievementsWithSAM(game.AppID);
                };

                // Options (⋯) button
                var optionsBtn = new Button
                {
                    Content      = "⋯",
                    FontSize     = 14,
                    Foreground   = new SolidColorBrush(Color.FromRgb(140, 140, 180)),
                    Background   = new SolidColorBrush(Color.FromRgb(28, 28, 42)),
                    BorderBrush  = new SolidColorBrush(Color.FromRgb(50, 50, 75)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Width        = 30,
                    Padding      = new Thickness(0),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment   = VerticalAlignment.Center,
                    Cursor       = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                    [ToolTip.TipProperty] = "Options"
                };
                Avalonia.Controls.Primitives.FlyoutBase.SetAttachedFlyout(optionsBtn, optionsFlyout);
                optionsBtn.Click += (s, e) =>
                {
                    e.Handled = true;
                    Avalonia.Controls.Primitives.FlyoutBase.ShowAttachedFlyout(optionsBtn);
                };

                // Row container
                var actionRow = new Grid { Margin = new Thickness(8, 0, 8, 8) };
                actionRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                actionRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

                Grid.SetColumn(playBtn,    0);
                Grid.SetColumn(optionsBtn, 1);
                actionRow.Children.Add(playBtn);
                actionRow.Children.Add(optionsBtn);

                // ── Hover effect on card border ──────────────────────────────────
                card.PointerEntered += (s, e) =>
                    card.BorderBrush = new SolidColorBrush(Color.FromRgb(108, 71, 255));
                card.PointerExited  += (s, e) =>
                    card.BorderBrush = new SolidColorBrush(Color.FromRgb(35, 35, 55));

                // Clicking the cover also plays
                coverBorder.PointerPressed += async (s, e) =>
                {
                    string? overrideId = AccountConfigManager.GetGameOwnerOverride(game.AppID);
                    string  resolvedId = overrideId ?? steamData.GetGameOwner(game.AppID);
                    SteamUserEntry? owner = accessAccounts.FirstOrDefault(u => u.AccountID == resolvedId)
                                         ?? accessAccounts.FirstOrDefault();
                    await OnGameCardClick(game, owner, resolvedId);
                };

                var stack = new StackPanel();
                stack.Children.Add(coverBorder);
                stack.Children.Add(info);
                stack.Children.Add(actionRow);
                card.Child = stack;
                return card;
            }
            catch { return null; }
        }

        // â”€â”€â”€ GAME CARD CLICK â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task OnGameCardClick(SteamGame game, SteamUserEntry? registeredOwner, string? ownerId)
        {
            SteamUserEntry? account = registeredOwner;
            if (account == null)
            {
                account = usersList.FirstOrDefault(u => !AccountConfigManager.IsPersonal(u.AccountID ?? ""));
                if (account == null)
                {
                    HandyControl.Controls.Growl.Warning(Localization.GetF("NoAccountToLaunch", game.Name));
                    return;
                }
            }

            string? currentLogin = steamData.GetAutoLoginUser();
            bool alreadySignedIn = !string.IsNullOrEmpty(currentLogin)
                && !string.IsNullOrEmpty(account.AccountName)
                && string.Equals(currentLogin, account.AccountName, StringComparison.OrdinalIgnoreCase);

            if (alreadySignedIn)
            {
                string title   = Localization.Get("AlreadySignedInTitle");
                string message = Localization.GetF("AlreadySignedInMsg", account.PersonaName ?? account.AccountName ?? "?");
                string reboot  = Localization.Get("AlreadySignedInReboot");
                string launch  = Localization.Get("AlreadySignedInLaunch");
                string cancel  = Localization.Get("AlreadySignedInCancel");

                var dlg = new AccessGamesManager.Misc.AlreadySignedInDialog(title, message, reboot, launch, cancel);
                await dlg.ShowDialog(this);

                switch (dlg.Result)
                {
                    case AccessGamesManager.Misc.AlreadySignedInResult.Reboot:
                        break;
                    case AccessGamesManager.Misc.AlreadySignedInResult.LaunchOnly:
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            { FileName = $"steam://rungameid/{game.AppID}", UseShellExecute = true });
                        SetStatus(Localization.GetF("StatusLaunching", game.Name, account.PersonaName ?? "", Localization.Get("Online")));
                        return;
                    default:
                        return;
                }
            }

            bool offline   = AccountConfigManager.ShouldLaunchOffline(account.AccountID ?? "");
            string modeStr = offline ? Localization.Get("Offline") : Localization.Get("Online");
            SetStatus(Localization.GetF("StatusLaunching", game.Name, account.PersonaName ?? "", modeStr));
            steamData.LaunchGame(account, game.AppID, offline);
        }

        private static Brush BuildPlaceholderBrush() =>
            new LinearGradientBrush
            {
                StartPoint = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Relative),
                EndPoint   = new Avalonia.RelativePoint(1, 1, Avalonia.RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.FromRgb(25, 25, 45), 0),
                    new GradientStop(Color.FromRgb(40, 30, 70), 1)
                }
            };

        private void GamesSearchBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            string query = (GamesSearchBox.Text ?? "").Trim().ToLower();
            GamesWraper.Children.Clear();

            var personalIds = usersList
                .Where(u => AccountConfigManager.IsPersonal(u.AccountID ?? ""))
                .Select(u => u.AccountID)
                .ToHashSet();

            var filtered = (string.IsNullOrEmpty(query)
                ? installedGames
                : installedGames.Where(g => g.Name.ToLower().Contains(query)).ToList())
                .Where(g =>
                {
                    string oid = steamData.GetGameOwner(g.AppID);
                    return string.IsNullOrEmpty(oid) || !personalIds.Contains(oid);
                });

            foreach (var game in filtered)
            {
                var card = BuildGameCard(game);
                if (card != null) GamesWraper.Children.Add(card);
            }
            GamesCountLBL.Text = $"{GamesWraper.Children.Count} {Localization.Get("GamesCount")}";
        }

        private void RefreshGamesBtn_Click(object? sender, RoutedEventArgs e) => LoadGamesTab();

        // â”€â”€â”€ ACCOUNTS TAB â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void LoadAccountCards()
        {
            SetStatus(Localization.Get("StatusLoadingAccs"));
            usersList = steamData.GetSteamUsers();
            AccountsWrap.Children.Clear();
            foreach (var user in usersList)
                AccountsWrap.Children.Add(BuildAccountCard(user));
            int count = usersList.Count;
            AccountsCountLBL.Text = $"{count} {Localization.Get("AccountsCount")}";
            SetStatus(Localization.GetF("StatusLoadedAccs", count));
        }

        private Control BuildAccountCard(SteamUserEntry user)
        {
            var role = AccountConfigManager.GetRole(user.AccountID ?? "");

            var avatarBorder = new Border
            {
                Width = 56, Height = 56, CornerRadius = new CornerRadius(28),
                ClipToBounds = true, Margin = new Thickness(0, 0, 0, 8)
            };
            if (File.Exists(user.AvatarImage))
            {
                try { avatarBorder.Background = new ImageBrush(new Bitmap(user.AvatarImage!)) { Stretch = Stretch.UniformToFill }; }
                catch { avatarBorder.Background = BuildPlaceholderBrush(); }
            }
            else
            {
                avatarBorder.Background = new SolidColorBrush(Color.FromRgb(108, 71, 255));
                string initials = user.PersonaName?.Length > 0 ? user.PersonaName[0].ToString().ToUpper() : "?";
                avatarBorder.Child = new TextBlock
                {
                    Text = initials, Foreground = Brushes.White, FontSize = 22, FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
                };
            }

            var (badgeKey, badgeColor) = role == AccountRole.Personal
                ? ("RolePersonal", Color.FromRgb(255, 200, 40))
                : ("RoleAccess",   Color.FromRgb(108, 71, 255));

            var roleBadge = new Border
            {
                Background      = new SolidColorBrush(Color.FromArgb(40, badgeColor.R, badgeColor.G, badgeColor.B)),
                BorderBrush     = new SolidColorBrush(Color.FromArgb(120, badgeColor.R, badgeColor.G, badgeColor.B)),
                BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 3, 6, 3), Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = Localization.Get(badgeKey),
                    Foreground = new SolidColorBrush(badgeColor),
                    FontSize = 10, FontWeight = FontWeight.SemiBold
                }
            };

            var rolePicker = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 8), FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            rolePicker.Items.Add(AccountRole.Access.ToString());
            rolePicker.Items.Add(AccountRole.Personal.ToString());
            rolePicker.SelectedItem = role.ToString();
            rolePicker.SelectionChanged += (s, e) =>
            {
                if (rolePicker.SelectedItem is string sel && Enum.TryParse<AccountRole>(sel, out var newRole))
                {
                    AccountConfigManager.SetRole(user.AccountID ?? "", newRole);
                    LoadAccountCards();
                    if (PageGames.IsVisible) LoadGamesTab();
                }
            };

            string btnLabel = AccountConfigManager.IsNormalLogin(user.AccountID ?? "")
                ? Localization.Get("SwitchBtnOnline")
                : Localization.Get("SwitchBtnOffline");

            var switchBtn = new Button
            {
                Content = btnLabel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                [ToolTip.TipProperty] = $"{Localization.Get("SwitchTooltip")} {user.AccountName}"
            };
            switchBtn.Click += (s, e) =>
            {
                SetStatus(Localization.GetF("StatusSwitching", user.PersonaName ?? ""));
                steamData.SwitchAccount(user);
                RefreshFirewallStatus();
            };

            var content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(12) };
            content.Children.Add(avatarBorder);
            content.Children.Add(roleBadge);
            content.Children.Add(new TextBlock
            {
                Text = user.PersonaName, Foreground = Brushes.White, FontWeight = FontWeight.Bold,
                FontSize = 13, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap,
                MaxWidth = 150, Margin = new Thickness(0, 0, 0, 2)
            });
            content.Children.Add(new TextBlock
            {
                Text = user.AccountName, Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 170)),
                FontSize = 11, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 8)
            });
            content.Children.Add(rolePicker);
            content.Children.Add(switchBtn);

            var borderAccent = role == AccountRole.Personal
                ? Color.FromRgb(255, 200, 40)
                : Color.FromRgb(35, 35, 55);

            var card = new Border
            {
                Width = 190, CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Color.FromRgb(18, 18, 28)),
                BorderBrush = new SolidColorBrush(borderAccent),
                BorderThickness = new Thickness(1), Margin = new Thickness(8),
                Child = content
            };
            card.PointerEntered += (s, e) => card.BorderBrush = new SolidColorBrush(Color.FromRgb(108, 71, 255));
            card.PointerExited  += (s, e) => card.BorderBrush = new SolidColorBrush(borderAccent);
            return card;
        }

        // â”€â”€â”€ FIX INFINITE LOOP BUTTON â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async void FixLoopBTN_Click(object? sender, RoutedEventArgs e)
        {
            //FixLoopBTN.IsEnabled = false;
            SetStatus("Fixing Steam infinite loading loopâ€¦");
            HandyControl.Controls.Growl.Info("Running infinite-loop fix. Steam will restart automatically.");

            await steamData.FixInfiniteLoadingLoop(waitCallback: async () =>
            {
                var dlg = new AccessGamesManager.Misc.SteamCountdownDialog(totalSeconds: 20);
                await dlg.ShowDialog(this);
            });

            RefreshFirewallStatus();
           // FixLoopBTN.IsEnabled = true;
        }

        // â”€â”€â”€ ADD ACCOUNT BUTTON â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void ShowLoginBTN_Click(object? sender, RoutedEventArgs e)
        {
            if (steamData.IsSteamNetworkBlocked())
            {
                steamData.UnblockSteamNetwork();
                RefreshFirewallStatus();
                HandyControl.Controls.Growl.Info("Steam internet access restored for login.");
            }

            steamData.ForceLoginPage();
            SetStatus(Localization.Get("StatusSteamRestart"));
        }

        private void RefreshAccBtn_Click(object? sender, RoutedEventArgs e) => LoadAccountCards();

        private void AnalyticsBTN_Click(object? sender, RoutedEventArgs e)
        {
            string report = AccessGames_Manager.Views.AnalyticsDisplay.GetAnalyticsReport();
            Console.WriteLine(report);

            // Copy to clipboard and notify
            SetStatus("📊 Analytics displayed in console output");
            HandyControl.Controls.Growl.Info("Analytics report copied to console. Check the output window.");
        }


        // ─── SETTINGS ────────────────────────────────────────────────────────────────
        private void BlockBTN_Click(object? sender, RoutedEventArgs e)
        {
            steamData.BlockSteamNetwork();
            RefreshFirewallStatus();
            HandyControl.Controls.Growl.Success(Localization.Get("GrowlBlocked"));
        }

        private void UnblockBTN_Click(object? sender, RoutedEventArgs e)
        {
            steamData.UnblockSteamNetwork();
            RefreshFirewallStatus();
            HandyControl.Controls.Growl.Success(Localization.Get("GrowlUnblocked"));
        }

        private bool _syncing = false;

        private void SyncSettingsToggles()
        {
            _syncing = true;
            var mode = AccountConfigManager.GetLaunchMode();
            LaunchModeAuto.IsChecked    = mode == ForceLaunchMode.Auto;
            LaunchModeOnline.IsChecked  = mode == ForceLaunchMode.ForceOnline;
            LaunchModeOffline.IsChecked = mode == ForceLaunchMode.ForceOffline;
            _syncing = false;
        }

        private void LaunchModeAuto_Checked(object? sender, RoutedEventArgs e)
        {
            if (_syncing || LaunchModeAuto.IsChecked != true) return;
            AccountConfigManager.SetLaunchMode(ForceLaunchMode.Auto);
            SetStatus(Localization.Get("StatusLaunchModeAuto"));
        }

        private void LaunchModeOnline_Checked(object? sender, RoutedEventArgs e)
        {
            if (_syncing || LaunchModeOnline.IsChecked != true) return;
            AccountConfigManager.SetLaunchMode(ForceLaunchMode.ForceOnline);
            SetStatus(Localization.Get("StatusLaunchOnline"));
        }

        private void LaunchModeOffline_Checked(object? sender, RoutedEventArgs e)
        {
            if (_syncing || LaunchModeOffline.IsChecked != true) return;
            AccountConfigManager.SetLaunchMode(ForceLaunchMode.ForceOffline);
            SetStatus(Localization.Get("StatusLaunchOffline"));
        }

        // ─── LAUNCHERS PAGE ──────────────────────────────────────────────────────────
        private void RefreshLaunchersUI()
        {
            var paths = AccountConfigManager.GetLauncherPaths();

            // Ubisoft
            bool ubiFound = LauncherNetworkManager.CheckUbisoftFound(paths.UbisoftConnectPath);
            UbiWarningTXT.IsVisible = !ubiFound;
            BlockUbiBTN.IsEnabled = ubiFound;
            UnblockUbiBTN.IsEnabled = ubiFound;

            // Show block status badge
            bool ubiBlocked = LauncherNetworkManager.IsUbisoftBlocked();
            UbiBlockedBadge.IsVisible = ubiFound && ubiBlocked;
            UbiAllowedBadge.IsVisible = ubiFound && !ubiBlocked;

            // Epic
            bool epicFound = LauncherNetworkManager.CheckEpicFound(paths.EpicGamesPath);
            EpicWarningTXT.IsVisible = !epicFound;
            EpicPathFoundTXT.IsVisible = epicFound;
            BlockEpicBTN.IsEnabled = epicFound;
            UnblockEpicBTN.IsEnabled = epicFound;

            // EA
            bool eaFound = LauncherNetworkManager.CheckEAFound(paths.EAPlayPath);
            EaWarningTXT.IsVisible = !eaFound;
            EaPathFoundTXT.IsVisible = eaFound;
            BlockEaBTN.IsEnabled = eaFound;
            UnblockEaBTN.IsEnabled = eaFound;
        }

        private async void BlockUbiBTN_Click(object? sender, RoutedEventArgs e)
        {
            var paths = AccountConfigManager.GetLauncherPaths();
            
            // Disable buttons to prevent duplicate clicks
            BlockUbiBTN.IsEnabled = false;
            UnblockUbiBTN.IsEnabled = false;
            
            // Reset and Show progress panel
            UbiProgressPanel.IsVisible = true;
            UbiProgressBar.Value = 0;
            UbiProgressStatusTXT.Text = "Preparing to block...";
            
            // Set text for step 2 & 4
            UbiStep2Text.Text = "Step 2: Add outbound/inbound firewall block rules for Ubisoft";
            UbiStep4Text.Text = "Step 4: Block completed successfully!";
            
            // Reset steps to pending state
            UpdateUbiStep(1, "Step 1", "○", "#FF555577");
            UpdateUbiStep(2, "Step 2", "○", "#FF555577");
            UpdateUbiStep(3, "Step 3", "○", "#FF555577");
            UpdateUbiStep(4, "Step 4", "○", "#FF555577");
            
            try
            {
                // ─── STEP 1 ───
                UpdateUbiStep(1, "Step 1", "▶", "#FF6C47FF");
                UbiProgressStatusTXT.Text = "Step 1: Disabling active network adapters & enabling Airplane Mode...";
                UbiProgressBar.Value = 10;
                
                // Get active adapters and radios
                var disabledAdapters = await Task.Run(() => LauncherNetworkManager.GetEnabledNetworkAdapters());
                var onRadios = await LauncherNetworkManager.GetOnRadiosAsync();
                
                // Disable active adapters
                await Task.Run(() => LauncherNetworkManager.DisableNetworkAdapters(disabledAdapters));
                
                // Turn off all radios
                await LauncherNetworkManager.TurnOffAllRadiosAsync();
                
                UpdateUbiStep(1, "Step 1", "✓", "#FF44FF88");
                UbiProgressBar.Value = 35;
                await Task.Delay(500); // Small pause for user to follow along
                
                // ─── STEP 2 ───
                UpdateUbiStep(2, "Step 2", "▶", "#FF6C47FF");
                UbiProgressStatusTXT.Text = "Step 2: Adding outbound/inbound firewall block rules...";
                UbiProgressBar.Value = 45;
                
                // Apply block rule
                await Task.Run(() => LauncherNetworkManager.BlockUbisoft(paths.UbisoftConnectPath));
                
                UpdateUbiStep(2, "Step 2", "✓", "#FF44FF88");
                UbiProgressBar.Value = 70;
                await Task.Delay(500);
                
                // ─── STEP 3 ───
                UpdateUbiStep(3, "Step 3", "▶", "#FF6C47FF");
                UbiProgressStatusTXT.Text = "Step 3: Restoring network adapters & Airplane Mode...";
                UbiProgressBar.Value = 75;
                
                // Enable adapters
                await Task.Run(() => LauncherNetworkManager.EnableNetworkAdapters(disabledAdapters));
                
                // Restore radios
                await LauncherNetworkManager.SetRadiosStateAsync(onRadios, true);
                
                UpdateUbiStep(3, "Step 3", "✓", "#FF44FF88");
                UbiProgressBar.Value = 90;
                await Task.Delay(500);
                
                // ─── STEP 4 ───
                UpdateUbiStep(4, "Step 4", "✓", "#FF44FF88");
                UbiProgressStatusTXT.Text = "Success! Ubisoft Connect has been blocked.";
                UbiProgressBar.Value = 100;
                
                HandyControl.Controls.Growl.Success("Blocked Ubisoft Connect");
                
                await Task.Delay(2000); // Show final state for 2 seconds
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($"Error blocking Ubisoft: {ex.Message}");
            }
            finally
            {
                UbiProgressPanel.IsVisible = false;
                RefreshLaunchersUI();
            }
        }

        private async void UnblockUbiBTN_Click(object? sender, RoutedEventArgs e)
        {
            // Disable buttons to prevent duplicate clicks
            BlockUbiBTN.IsEnabled = false;
            UnblockUbiBTN.IsEnabled = false;
            
            // Reset and Show progress panel
            UbiProgressPanel.IsVisible = true;
            UbiProgressBar.Value = 0;
            UbiProgressStatusTXT.Text = "Preparing to allow...";
            
            // Set text for step 2 & 4
            UbiStep2Text.Text = "Step 2: Remove outbound/inbound firewall block rules for Ubisoft";
            UbiStep4Text.Text = "Step 4: Unblock completed successfully!";
            
            // Reset steps to pending state
            UpdateUbiStep(1, "Step 1", "○", "#FF555577");
            UpdateUbiStep(2, "Step 2", "○", "#FF555577");
            UpdateUbiStep(3, "Step 3", "○", "#FF555577");
            UpdateUbiStep(4, "Step 4", "○", "#FF555577");
            
            try
            {
                // ─── STEP 1 ───
                UpdateUbiStep(1, "Step 1", "▶", "#FF6C47FF");
                UbiProgressStatusTXT.Text = "Step 1: Disabling active network adapters & enabling Airplane Mode...";
                UbiProgressBar.Value = 10;
                
                // Get active adapters and radios
                var disabledAdapters = await Task.Run(() => LauncherNetworkManager.GetEnabledNetworkAdapters());
                var onRadios = await LauncherNetworkManager.GetOnRadiosAsync();
                
                // Disable active adapters
                await Task.Run(() => LauncherNetworkManager.DisableNetworkAdapters(disabledAdapters));
                
                // Turn off all radios
                await LauncherNetworkManager.TurnOffAllRadiosAsync();
                
                UpdateUbiStep(1, "Step 1", "✓", "#FF44FF88");
                UbiProgressBar.Value = 35;
                await Task.Delay(500); // Small pause for user to follow along
                
                // ─── STEP 2 ───
                UpdateUbiStep(2, "Step 2", "▶", "#FF6C47FF");
                UbiProgressStatusTXT.Text = "Step 2: Removing firewall block rules...";
                UbiProgressBar.Value = 45;
                
                // Remove block rule
                await Task.Run(() => LauncherNetworkManager.UnblockRule(LauncherNetworkManager.RuleUbisoft));
                
                UpdateUbiStep(2, "Step 2", "✓", "#FF44FF88");
                UbiProgressBar.Value = 70;
                await Task.Delay(500);
                
                // ─── STEP 3 ───
                UpdateUbiStep(3, "Step 3", "▶", "#FF6C47FF");
                UbiProgressStatusTXT.Text = "Step 3: Restoring network adapters & Airplane Mode...";
                UbiProgressBar.Value = 75;
                
                // Enable adapters
                await Task.Run(() => LauncherNetworkManager.EnableNetworkAdapters(disabledAdapters));
                
                // Restore radios
                await LauncherNetworkManager.SetRadiosStateAsync(onRadios, true);
                
                UpdateUbiStep(3, "Step 3", "✓", "#FF44FF88");
                UbiProgressBar.Value = 90;
                await Task.Delay(500);
                
                // ─── STEP 4 ───
                UpdateUbiStep(4, "Step 4", "✓", "#FF44FF88");
                UbiProgressStatusTXT.Text = "Success! Ubisoft Connect has been allowed.";
                UbiProgressBar.Value = 100;
                
                HandyControl.Controls.Growl.Success("Allowed Ubisoft Connect");
                
                await Task.Delay(2000); // Show final state for 2 seconds
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($"Error unblocking Ubisoft: {ex.Message}");
            }
            finally
            {
                UbiProgressPanel.IsVisible = false;
                RefreshLaunchersUI();
            }
        }

        private void UpdateUbiStep(int stepNumber, string status, string icon, string colorHex)
        {
            var brush = Avalonia.Media.Brush.Parse(colorHex);
            if (stepNumber == 1)
            {
                UbiStep1Text.Foreground = brush;
                UbiStep1Icon.Text = icon;
                UbiStep1Icon.Foreground = brush;
            }
            else if (stepNumber == 2)
            {
                UbiStep2Text.Foreground = brush;
                UbiStep2Icon.Text = icon;
                UbiStep2Icon.Foreground = brush;
            }
            else if (stepNumber == 3)
            {
                UbiStep3Text.Foreground = brush;
                UbiStep3Icon.Text = icon;
                UbiStep3Icon.Foreground = brush;
            }
            else if (stepNumber == 4)
            {
                UbiStep4Text.Foreground = brush;
                UbiStep4Icon.Text = icon;
                UbiStep4Icon.Foreground = brush;
            }
        }

        private void BlockEpicBTN_Click(object? sender, RoutedEventArgs e)
        {
            var paths = AccountConfigManager.GetLauncherPaths();
            LauncherNetworkManager.BlockEpic(paths.EpicGamesPath);
            HandyControl.Controls.Growl.Success("Blocked Epic Games");
            RefreshLaunchersUI();
        }

        private void UnblockEpicBTN_Click(object? sender, RoutedEventArgs e)
        {
            LauncherNetworkManager.UnblockRule(LauncherNetworkManager.RuleEpic);
            HandyControl.Controls.Growl.Success("Allowed Epic Games");
            RefreshLaunchersUI();
        }

        private void BlockEaBTN_Click(object? sender, RoutedEventArgs e)
        {
            var paths = AccountConfigManager.GetLauncherPaths();
            LauncherNetworkManager.BlockEA(paths.EAPlayPath);
            HandyControl.Controls.Growl.Success("Blocked EA Desktop");
            RefreshLaunchersUI();
        }

        private void UnblockEaBTN_Click(object? sender, RoutedEventArgs e)
        {
            LauncherNetworkManager.UnblockRule(LauncherNetworkManager.RuleEA);
            HandyControl.Controls.Growl.Success("Allowed EA Desktop");
            RefreshLaunchersUI();
        }

        private void SaveLauncherPathsBTN_Click(object? sender, RoutedEventArgs e)
        {
            var paths = new LauncherPaths
            {
                UbisoftConnectPath = UbiPathBox.Text ?? "",
                EpicGamesPath = EpicPathBox.Text ?? "",
                EAPlayPath = EaPathBox.Text ?? ""
            };
            AccountConfigManager.SaveLauncherPaths(paths);
            HandyControl.Controls.Growl.Success("Saved Launcher Paths");
            RefreshLaunchersUI();
        }
    }
}


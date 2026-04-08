using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Admin panel — password-gated window for managing store offers and pushing to GitHub.
    /// Entry point: AdminPanelDialog.TryOpenAsync(ownerWindow).
    /// </summary>
    public class AdminPanelDialog : Window
    {
        private readonly StackPanel   _offerList;
        private readonly TextBlock    _statusTxt;
        private readonly TextBlock    _syncStatusTxt;
        private readonly Button       _pushBtn;
        private WindowNotificationManager? _notif;

        public AdminPanelDialog()
        {
            Title                 = "🔐  Admin Panel — Store Manager";
            Width                 = 860;
            Height                = 680;
            MinWidth              = 680;
            MinHeight             = 500;
            Background            = new SolidColorBrush(Color.FromRgb(9, 9, 15));
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize             = true;

            // ── Header ───────────────────────────────────────────────────────
            var header = new Border
            {
                Background      = new SolidColorBrush(Color.FromRgb(13, 13, 20)),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding         = new Thickness(20, 14)
            };
            var headerRow = new Grid();
            headerRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            headerRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var titleBlock = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            titleBlock.Children.Add(new Border
            {
                Width = 30, Height = 30, CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                Child = new TextBlock
                {
                    Text = "A", Foreground = Brushes.White,
                    FontSize = 13, FontWeight = FontWeight.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
            titleBlock.Children.Add(new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "Admin Panel", Foreground = Brushes.White, FontSize = 15, FontWeight = FontWeight.Bold },
                    new TextBlock { Text = "Store Manager", Foreground = new SolidColorBrush(Color.FromRgb(108, 71, 255)), FontSize = 11 }
                }
            });
            Grid.SetColumn(titleBlock, 0);

            var addBtn = new Button
            {
                Content             = "＋  Add Offer",
                Margin              = new Thickness(0, 0, 10, 0),
                Classes             = { "SecondaryBtn" },
                VerticalAlignment   = VerticalAlignment.Center
            };
            addBtn.Click += async (s, e) => await ShowOfferEditor();

            _pushBtn = new Button
            {
                Content           = "⬆  Push to GitHub",
                Classes           = { "SuccessBtn" },
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled         = GitHubStoreSync.HasPat
            };
            if (!GitHubStoreSync.HasPat)
                ToolTip.SetTip(_pushBtn, "Add \"GithubPat\" to secrets.json to enable push");
            _pushBtn.Click += async (s, e) => await PushToGitHub();

            var headerBtns = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            headerBtns.Children.Add(addBtn);
            headerBtns.Children.Add(_pushBtn);
            Grid.SetColumn(headerBtns, 1);

            headerRow.Children.Add(titleBlock);
            headerRow.Children.Add(headerBtns);
            header.Child = headerRow;

            // ── Sync status strip ────────────────────────────────────────────
            _syncStatusTxt = new TextBlock
            {
                FontSize  = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 150)),
                Margin    = new Thickness(20, 6)
            };
            UpdateSyncStatus();

            // ── Offer list ───────────────────────────────────────────────────
            _offerList = new StackPanel { Spacing = 6, Margin = new Thickness(16, 10) };
            var scroll = new ScrollViewer
            {
                Content = _offerList,
                VerticalScrollBarVisibility   = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            };

            // ── Status bar ───────────────────────────────────────────────────
            _statusTxt = new TextBlock
            {
                Text       = "Ready",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 150)),
                FontSize   = 11,
                Margin     = new Thickness(16, 0)
            };
            var statusBar = new Border
            {
                Background      = new SolidColorBrush(Color.FromRgb(9, 9, 15)),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(25, 25, 40)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding         = new Thickness(0, 6),
                Child           = _statusTxt
            };

            // ── Root layout ──────────────────────────────────────────────────
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetRow(header,       0);
            Grid.SetRow(_syncStatusTxt, 1);
            Grid.SetRow(scroll,       2);
            Grid.SetRow(statusBar,    3);
            root.Children.Add(header);
            root.Children.Add(_syncStatusTxt);
            root.Children.Add(scroll);
            root.Children.Add(statusBar);

            Content = root;

            RebuildList();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            _notif = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3
            };
        }

        // ── Entry point ───────────────────────────────────────────────────────

        /// <summary>
        /// Shows the password gate, then the admin panel if auth succeeds.
        /// </summary>
        public static async Task TryOpenAsync(Window owner)
        {
            bool authed = await AuthenticateAsync(owner);
            if (!authed) return;

            var panel = new AdminPanelDialog();
            await panel.ShowDialog(owner);
        }

        // ── Auth ──────────────────────────────────────────────────────────────

        private static async Task<bool> AuthenticateAsync(Window owner)
        {
            if (!AppSecrets.HasAdminPassword)
                return await SetFirstPasswordAsync(owner);

            return await ShowPasswordPromptAsync(owner, isSetMode: false);
        }

        private static async Task<bool> SetFirstPasswordAsync(Window owner)
        {
            var dlg = BuildAuthWindow("🔑  Set Admin Password",
                "This is the first time you're opening the admin panel.\nSet a password to protect it.",
                isSetMode: true);
            await dlg.ShowDialog(owner);
            return dlg.Tag is bool b && b;
        }

        private static async Task<bool> ShowPasswordPromptAsync(Window owner, bool isSetMode)
        {
            var dlg = BuildAuthWindow("🔐  Admin Access", "Enter admin password to continue.", isSetMode);
            await dlg.ShowDialog(owner);
            return dlg.Tag is bool b && b;
        }

        private static Window BuildAuthWindow(string title, string subtitle, bool isSetMode)
        {
            var dlg = new Window
            {
                Title                 = title,
                Width                 = 360,
                Height                = isSetMode ? 300 : 240,
                Background            = new SolidColorBrush(Color.FromRgb(13, 13, 20)),
                CanResize             = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar         = false,
                Tag                   = false
            };

            var passBox = new TextBox
            {
                Watermark        = isSetMode ? "Choose a password" : "Password",
                PasswordChar     = '●',
                Margin           = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var confirmBox = isSetMode ? new TextBox
            {
                Watermark        = "Confirm password",
                PasswordChar     = '●',
                Margin           = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Stretch
            } : null;

            var errTxt = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(255, 80, 80)),
                FontSize   = 11,
                Margin     = new Thickness(0, 0, 0, 6),
                IsVisible  = false
            };

            var btn = new Button
            {
                Content             = isSetMode ? "Set Password" : "Unlock",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Classes             = { "SecondaryBtn" }
            };

            btn.Click += (s, e) =>
            {
                string pass = passBox.Text ?? "";
                if (string.IsNullOrEmpty(pass))
                {
                    errTxt.Text = "Password cannot be empty."; errTxt.IsVisible = true; return;
                }
                if (isSetMode)
                {
                    string confirm = confirmBox?.Text ?? "";
                    if (pass != confirm) { errTxt.Text = "Passwords do not match."; errTxt.IsVisible = true; return; }
                    AppSecrets.SetAdminPassword(pass);
                    dlg.Tag = true;
                    dlg.Close();
                }
                else
                {
                    if (!AppSecrets.VerifyAdminPassword(pass)) { errTxt.Text = "Incorrect password."; errTxt.IsVisible = true; return; }
                    dlg.Tag = true;
                    dlg.Close();
                }
            };

            // Allow Enter key to submit
            passBox.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter) btn.Command?.Execute(null);
            };

            var form = new StackPanel { Margin = new Thickness(28), Spacing = 0 };
            form.Children.Add(MakeLockIcon());
            form.Children.Add(new TextBlock
            {
                Text       = title,
                Foreground = Brushes.White,
                FontSize   = 16, FontWeight = FontWeight.Bold,
                Margin     = new Thickness(0, 10, 0, 4)
            });
            form.Children.Add(new TextBlock
            {
                Text       = subtitle,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 180)),
                FontSize   = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin     = new Thickness(0, 0, 0, 14)
            });
            form.Children.Add(passBox);
            if (confirmBox != null) form.Children.Add(confirmBox);
            form.Children.Add(errTxt);
            form.Children.Add(btn);

            dlg.Content = form;
            return dlg;
        }

        private static Border MakeLockIcon() => new Border
        {
            Width = 44, Height = 44, CornerRadius = new CornerRadius(22),
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(40, 108, 71, 255)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = "🔐", FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        // ── Offer list ────────────────────────────────────────────────────────

        private void RebuildList()
        {
            _offerList.Children.Clear();
            var offers = StoreManager.Offers.OrderBy(o => o.Title).ToList();

            if (offers.Count == 0)
            {
                _offerList.Children.Add(new TextBlock
                {
                    Text       = "No offers yet. Click ＋ Add Offer to get started.",
                    Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 120)),
                    FontSize   = 13,
                    Margin     = new Thickness(0, 40),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (var offer in offers)
                _offerList.Children.Add(BuildRow(offer));

            _statusTxt.Text = $"{offers.Count} offer{(offers.Count == 1 ? "" : "s")} in the store";
        }

        private Control BuildRow(StoreOffer offer)
        {
            var (platEmoji, platColor) = offer.Platform switch
            {
                StorePlatform.PlayStation => ("🎮", Color.FromRgb(0, 112, 209)),
                StorePlatform.Xbox        => ("🟩", Color.FromRgb(16, 124, 16)),
                _                         => ("💻", Color.FromRgb(108, 71, 255))
            };

            var (statusText, statusColor) = offer.Availability switch
            {
                OfferStatus.SoldOut    => ("Sold Out",    Color.FromRgb(255, 80,  80)),
                OfferStatus.ComingSoon => ("Coming Soon", Color.FromRgb(255, 200, 40)),
                _                      => ("Available",   Color.FromRgb(68,  255, 136))
            };

            // Thumbnail
            var thumb = new Border
            {
                Width = 52, Height = 52, CornerRadius = new CornerRadius(8),
                ClipToBounds = true,
                Background = new SolidColorBrush(Color.FromArgb(60, platColor.R, platColor.G, platColor.B))
            };
            if (!string.IsNullOrEmpty(offer.CoverUrl) && File.Exists(offer.CoverUrl))
            {
                try { thumb.Background = new ImageBrush(new Bitmap(offer.CoverUrl)) { Stretch = Stretch.UniformToFill }; }
                catch { }
            }
            else
            {
                thumb.Child = new TextBlock
                {
                    Text = offer.Title.Length > 0 ? offer.Title[0].ToString().ToUpper() : "?",
                    Foreground = Brushes.White, FontSize = 20, FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            // Info
            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 2 };
            info.Children.Add(new TextBlock
            {
                Text = offer.Title, Foreground = Brushes.White,
                FontSize = 13, FontWeight = FontWeight.SemiBold
            });

            var subRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            subRow.Children.Add(new TextBlock { Text = $"{platEmoji} {offer.Platform}", Foreground = new SolidColorBrush(platColor), FontSize = 11 });
            subRow.Children.Add(new TextBlock { Text = "·", Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 90)), FontSize = 11 });
            subRow.Children.Add(new TextBlock
            {
                Text = offer.Price > 0 ? $"{offer.Price:F0} {offer.Currency}" : "Free",
                Foreground = offer.Price > 0 ? new SolidColorBrush(Color.FromRgb(68, 255, 136)) : new SolidColorBrush(Color.FromRgb(255, 200, 40)),
                FontSize = 11, FontWeight = FontWeight.SemiBold
            });
            subRow.Children.Add(new TextBlock { Text = "·", Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 90)), FontSize = 11 });
            subRow.Children.Add(new TextBlock { Text = statusText, Foreground = new SolidColorBrush(statusColor), FontSize = 11 });
            if (offer.IsHighlighted)
            {
                subRow.Children.Add(new TextBlock { Text = "·", Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 90)), FontSize = 11 });
                subRow.Children.Add(new TextBlock { Text = "⭐ Featured", Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 40)), FontSize = 11 });
            }
            info.Children.Add(subRow);

            if (!string.IsNullOrEmpty(offer.Genre))
                info.Children.Add(new TextBlock { Text = offer.Genre, Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 140)), FontSize = 11 });

            // Action buttons
            var editBtn = new Button { Content = "✏  Edit", Classes = { "SecondaryBtn" }, FontSize = 11, Height = 30 };
            editBtn.Click += async (s, e) => await ShowOfferEditor(offer);

            var deleteBtn = new Button { Content = "🗑  Delete", Classes = { "DangerBtn" }, FontSize = 11, Height = 30 };
            deleteBtn.Click += (s, e) =>
            {
                StoreManager.Remove(offer.Id);
                RebuildList();
                _statusTxt.Text = $"Removed \"{offer.Title}\"";
            };

            var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
            actionRow.Children.Add(editBtn);
            actionRow.Children.Add(deleteBtn);

            var row = new Grid { Margin = new Thickness(0, 0, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // thumb
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // info
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // buttons

            Grid.SetColumn(thumb,     0);
            Grid.SetColumn(info,      1);
            Grid.SetColumn(actionRow, 2);

            row.Children.Add(thumb);
            row.Children.Add(info);
            row.Children.Add(actionRow);

            var card = new Border
            {
                Background      = new SolidColorBrush(Color.FromRgb(15, 15, 24)),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(28, 28, 45)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(10),
                Padding         = new Thickness(12, 10),
                Child           = row
            };
            card.PointerEntered += (s, e) => card.BorderBrush = new SolidColorBrush(Color.FromRgb(108, 71, 255));
            card.PointerExited  += (s, e) => card.BorderBrush = new SolidColorBrush(Color.FromRgb(28, 28, 45));

            return card;
        }

        // ── Add / Edit editor ─────────────────────────────────────────────────

        private async Task ShowOfferEditor(StoreOffer? existing = null)
        {
            bool isNew = existing == null;
            var offer  = existing != null
                ? JsonConvert.DeserializeObject<StoreOffer>(JsonConvert.SerializeObject(existing))!
                : new StoreOffer();

            var editorWin = new Window
            {
                Title                 = isNew ? "＋  New Offer" : "✏  Edit Offer",
                Width                 = 440,
                Height                = 600,
                Background            = new SolidColorBrush(Color.FromRgb(13, 13, 20)),
                CanResize             = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar         = false
            };

            var titleBox    = new TextBox { Text = offer.Title,   Watermark = "Title *" };
            var searchSteamBtn = new Button { Content = "🔍", Width = 36 };
            ToolTip.SetTip(searchSteamBtn, "Search Steam for Title & Cover");

            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            titleRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            Grid.SetColumn(titleBox, 0); Grid.SetColumn(searchSteamBtn, 1);
            titleRow.Children.Add(titleBox); titleRow.Children.Add(searchSteamBtn);
            var priceBox    = new TextBox { Text = offer.Price > 0 ? offer.Price.ToString("F0") : "", Watermark = "Price (e.g. 120)" };
            var currencyBox = new TextBox { Text = offer.Currency, Watermark = "Currency (MAD, USD…)" };
            var genreBox    = new TextBox { Text = offer.Genre ?? "", Watermark = "Genre (optional)" };
            var coverBox    = new TextBox { Text = offer.CoverUrl ?? "", Watermark = "Cover image path (optional)" };
            var urlBox      = new TextBox { Text = offer.StoreUrl ?? "", Watermark = "Product URL (optional)" };
            var tagsBox     = new TextBox { Text = string.Join(", ", offer.Tags), Watermark = "Tags: comma-separated (e.g. Action, RPG)" };
            var descBox     = new TextBox
            {
                Text = offer.Description ?? "", Watermark = "Description…",
                AcceptsReturn = true, Height = 70, TextWrapping = TextWrapping.Wrap
            };

            var highlighted = new CheckBox
            {
                Content     = "⭐  Featured (show in highlight banner)",
                IsChecked   = offer.IsHighlighted,
                Foreground  = new SolidColorBrush(Color.FromRgb(255, 200, 40)),
                FontSize    = 12,
                Margin      = new Thickness(0, 4, 0, 4)
            };

            var platformPicker = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            foreach (var p in Enum.GetValues<StorePlatform>()) platformPicker.Items.Add(p.ToString());
            platformPicker.SelectedItem = offer.Platform.ToString();

            var statusPicker = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            foreach (var s in Enum.GetValues<OfferStatus>()) statusPicker.Items.Add(s.ToString());
            statusPicker.SelectedItem = offer.Availability.ToString();

            // Browse for cover
            var browseBtn = new Button { Content = "📂", Width = 36 };
            browseBtn.Click += async (s, e) =>
            {
                var picker = new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    AllowMultiple  = false, Title = "Select Cover Image",
                    FileTypeFilter = new[]
                    {
                        new Avalonia.Platform.Storage.FilePickerFileType("Images")
                        { Patterns = new[] { "*.png","*.jpg","*.jpeg","*.webp","*.bmp" } }
                    }
                };
                var files = await editorWin.StorageProvider.OpenFilePickerAsync(picker);
                if (files.Count > 0) coverBox.Text = files[0].Path.LocalPath;
            };

            static TextBlock Lbl(string t) => new()
            {
                Text = t, Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 190)),
                FontSize = 11, Margin = new Thickness(0, 8, 0, 3)
            };

            var coverRow = new Grid();
            coverRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            coverRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            Grid.SetColumn(coverBox, 0); Grid.SetColumn(browseBtn, 1);
            coverRow.Children.Add(coverBox); coverRow.Children.Add(browseBtn);

            var priceRow = new Grid();
            priceRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            priceRow.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(90)));
            Grid.SetColumn(priceBox, 0); Grid.SetColumn(currencyBox, 1);
            priceRow.Children.Add(priceBox); priceRow.Children.Add(currencyBox);

            var errTxt = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(255, 80, 80)),
                FontSize   = 11, IsVisible = false
            };

            var saveBtn = new Button
            {
                Content             = isNew ? "＋  Add to Store" : "💾  Save Changes",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin              = new Thickness(0, 12, 0, 0),
                Classes             = { "SecondaryBtn" }
            };
            saveBtn.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text)) { errTxt.Text = "Title is required."; errTxt.IsVisible = true; return; }
                offer.Title = titleBox.Text.Trim();
                if (Enum.TryParse<StorePlatform>(platformPicker.SelectedItem?.ToString(), out var plat)) offer.Platform      = plat;
                if (Enum.TryParse<OfferStatus>(statusPicker.SelectedItem?.ToString(),   out var stat)) offer.Availability  = stat;
                offer.Price         = decimal.TryParse(priceBox.Text, out decimal p) ? Math.Max(0, p) : 0;
                offer.Currency      = string.IsNullOrWhiteSpace(currencyBox.Text) ? "MAD" : currencyBox.Text.Trim().ToUpper();
                offer.Genre         = string.IsNullOrWhiteSpace(genreBox.Text)    ? null  : genreBox.Text.Trim();
                offer.CoverUrl      = string.IsNullOrWhiteSpace(coverBox.Text)    ? null  : coverBox.Text.Trim();
                offer.StoreUrl      = string.IsNullOrWhiteSpace(urlBox.Text)      ? null  : urlBox.Text.Trim();
                offer.Description   = string.IsNullOrWhiteSpace(descBox.Text)     ? null  : descBox.Text.Trim();
                offer.IsHighlighted = highlighted.IsChecked == true;
                offer.Tags          = tagsBox.Text?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                        .ToList() ?? new();

                if (isNew) StoreManager.Add(offer);
                else       StoreManager.Update(offer);

                editorWin.Close();
                RebuildList();
                _statusTxt.Text = isNew ? $"Added \"{offer.Title}\"" : $"Updated \"{offer.Title}\"";
            };

            var form = new StackPanel { Margin = new Thickness(24), Spacing = 0 };
            form.Children.Add(new TextBlock { Text = isNew ? "New Store Offer" : "Edit Offer", Foreground = Brushes.White, FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 16) });
            form.Children.Add(Lbl("Title *"));        form.Children.Add(titleRow);
            form.Children.Add(Lbl("Platform"));       form.Children.Add(platformPicker);
            form.Children.Add(Lbl("Availability"));   form.Children.Add(statusPicker);
            form.Children.Add(Lbl("Price / Currency")); form.Children.Add(priceRow);
            form.Children.Add(Lbl("Genre"));          form.Children.Add(genreBox);
            form.Children.Add(Lbl("Cover Image"));    form.Children.Add(coverRow);
            form.Children.Add(Lbl("Product URL"));    form.Children.Add(urlBox);
            form.Children.Add(Lbl("Tags"));           form.Children.Add(tagsBox);
            form.Children.Add(Lbl("Description"));    form.Children.Add(descBox);
            form.Children.Add(highlighted);
            form.Children.Add(errTxt);
            form.Children.Add(saveBtn);

            editorWin.Content = new ScrollViewer { Content = form, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };

            searchSteamBtn.Click += async (s, e) =>
            {
                string query = titleBox.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(query)) return;

                searchSteamBtn.IsEnabled = false;
                searchSteamBtn.Content = "…";
                var results = await SteamApiSearch.SearchAsync(query);
                searchSteamBtn.IsEnabled = true;
                searchSteamBtn.Content = "🔍";

                if (results.Count == 0)
                {
                    errTxt.Text = "No Steam results found.";
                    errTxt.IsVisible = true;
                    return;
                }

                var selected = await ShowSteamSearchPopup(editorWin, results);
                if (selected != null)
                {
                    titleBox.Text = selected.Title;
                    coverBox.Text = SteamApiSearch.GetLibraryCoverUrl(selected.AppId);
                    errTxt.IsVisible = false;
                }
            };

            await editorWin.ShowDialog(this);
        }

        private async Task<SteamSearchResult?> ShowSteamSearchPopup(Window parent, List<SteamSearchResult> results)
        {
            var dlg = new Window
            {
                Title = "Select Game",
                Width = 400,
                Height = 450,
                Background = new SolidColorBrush(Color.FromRgb(15, 15, 24)),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };

            SteamSearchResult? selected = null;
            var listPanel = new StackPanel { Spacing = 6, Margin = new Thickness(16) };

            foreach (var item in results)
            {
                var btn = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    Background = new SolidColorBrush(Color.FromRgb(25, 25, 35)),
                    Padding = new Thickness(10)
                };

                var itemRow = new Grid();
                itemRow.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(80)));
                itemRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                // Steam tiny_image is 231x87 which is roughly 2.6:1
                var imgBorder = new Border
                {
                    Width = 80, Height = 30, CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 50)),
                    ClipToBounds = true
                };

                // Asynchronously load the capsule image if specified
                if (!string.IsNullOrEmpty(item.TinyImage))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var hc = new HttpClient();
                            var bytes = await hc.GetByteArrayAsync(item.TinyImage);
                            using var ms = new MemoryStream(bytes);
                            var bmp = new Bitmap(ms);
                            Dispatcher.UIThread.Post(() => imgBorder.Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill });
                        }
                        catch { }
                    });
                }

                var txt = new TextBlock
                {
                    Text = item.Title,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };

                Grid.SetColumn(imgBorder, 0); Grid.SetColumn(txt, 1);
                itemRow.Children.Add(imgBorder); itemRow.Children.Add(txt);

                btn.Content = itemRow;
                btn.Click += (s, e) => { selected = item; dlg.Close(); };
                listPanel.Children.Add(btn);
            }

            dlg.Content = new ScrollViewer { Content = listPanel, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
            await dlg.ShowDialog(parent);
            return selected;
        }

        // ── GitHub push ───────────────────────────────────────────────────────

        private async Task PushToGitHub()
        {
            _pushBtn.IsEnabled = false;
            _statusTxt.Text    = "Pushing to GitHub…";

            bool ok = await StoreManager.PushToGitHubAsync();

            _pushBtn.IsEnabled = true;
            if (ok)
            {
                _statusTxt.Text = $"✅  Pushed successfully at {DateTime.Now:HH:mm:ss}";
                _notif?.Show(new Notification("Store Synced", "store.json pushed to GitHub.", NotificationType.Success));
            }
            else
            {
                _statusTxt.Text = "❌  Push failed — check that GithubPat is set in secrets.json.";
                _notif?.Show(new Notification("Push Failed", "Could not push to GitHub. Check your PAT.", NotificationType.Error));
            }
            UpdateSyncStatus();
        }

        private void UpdateSyncStatus()
        {
            string patStatus = GitHubStoreSync.HasPat ? "PAT ✓" : "No PAT — push disabled";
            string fetchTime = StoreManager.LastFetched.HasValue
                ? $"Last fetched: {StoreManager.LastFetched.Value.ToLocalTime():HH:mm:ss}"
                : "Not yet fetched from GitHub";
            _syncStatusTxt.Text = $"GitHub: {GitHubStoreSync.Repo}/{GitHubStoreSync.FilePath}  ·  {fetchTime}  ·  {patStatus}";
        }
    }
}

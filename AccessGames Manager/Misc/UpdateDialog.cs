using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Shown when a new version is available. Displays version info + changelog,
    /// then a live progress bar while downloading. Auto-closes when the new exe
    /// launches to replace itself.
    /// </summary>
    public class UpdateDialog : Window
    {
        // ── Live-update controls ──────────────────────────────────────────────────
        private readonly ProgressBar _bar;
        private readonly TextBlock   _progressTxt;
        private readonly Button      _updateBtn;
        private readonly Button      _laterBtn;

        private readonly UpdateInfo _info;

        public UpdateDialog(UpdateInfo info)
        {
            _info = info;

            Title                 = "Update Available";
            Width                 = 460;
            SizeToContent         = SizeToContent.Height;
            CanResize             = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background            = new SolidColorBrush(Color.FromRgb(13, 13, 20));
            BorderBrush           = new SolidColorBrush(Color.FromRgb(108, 71, 255));
            BorderThickness       = new Thickness(1);

            var root = new StackPanel { Margin = new Thickness(28, 24, 28, 28) };

            // ── Header ──────────────────────────────────────────────────────────
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            header.Children.Add(new Border
            {
                Width        = 32, Height = 32, CornerRadius = new CornerRadius(8),
                Background   = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                Margin       = new Thickness(0, 0, 12, 0),
                Child        = new TextBlock
                {
                    Text = "AG", Foreground = Brushes.White,
                    FontSize = 11, FontWeight = FontWeight.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center
                }
            });

            var titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titleStack.Children.Add(new TextBlock
            {
                Text       = "A new version is available!",
                Foreground = Brushes.White,
                FontSize   = 16, FontWeight = FontWeight.Bold
            });
            titleStack.Children.Add(new TextBlock
            {
                Text       = $"v{info.Version}",
                Foreground = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                FontSize   = 12, FontWeight = FontWeight.SemiBold
            });
            header.Children.Add(titleStack);
            root.Children.Add(header);

            // ── Divider ─────────────────────────────────────────────────────────
            root.Children.Add(new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Margin     = new Thickness(0, 14, 0, 14)
            });

            // ── Changelog ───────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(info.Changelog))
            {
                root.Children.Add(new TextBlock
                {
                    Text           = "WHAT'S NEW",
                    Foreground     = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                    FontSize       = 10, FontWeight = FontWeight.Bold,
                    LetterSpacing  = 2,
                    Margin         = new Thickness(0, 0, 0, 8)
                });

                root.Children.Add(new Border
                {
                    Background      = new SolidColorBrush(Color.FromRgb(18, 18, 30)),
                    CornerRadius    = new CornerRadius(8),
                    Padding         = new Thickness(14, 10, 14, 10),
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                    BorderThickness = new Thickness(1),
                    Margin          = new Thickness(0, 0, 0, 18),
                    Child           = new TextBlock
                    {
                        Text         = info.Changelog,
                        Foreground   = new SolidColorBrush(Color.FromRgb(180, 180, 210)),
                        FontSize     = 12,
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight   = 20
                    }
                });
            }

            // ── Progress bar (hidden until download starts) ──────────────────────
            _bar = new ProgressBar
            {
                Minimum    = 0,
                Maximum    = 100,
                Value      = 0,
                Height     = 6,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                Margin     = new Thickness(0, 0, 0, 6),
                IsVisible  = false
            };
            root.Children.Add(_bar);

            _progressTxt = new TextBlock
            {
                Text                = "Downloading…",
                Foreground          = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                FontSize            = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 0, 0, 16),
                IsVisible           = false
            };
            root.Children.Add(_progressTxt);

            // ── Buttons ─────────────────────────────────────────────────────────
            var btns = new StackPanel
            {
                Orientation         = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing             = 10
            };

            _laterBtn = MakeBtn("Later", Color.FromRgb(30, 30, 46), Color.FromRgb(136, 136, 170));
            _laterBtn.Click += (_, __) => Close();
            btns.Children.Add(_laterBtn);

            _updateBtn = MakeBtn("⬇  Update now", Color.FromRgb(30, 20, 60), Color.FromRgb(108, 71, 255));
            _updateBtn.Click += async (_, __) => await StartDownloadAsync();
            btns.Children.Add(_updateBtn);

            root.Children.Add(btns);
            Content = root;
        }

        private async Task StartDownloadAsync()
        {
            _updateBtn.IsEnabled = false;
            _laterBtn.IsEnabled  = false;
            _bar.IsVisible       = true;
            _progressTxt.IsVisible = true;

            var progress = new Progress<int>(pct =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _bar.Value         = pct;
                    _progressTxt.Text  = $"Downloading… {pct}%";
                });
            });

            try
            {
                await AutoUpdater.DownloadAndReplaceAsync(_info, progress);
                // If we reach here the download failed before launching the new exe
            }
            catch (Exception ex)
            {
                _progressTxt.Foreground = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                _progressTxt.Text       = $"Download failed: {ex.Message}";
                _updateBtn.IsEnabled    = true;
                _laterBtn.IsEnabled     = true;
            }
        }

        private static Button MakeBtn(string label, Color bg, Color fg)
        {
            var btn = new Button
            {
                Content         = label,
                Padding         = new Thickness(18, 9, 18, 9),
                Background      = new SolidColorBrush(bg),
                Foreground      = new SolidColorBrush(fg),
                BorderBrush     = new SolidColorBrush(fg) { Opacity = 0.4 },
                BorderThickness = new Thickness(1),
                FontSize        = 12,
                FontWeight      = FontWeight.SemiBold,
                Cursor          = new Cursor(StandardCursorType.Hand)
            };
            btn.PointerEntered += (s, _) => { if (s is Button b) b.Opacity = 0.85; };
            btn.PointerExited  += (s, _) => { if (s is Button b) b.Opacity = 1.0; };
            return btn;
        }
    }
}

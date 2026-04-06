using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Shown while Steam is running online during the infinite-loop fix.
    /// Counts down from <see cref="TotalSeconds"/> and closes automatically,
    /// or the user can click "Done" to skip the remaining wait.
    /// </summary>
    public class SteamCountdownDialog : Window
    {
        public int TotalSeconds { get; }

        private int _remaining;
        private DispatcherTimer? _timer;

        // Live-update controls
        private readonly TextBlock _countTxt;
        private readonly TextBlock _subTxt;
        private readonly ProgressBar _bar;
        private readonly Button _skipBtn;

        public SteamCountdownDialog(int totalSeconds = 20)
        {
            TotalSeconds = totalSeconds;
            _remaining   = totalSeconds;

            Title                 = "Steam Sync – Fix Infinite Loop";
            Width                 = 420;
            SizeToContent         = SizeToContent.Height;
            CanResize             = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background            = new SolidColorBrush(Color.FromRgb(13, 13, 20));
            BorderBrush           = new SolidColorBrush(Color.FromRgb(108, 71, 255));
            BorderThickness       = new Thickness(1);

            // ── Root layout ────────────────────────────────────────────────────
            var root = new StackPanel { Margin = new Thickness(28, 24, 28, 28) };

            // Header row
            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 0, 0, 18)
            };
            header.Children.Add(new TextBlock
            {
                Text              = "🔧",
                FontSize          = 22,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 10, 0)
            });
            header.Children.Add(new TextBlock
            {
                Text              = "Steam is syncing online…",
                Foreground        = Brushes.White,
                FontSize          = 16,
                FontWeight        = FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center
            });
            root.Children.Add(header);

            // Description
            root.Children.Add(new TextBlock
            {
                Text         = "Steam was launched online so it can update your account data. " +
                               "It will be closed automatically when the timer reaches zero.",
                Foreground   = new SolidColorBrush(Color.FromRgb(136, 136, 170)),
                FontSize     = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin       = new Thickness(0, 0, 0, 24)
            });

            // Big countdown number
            _countTxt = new TextBlock
            {
                Text                = totalSeconds.ToString(),
                Foreground          = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                FontSize            = 64,
                FontWeight          = FontWeight.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 0, 0, 6)
            };
            root.Children.Add(_countTxt);

            // "seconds remaining" sub-label
            _subTxt = new TextBlock
            {
                Text                = "seconds remaining",
                Foreground          = new SolidColorBrush(Color.FromRgb(90, 90, 130)),
                FontSize            = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 0, 0, 20)
            };
            root.Children.Add(_subTxt);

            // Progress bar (fills left-to-right as time passes)
            _bar = new ProgressBar
            {
                Minimum    = 0,
                Maximum    = totalSeconds,
                Value      = 0,
                Height     = 6,
                Margin     = new Thickness(0, 0, 0, 24),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 71, 255))
            };
            root.Children.Add(_bar);

            // Skip button
            _skipBtn = new Button
            {
                Content             = "✔  Done — close Steam now",
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding             = new Thickness(20, 10, 20, 10),
                Background          = new SolidColorBrush(Color.FromRgb(30, 20, 60)),
                Foreground          = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                BorderBrush         = new SolidColorBrush(Color.FromArgb(120, 108, 71, 255)),
                BorderThickness     = new Thickness(1),
                FontSize            = 13,
                FontWeight          = FontWeight.SemiBold,
                Cursor              = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            _skipBtn.Click += (_, __) => CloseDialog();
            root.Children.Add(_skipBtn);

            Content = root;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            _timer          = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick    += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _remaining--;

            _countTxt.Text = _remaining.ToString();
            _bar.Value     = TotalSeconds - _remaining;

            if (_remaining <= 5)
            {
                // Turn red in the final countdown
                var red = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                _countTxt.Foreground = red;
                _bar.Foreground      = red;
                _subTxt.Text         = $"seconds remaining — closing soon";
            }

            if (_remaining <= 0)
                CloseDialog();
        }

        private void CloseDialog()
        {
            _timer?.Stop();
            Close();
        }
    }
}

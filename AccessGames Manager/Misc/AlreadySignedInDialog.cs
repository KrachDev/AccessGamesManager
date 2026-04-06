using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Media;

namespace AccessGamesManager.Misc
{
    public enum AlreadySignedInResult { Reboot, LaunchOnly, Cancel }

    /// <summary>
    /// Lightweight code-only WPF dialog shown when the user clicks a game
    /// whose owner account is already the active Steam session.
    /// </summary>
    public class AlreadySignedInDialog : Window
    {
        public AlreadySignedInResult Result { get; private set; } = AlreadySignedInResult.Cancel;

        public AlreadySignedInDialog(string title, string message, string reboot, string launchOnly, string cancel)
        {
            Title           = title;
            Width           = 440;
            SizeToContent   = Avalonia.Controls.SizeToContent.Height;
            CanResize      = false;
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner;
            Background      = new SolidColorBrush(Color.FromRgb(13, 13, 15));
            BorderBrush     = new SolidColorBrush(Color.FromRgb(30, 30, 46));
            BorderThickness = new Thickness(1);

            var root = new StackPanel { Margin = new Thickness(28, 24, 28, 24) };

            // Icon + title row
            var headerRow = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
            headerRow.Children.Add(new TextBlock
            {
                Text = "⚡", FontSize = 22,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });
            headerRow.Children.Add(new TextBlock
            {
                Text = title,
                Foreground = Avalonia.Media.Brushes.White,
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center
            });
            root.Children.Add(headerRow);

            // Message
            root.Children.Add(new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 170)),
                FontSize = 13,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 24)
            });

            // Buttons
            var btnPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var cancelBtn = MakeBtn(cancel, Color.FromRgb(30, 30, 46), Color.FromRgb(136, 136, 170));
            cancelBtn.Click += (_, __) => { Result = AlreadySignedInResult.Cancel; Close(); };
            btnPanel.Children.Add(cancelBtn);

            var launchBtn = MakeBtn(launchOnly, Color.FromRgb(20, 20, 50), Color.FromRgb(108, 71, 255));
            launchBtn.Click += (_, __) => { Result = AlreadySignedInResult.LaunchOnly; Close(); };
            btnPanel.Children.Add(launchBtn);

            var rebootBtn = MakeBtn(reboot, Color.FromRgb(40, 20, 20), Color.FromRgb(255, 80, 80));
            rebootBtn.Click += (_, __) => { Result = AlreadySignedInResult.Reboot; Close(); };
            btnPanel.Children.Add(rebootBtn);

            root.Children.Add(btnPanel);
            Content = root;
        }

        private static Button MakeBtn(string label, Color bg, Color fg)
        {
            var btn = new Button
            {
                Content = label,
                Padding = new Thickness(16, 8, 16, 8),
                Margin = new Thickness(8, 0, 0, 0),
                Background = new SolidColorBrush(bg),
                Foreground = new SolidColorBrush(fg),
                BorderBrush = new SolidColorBrush(fg) { Opacity = 0.4 },
                BorderThickness = new Thickness(1),
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            btn.PointerEntered += (s, _) =>
            {
                if (s is Button b) b.Opacity = 0.85;
            };
            btn.PointerExited += (s, _) =>
            {
                if (s is Button b) b.Opacity = 1.0;
            };
            return btn;
        }
    }
}

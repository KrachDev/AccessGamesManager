using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Shown when SteamKit2 needs a Steam Guard code (email or authenticator app).
    /// Awaiting <see cref="ShowDialog"/> returns the code the user typed, or null if cancelled.
    /// </summary>
    public class SteamGuardDialog : Window
    {
        public string? Code { get; private set; }

        private readonly TextBox _codeBox;

        public SteamGuardDialog(string prompt, string hint)
        {
            Title                 = "Steam Guard Required";
            Width                 = 400;
            SizeToContent         = SizeToContent.Height;
            CanResize             = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background            = new SolidColorBrush(Color.FromRgb(13, 13, 20));
            BorderBrush           = new SolidColorBrush(Color.FromRgb(108, 71, 255));
            BorderThickness       = new Thickness(1);

            var root = new StackPanel { Margin = new Thickness(28, 24, 28, 28) };

            // Header
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
            header.Children.Add(new TextBlock
            {
                Text              = "🔐",
                FontSize          = 22,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 10, 0)
            });
            header.Children.Add(new TextBlock
            {
                Text              = "Steam Guard",
                Foreground        = Brushes.White,
                FontSize          = 16,
                FontWeight        = FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center
            });
            root.Children.Add(header);

            // Prompt
            root.Children.Add(new TextBlock
            {
                Text         = prompt,
                Foreground   = new SolidColorBrush(Color.FromRgb(136, 136, 170)),
                FontSize     = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin       = new Thickness(0, 0, 0, 6)
            });

            // Hint (e.g. "Check your email: ex***@gmail.com")
            if (!string.IsNullOrEmpty(hint))
                root.Children.Add(new TextBlock
                {
                    Text       = hint,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 71, 255)),
                    FontSize   = 11,
                    FontWeight = FontWeight.SemiBold,
                    Margin     = new Thickness(0, 0, 0, 16)
                });

            // Code input
            _codeBox = new TextBox
            {
                Watermark           = "Enter code…",
                MaxLength           = 10,
                FontSize            = 18,
                FontWeight          = FontWeight.Bold,
                TextAlignment       = TextAlignment.Center,
                Margin              = new Thickness(0, 0, 0, 20),
                LetterSpacing       = 6,
            };
            // Submit on Enter
            _codeBox.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter) Submit();
            };
            root.Children.Add(_codeBox);

            // Buttons
            var btns = new StackPanel
            {
                Orientation         = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing             = 10
            };

            var cancelBtn = MakeBtn("Cancel", Color.FromRgb(30, 30, 46), Color.FromRgb(136, 136, 170));
            cancelBtn.Click += (_, __) => { Code = null; Close(); };
            btns.Children.Add(cancelBtn);

            var okBtn = MakeBtn("Submit", Color.FromRgb(30, 20, 60), Color.FromRgb(108, 71, 255));
            okBtn.Click += (_, __) => Submit();
            btns.Children.Add(okBtn);

            root.Children.Add(btns);
            Content = root;
        }

        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);
            _codeBox.Focus();
        }

        private void Submit()
        {
            string code = (_codeBox.Text ?? "").Trim();
            if (string.IsNullOrEmpty(code)) return;
            Code = code;
            Close();
        }

        private static Button MakeBtn(string label, Color bg, Color fg)
        {
            var btn = new Button
            {
                Content         = label,
                Padding         = new Thickness(16, 8, 16, 8),
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

using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace AccessGames_Manager.Views
{
    public class NodeDownloadDialog : Window
    {
        private ProgressBar? _progressBar;
        private TextBlock? _statusText;
        private TextBlock? _percentText;
        private Button? _cancelBtn;
        private bool _cancelled = false;

        public NodeDownloadDialog()
        {
            Title = "Installing Node.js Runtime";
            Width = 500;
            Height = 300;
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF0D0D14"));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CanResize = false;
            ShowInTaskbar = false;

            BuildUI();
        }

        private void BuildUI()
        {
            var grid = new Grid
            {
                Margin = new Avalonia.Thickness(24),
                RowDefinitions = new Avalonia.Controls.RowDefinitions("Auto,Auto,Auto,*,Auto")
            };

            // Title
            var title = new TextBlock
            {
                Text = "Setting Up Node.js Runtime",
                Foreground = Avalonia.Media.Brushes.White,
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold
            };
            Grid.SetRow(title, 0);
            grid.Children.Add(title);

            // Subtitle
            var subtitle = new TextBlock
            {
                Text = "First startup requires installing the backend runtime. This happens only once.",
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF888899")),
                FontSize = 12,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 6, 0, 0)
            };
            Grid.SetRow(subtitle, 0);
            grid.Children.Add(subtitle);

            // Progress bar
            _progressBar = new ProgressBar
            {
                Height = 8,
                Margin = new Avalonia.Thickness(0, 24, 0, 0),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF6C47FF")),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF1A1A2E"))
            };
            Grid.SetRow(_progressBar, 1);
            grid.Children.Add(_progressBar);

            // Status text
            var statusGrid = new Grid
            {
                Margin = new Avalonia.Thickness(0, 12, 0, 0),
                ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("*,Auto")
            };
            _statusText = new TextBlock
            {
                Text = "Preparing download...",
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF6C47FF")),
                FontSize = 12
            };
            Grid.SetColumn(_statusText, 0);
            statusGrid.Children.Add(_statusText);

            _percentText = new TextBlock
            {
                Text = "0%",
                Foreground = Avalonia.Media.Brushes.White,
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Bold
            };
            Grid.SetColumn(_percentText, 1);
            statusGrid.Children.Add(_percentText);

            Grid.SetRow(statusGrid, 2);
            grid.Children.Add(statusGrid);

            // Warning
            var warning = new TextBlock
            {
                Text = "Please do not close the application during installation.",
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF666688")),
                FontSize = 11,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Margin = new Avalonia.Thickness(0, 0, 0, 24)
            };
            Grid.SetRow(warning, 3);
            grid.Children.Add(warning);

            // Cancel button
            _cancelBtn = new Button
            {
                Content = "Cancel",
                Classes = { "SecondaryBtn" },
                Padding = new Avalonia.Thickness(16, 8),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };
            _cancelBtn.Click += Cancel_Click;
            Grid.SetRow(_cancelBtn, 4);
            grid.Children.Add(_cancelBtn);

            this.Content = grid;
        }

        /// <summary>
        /// Show dialog and download Node.js
        /// Returns true if download succeeded, false if cancelled or failed
        /// </summary>
        public async Task<bool> StartDownloadAsync()
        {
            try
            {
                var progress = new Progress<(int percent, string status)>(report =>
                {
                    if (_progressBar != null) _progressBar.Value = report.percent;
                    if (_statusText != null) _statusText.Text = report.status;
                    if (_percentText != null) _percentText.Text = $"{report.percent}%";
                });

                // Start download
                bool success = await AccessGamesManager.Misc.NodeDownloader.DownloadNodeAsync(progress);

                if (success)
                {
                    if (_cancelBtn != null) _cancelBtn.IsEnabled = false;
                    if (_statusText != null) _statusText.Text = "✓ Installation complete!";
                    await Task.Delay(1500); // Show success message briefly
                    this.Close();
                }
                else
                {
                    if (_statusText != null) _statusText.Text = "✗ Installation failed. Please try again.";
                    if (_cancelBtn != null) _cancelBtn.Content = "Retry";
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download error: {ex.Message}");
                if (_statusText != null) _statusText.Text = $"✗ Error: {ex.Message}";
                return false;
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            _cancelled = true;
            this.Close();
        }
    }
}

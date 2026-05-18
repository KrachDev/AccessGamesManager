import sys

filepath = r'Views\MainWindow.axaml.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

start_str = '                if (multipleOwners)'
end_str = 'card.Child = stack;'

idx_start = content.find(start_str)
idx_end = content.find(end_str) + len(end_str)

if idx_start == -1 or idx_end == -1:
    print('Failed')
    sys.exit(1)

replacement = """                // ── Action row: [▶ Play] [⋯] ────────────────────────────────────
                var playBtn = new Button
                {
                    Content             = "▶  Play",
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

                // Options flyout content
                var flyoutResetTime = new Button
                {
                    Content    = "⏱  Reset Playtime",
                    FontSize   = 11,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromRgb(35, 35, 55)),
                    HorizontalAlignment        = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding    = new Thickness(10, 7, 10, 7),
                    CornerRadius = new CornerRadius(6)
                };

                var flyoutResetAchv = new Button
                {
                    Content    = "🏆  Reset Achievements",
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
                    Spacing  = 4,
                    Margin   = new Thickness(4),
                    MinWidth = 160
                };
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
                card.Child = stack;"""

new_content = content[:idx_start] + replacement + content[idx_end:]

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(new_content)

print('Success')

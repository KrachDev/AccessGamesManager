import sys

filepath = r'Views\MainWindow.axaml.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# ── Replace the flyout panel section ──────────────────────────────────────────
old = """                // Options flyout content
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
                flyoutPanel.Children.Add(flyoutResetAchv);"""

new = """                // ── Options flyout: account selector + actions ─────────────────
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
                    card[ToolTip.TipProperty] = $"AppID: {game.AppID}\\n{Localization.Get("GameTooltipOwner")}: {chosen.PersonaName ?? Localization.Get("UnknownOwner")}\\n{Localization.Get("GameTooltipLaunch")}";
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
                flyoutPanel.Children.Add(flyoutResetAchv);"""

if old not in content:
    print('ERROR: old string not found')
    sys.exit(1)

content = content.replace(old, new, 1)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print('Done')

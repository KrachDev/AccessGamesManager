import sys

filepath = r'Views\MainWindow.axaml.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

method = """        private void NetworkToggle_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (steamData.IsSteamNetworkBlocked())
            {
                steamData.UnblockSteamNetwork();
                HandyControl.Controls.Growl.Success(Localization.Get("GrowlUnblocked"));
            }
            else
            {
                steamData.BlockSteamNetwork();
                HandyControl.Controls.Growl.Success(Localization.Get("GrowlBlocked"));
            }
            RefreshFirewallStatus();
        }

        // ─── GAMES TAB"""

new_content = content.replace('        // ─── GAMES TAB', method)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(new_content)

print('Done')

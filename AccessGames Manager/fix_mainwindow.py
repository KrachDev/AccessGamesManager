"""
Surgical fix for MainWindow.axaml.cs:
1. Restore the missing SETTINGS section (BlockBTN, UnblockBTN, LaunchMode handlers)
2. Remove the leftover dead code after the closing braces at line ~1004
3. Fix the corrupted comment line
"""
import re

with open('Views/MainWindow.axaml.cs', 'rb') as f:
    content = f.read().decode('utf-8', errors='replace')

# Split into lines (normalise CRLF -> LF for processing, restore at end)
content_unix = content.replace('\r\n', '\n').replace('\r', '\n')
lines = content_unix.split('\n')

print(f"Total lines before fix: {len(lines)}")

# ─────────────────────────────────────────────────────────────────
# Step 1: Find the corrupted comment line around 635 and replace
# the whole bad line with the restored settings section
# ─────────────────────────────────────────────────────────────────
SETTINGS_BLOCK = '''
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

        // ════════════════════════════════════════════════════════════════════════════
        // STORE TAB
        // ════════════════════════════════════════════════════════════════════════════
'''

# Find the corrupted line: contains both "SETT" and "STORE TAB" or "═══"
bad_line_idx = None
for i, line in enumerate(lines):
    if 'SETT' in line and '══════' in line:
        bad_line_idx = i
        break
    if '// â"€â"€â"€ SETT' in line or ('// â"€â"€â"€ SETT' in line):
        bad_line_idx = i
        break

if bad_line_idx is None:
    # Try broader search for the corrupted comment
    for i, line in enumerate(lines):
        if i > 630 and i < 645 and 'SETT' in line and len(line) > 40:
            bad_line_idx = i
            print(f"Found bad line at {i+1}: {repr(line[:60])}")
            break

if bad_line_idx is None:
    # Last resort - search for the exact corruption signature
    for i, line in enumerate(lines):
        if i > 630 and i < 645 and ('â"€â"€â"€' in line or '\u2500' in line) and len(line) > 60:
            bad_line_idx = i
            print(f"Found bad line at {i+1}: {repr(line[:60])}")
            break

if bad_line_idx is not None:
    print(f"Fixing corrupted line at {bad_line_idx+1}")
    # Find the STORE TAB comment line that follows (also corrupted)
    store_tab_end = bad_line_idx
    for j in range(bad_line_idx, bad_line_idx + 5):
        if j < len(lines) and ('STORE TAB' in lines[j] or '═══════' in lines[j]):
            store_tab_end = j
    
    # Replace the bad range with the proper settings block
    lines = lines[:bad_line_idx] + SETTINGS_BLOCK.split('\n') + lines[store_tab_end + 1:]
    print(f"Settings block restored. New length: {len(lines)}")
else:
    print("WARNING: Could not find bad settings line — skipping step 1")

# ─────────────────────────────────────────────────────────────────
# Step 2: Find and remove the dead code after closing braces
# The pattern: line has `}hickness(0, 12, 0, 0)`
# ─────────────────────────────────────────────────────────────────
dead_start = None
dead_end   = None

for i, line in enumerate(lines):
    if '}hickness' in line or ('}' in line and 'hickness(0, 12, 0, 0)' in line):
        dead_start = i
        break

if dead_start is not None:
    # The dead block ends with the last `}` line which is the final closing of the file
    # We need to find where the real closing braces are (the `    }\n}` at the end)
    # Look for `AddStoreOfferBtn_Click` in the dead section as end marker
    for i in range(dead_start, len(lines)):
        if 'AddStoreOfferBtn_Click' in lines[i]:
            dead_end = i
            break
    
    if dead_end is not None:
        # Find end of the dead block (last closing brace sequence)
        # Skip ahead to find `    }\n}` at the very end
        for i in range(dead_end, len(lines)):
            if lines[i].strip() == '}' and i + 1 < len(lines) and lines[i+1].strip() == '}':
                dead_end = i + 1
                break
        
        print(f"Removing dead code from line {dead_start+1} to {dead_end+1}")
        lines = lines[:dead_start] + ['        }', '    }', '}', '']
        print(f"Dead code removed. New length: {len(lines)}")
    else:
        print("Could not find AddStoreOfferBtn_Click in dead section — removing from dead_start to end")
        lines = lines[:dead_start] + ['        }', '    }', '}', '']
else:
    print("No dead code found — skipping step 2")

# ─────────────────────────────────────────────────────────────────
# Write result
# ─────────────────────────────────────────────────────────────────
result = '\r\n'.join(lines)
with open('Views/MainWindow.axaml.cs', 'wb') as f:
    f.write(result.encode('utf-8'))

print(f"\nDone. Final line count: {len(lines)}")

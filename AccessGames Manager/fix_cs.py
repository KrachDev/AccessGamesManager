import re

cs_file = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml.cs"
with open(cs_file, 'r', encoding='utf-8') as f:
    cs = f.read()

# using System;
if "using System;" not in cs:
    cs = "using System;\nusing System.Threading.Tasks;\n" + cs

# Remove var bmp = new BitmapImage();
cs = cs.replace('var bmp = new BitmapImage();', '')

# LinearGradientBrush
brush_old = r'new LinearGradientBrush(Color.FromRgb(25, 25, 45), Color.FromRgb(40, 30, 70), new Avalonia.Point(0, 0), new Avalonia.Point(1, 1))'
brush_new = '''new LinearGradientBrush { StartPoint = new Avalonia.RelativePoint(0,0,Avalonia.RelativeUnit.Relative), EndPoint = new Avalonia.RelativePoint(1,1,Avalonia.RelativeUnit.Relative), GradientStops = new Avalonia.Media.GradientStops{ new Avalonia.Media.GradientStop(Color.FromRgb(25,25,45),0), new Avalonia.Media.GradientStop(Color.FromRgb(40,30,70),1) } }'''
cs = cs.replace(brush_old, brush_new)

# ToolTips
cs = re.sub(r'ToolTip\s*=\s*([^;]+);', r'ToolTip.SetTip(card, \1);', cs, count=1)
cs = re.sub(r'ToolTip\s*=\s*(.*?)\s*\}', r'}', cs) # For the button inside object initializer, we will add it below
cs = cs.replace('switchBtn.Click += (s, e) =>', 'ToolTip.SetTip(switchBtn, $"{Localization.Get("SwitchTooltip")} {user.AccountName}");\n            switchBtn.Click += (s, e) =>')

# ComboBox
cs = cs.replace('new HandyControl.Controls.ComboBox', 'new Avalonia.Controls.ComboBox')

# PointerPressed and Dialog
cs = cs.replace('card.PointerPressed += (s, e) => OnGameCardClick', 'card.PointerPressed += async (s, e) => await OnGameCardClick')
cs = cs.replace('private void OnGameCardClick', 'private async Task OnGameCardClick')
cs = cs.replace('dlg.Owner = this;\n                dlg.ShowDialog();', 'await dlg.ShowDialog(this);')

# BadgeBG -> BadgeBorder
cs = cs.replace('BadgeBG.Color                =', 'BadgeBorder.Background = new SolidColorBrush(')
cs = cs.replace('? Color.FromRgb(42, 10, 10) : Color.FromRgb(10, 42, 10);', '? Color.FromRgb(42, 10, 10) : Color.FromRgb(10, 42, 10));')

with open(cs_file, 'w', encoding='utf-8') as f:
    f.write(cs)

xaml_file = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml"
with open(xaml_file, 'r', encoding='utf-8') as f:
    xaml = f.read()

xaml = xaml.replace('<Border.Background>\n                                        <SolidColorBrush x:Name="BadgeBG" Color="#FF0A2A0A"/>\n                                    </Border.Background>', '')
xaml = xaml.replace('<Border HorizontalAlignment="Right" VerticalAlignment="Top" CornerRadius="8" Padding="10,5">', '<Border x:Name="BadgeBorder" HorizontalAlignment="Right" VerticalAlignment="Top" CornerRadius="8" Padding="10,5">')

with open(xaml_file, 'w', encoding='utf-8') as f:
    f.write(xaml)

print("Fix applied.")

import os
import re

source_xaml = r"C:\Users\Kracher\source\repos\KrachDev\SteamAccountsManager\SteamAccountsManager(SAM)\MainWindow.xaml"
source_cs = r"C:\Users\Kracher\source\repos\KrachDev\SteamAccountsManager\SteamAccountsManager(SAM)\MainWindow.xaml.cs"

dest_xaml = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml"
dest_cs = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml.cs"

with open(source_xaml, 'r', encoding='utf-8') as f:
    xaml = f.read()

# Replace WPF specifically for MainWindow
xaml = xaml.replace('<hc:Window', '<Window')
xaml = xaml.replace('</hc:Window>', '</Window>')
xaml = xaml.replace('xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"', 'xmlns="https://github.com/avaloniaui"')
xaml = xaml.replace('x:Class="AccessGamesManager.MainWindow"', 'x:Class="AccessGames_Manager.Views.MainWindow"\n        xmlns:vm="using:AccessGamesManager.ViewModels"\n        x:DataType="vm:MainWindowViewModel"')
xaml = xaml.replace('Visibility="Visible"', 'IsVisible="True"')
xaml = xaml.replace('Visibility="Collapsed"', 'IsVisible="False"')
xaml = xaml.replace('Visibility="Hidden"', 'IsVisible="False"')
xaml = re.sub(r'ToolTip="([^"]+)"', r'ToolTip.Tip="\1"', xaml)
xaml = xaml.replace('<hc:SearchBar', '<TextBox Watermark="Search..."')
xaml = xaml.replace('SearchStarted="GamesSearchBox_SearchStarted"', 'TextChanged="GamesSearchBox_TextChanged"')
xaml = xaml.replace('NonClientAreaBackground=', 'Background=') # Just dup

with open(dest_xaml, 'w', encoding='utf-8') as f:
    f.write(xaml)


with open(source_cs, 'r', encoding='utf-8') as f:
    cs = f.read()

cs = cs.replace('using System.Windows;', 'using Avalonia.Controls;\nusing Avalonia.Interactivity;\nusing Avalonia;\nusing Avalonia.Layout;')
cs = cs.replace('using System.Windows.Controls;', 'using System.Linq;\nusing System.Collections.Generic;')
cs = cs.replace('using System.Windows.Media;', 'using Avalonia.Media;')
cs = cs.replace('using System.Windows.Media.Imaging;', 'using Avalonia.Media.Imaging;')
cs = cs.replace('using Window = HandyControl.Controls.Window;', '')
cs = cs.replace('namespace AccessGamesManager', 'namespace AccessGames_Manager.Views')

cs = cs.replace('PageGames.Visibility    = page == PageGames    ? Visibility.Visible : Visibility.Collapsed;', 'PageGames.IsVisible = page == PageGames;')
cs = cs.replace('PageAccounts.Visibility = page == PageAccounts ? Visibility.Visible : Visibility.Collapsed;', 'PageAccounts.IsVisible = page == PageAccounts;')
cs = cs.replace('PageSettings.Visibility = page == PageSettings ? Visibility.Visible : Visibility.Collapsed;', 'PageSettings.IsVisible = page == PageSettings;')

cs = cs.replace('PageGames.Visibility == Visibility.Visible', 'PageGames.IsVisible')

cs = cs.replace('FontWeights.Bold', 'Avalonia.Media.FontWeight.Bold')
cs = cs.replace('FontWeights.SemiBold', 'Avalonia.Media.FontWeight.SemiBold')

cs = cs.replace('System.Windows.Input.Cursors.Hand', 'new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)')

cs = cs.replace('Color.FromRgb', 'Color.FromRgb')

cs = cs.replace('HandyControl.Data.FunctionEventArgs<string> e', 'Avalonia.Controls.TextChangedEventArgs e')

# Remove Effect assignment
cs = re.sub(r'card\.Effect\s*=\s*new\s*System\.Windows\.Media\.Effects\.DropShadowEffect[^;]+;', '', cs)
cs = cs.replace('card.Effect = null;', '')

# Styles
cs = cs.replace('btn.Style = (Style)FindResource("NavBtnActive");', '// btn.Style (mocked)')
cs = cs.replace('b.Style = (Style)FindResource("NavBtn");', '// b.Style (mocked)')
cs = cs.replace('Style = (Style)FindResource("PrimaryBtn"),', '')


cs = cs.replace('Brushes.', 'Avalonia.Media.Brushes.')
cs = cs.replace('TextAlignment = TextAlignment', 'TextAlignment = Avalonia.Media.TextAlignment')
cs = cs.replace('TextWrapping = TextWrapping', 'TextWrapping = Avalonia.Media.TextWrapping')
cs = cs.replace('TextTrimming = TextTrimming', 'TextTrimming = Avalonia.Media.TextTrimming')

# MouseLeftButtonDown -> PointerPressed
cs = cs.replace('MouseLeftButtonDown', 'PointerPressed')
cs = cs.replace('MouseEnter', 'PointerEntered')
cs = cs.replace('MouseLeave', 'PointerExited')

# ImageBrush
cs = cs.replace('new ImageBrush(new BitmapImage(new Uri(user.AvatarImage)))', 'new ImageBrush(new Avalonia.Media.Imaging.Bitmap(user.AvatarImage))')
cs = cs.replace('bmp.BeginInit();', '')
cs = cs.replace('bmp.UriSource = new Uri(imagePath);', 'var bmp = new Avalonia.Media.Imaging.Bitmap(imagePath);')
cs = cs.replace('bmp.CacheOption = BitmapCacheOption.OnLoad;', '')
cs = cs.replace('bmp.EndInit();', '')

cs = cs.replace('Point(', 'Avalonia.Point(')

with open(dest_cs, 'w', encoding='utf-8') as f:
    f.write(cs)

print("Migration script completed.")

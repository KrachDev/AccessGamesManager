import re

cs_file = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml.cs"
with open(cs_file, 'r', encoding='utf-8') as f:
    cs = f.read()

cs = cs.replace('private void GamesSearchBox_SearchStarted', 'private void GamesSearchBox_TextChanged')

with open(cs_file, 'w', encoding='utf-8') as f:
    f.write(cs)

xaml_file = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml"
with open(xaml_file, 'r', encoding='utf-8') as f:
    xaml = f.read()

if '<Window' not in xaml:
    xaml = xaml.replace('        x:Class="AccessGames_Manager.Views.MainWindow"', '<Window x:Name="ParentWindow" x:Class="AccessGames_Manager.Views.MainWindow"')

xaml = re.sub(r'Style="\{StaticResource ([^\}]+)\}"', r'Classes="\1"', xaml)

with open(xaml_file, 'w', encoding='utf-8') as f:
    f.write(xaml)

print("Final XAML properties patched safely.")

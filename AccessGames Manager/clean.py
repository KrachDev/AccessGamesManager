import os

axaml_path = r'c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.axaml'

with open(axaml_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
in_store_page = False

for line in lines:
    if line.strip() == 'xmlns:local="clr-namespace:AccessGames_Manager"':
        new_lines.append(line)
        new_lines.append('        xmlns:views="clr-namespace:AccessGames_Manager.Views"\n')
        continue
    
    if line.strip() == '<!-- ── STORE PAGE ── -->':
        in_store_page = True
        new_lines.append(line)
        new_lines.append('            <Grid x:Name="PageStore" IsVisible="False">\n')
        new_lines.append('                <views:StoreView x:Name="StoreViewControl" />\n')
        new_lines.append('            </Grid>\n\n')
        continue
        
    if in_store_page:
        if line.strip() == '<!-- ── SETTINGS PAGE ── -->':
            in_store_page = False
            new_lines.append(line)
        continue
        
    if not in_store_page:
        new_lines.append(line)

with open(axaml_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
    
store_cs_path = r'c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Views\MainWindow.Store.axaml.cs'

store_cs_text = """using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace AccessGames_Manager.Views
{
    public partial class MainWindow
    {
        private async void NavStore_Click(object? sender, RoutedEventArgs e)
        {
            SetNav(NavStore, PageStore);
            await StoreViewControl.LoadStoreAsync();
        }
    }
}
"""
with open(store_cs_path, 'w', encoding='utf-8') as f:
    f.write(store_cs_text)

print("cleaned")

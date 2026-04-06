import os
import glob

def replace_in_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replacements
    content = content.replace("using System.Windows.Controls;", "using Avalonia.Controls;")
    content = content.replace("using System.Windows.Media;", "using Avalonia.Media;")
    content = content.replace("using System.Windows.Media.Imaging;", "using Avalonia.Media.Imaging;")
    content = content.replace("using System.Windows.Documents;", "") 
    content = content.replace("using System.Windows.Shapes;", "using Avalonia.Controls.Shapes;")
    content = content.replace("using System.Windows.Data;", "using Avalonia.Data;")
    content = content.replace("using System.Windows.Navigation;", "") 
    content = content.replace("using System.Windows;", "using Avalonia;\nusing Avalonia.Interactivity;\nusing Avalonia.Layout;\nusing Avalonia.Input;")
    content = content.replace("using Avalonia.ControlsApplicationLifetime;", "using Avalonia.Controls.ApplicationLifetimes;")
    content = content.replace("using System.Security.RightsManagement;", "") 
    
    # Common types
    content = content.replace("System.Windows.Thickness", "Avalonia.Thickness")
    content = content.replace("System.Windows.Input.Cursors.Hand", "new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)")
    content = content.replace("FontWeights.SemiBold", "Avalonia.Media.FontWeight.SemiBold")
    content = content.replace("FontWeights.Bold", "Avalonia.Media.FontWeight.Bold")
    content = content.replace("TextWrapping.Wrap", "Avalonia.Media.TextWrapping.Wrap")
    content = content.replace("Brushes.White", "Avalonia.Media.Brushes.White")
    content = content.replace("SizeToContent.Height", "Avalonia.Controls.SizeToContent.Height")
    content = content.replace("ResizeMode.NoResize", "Avalonia.Controls.WindowResizeMode.NoResize")
    content = content.replace("WindowStartupLocation.CenterOwner", "Avalonia.Controls.WindowStartupLocation.CenterOwner")
    content = content.replace("Orientation.Horizontal", "Avalonia.Layout.Orientation.Horizontal")
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

base_dir = r"c:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager"
for root, dirs, files in os.walk(base_dir):
    for file in files:
        if file.endswith(".cs") and "obj" not in root and "bin" not in root:
            replace_in_file(os.path.join(root, file))

print("Namespace replacement complete.")

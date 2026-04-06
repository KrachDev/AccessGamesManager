using AccessGamesManager.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Data;

using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using Avalonia.Controls.Shapes;

namespace AccessGamesManager.Tools
{
    /// <summary>
    /// Interaction logic for LibGame.xaml
    /// </summary>
    public partial class LibGame : UserControl
    {
        public bool animated = false;

        public LibGame()
        {
            InitializeComponent();
            AnimationsLib(GameImageBD);
        }

        private void GameImageBD_MouseDown(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            RBgame.IsChecked = true;
        }

        private void RBgame_Checked(object sender, RoutedEventArgs e)
        {
            GameImageBD.BorderThickness = new Thickness(3);
        }

        private void RBgame_Unchecked(object sender, RoutedEventArgs e)
        {
            GameImageBD.BorderThickness = new Thickness(0);

        }

        public void AnimationsLib(Border libGame)
        {
            if (animated)
            {
                

            }
        }
    }
}

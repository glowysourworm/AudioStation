using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioStation.Views
{
    /// <summary>
    /// Interaction logic for PlayerControlView.xaml
    /// </summary>
    public partial class PlayerControlView : UserControl
    {
        public PlayerControlView()
        {
            InitializeComponent();
        }
        private void VolumeControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.VolumeButton.IsChecked = false;
        }

        private void VolumeControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}

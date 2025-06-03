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

using AudioStation.Model;

namespace AudioStation.Views
{
    public partial class ManagerView : UserControl
    {
        public ManagerView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as LibraryEntry;

                this.LocalEntryItemView.DataContext = item;
                this.MusicBrainzItemView.DataContext = item;
            }
        }
    }
}

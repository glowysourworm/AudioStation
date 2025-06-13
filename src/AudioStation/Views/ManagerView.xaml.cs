using System.Windows.Controls;

using AudioStation.Core.Model;

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
            }
        }
    }
}

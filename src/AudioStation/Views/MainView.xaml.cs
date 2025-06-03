using System.Windows.Controls;

using AudioStation.ViewModels;

namespace AudioStation.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void OnShowOutputMessagesClick(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).ShowOutputMessages = !(this.DataContext as MainViewModel).ShowOutputMessages;
        }
    }
}

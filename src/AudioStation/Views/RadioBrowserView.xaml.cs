using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.ViewModels.RadioViewModel;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Views
{
    public partial class RadioBrowserView : UserControl
    {
        public event SimpleEventHandler<RadioStationViewModel> StartStationEvent;

        public RadioBrowserView()
        {
            InitializeComponent();
        }

        private void RadioStationLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (e.OriginalSource as FrameworkElement).DataContext as RadioStationViewModel;

            if (viewModel != null)
            {
                if (this.StartStationEvent != null)
                    this.StartStationEvent(viewModel);
            }
        }
    }
}

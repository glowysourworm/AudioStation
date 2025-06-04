using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.ViewModel.LibraryViewModel;

namespace AudioStation.Views
{
    public partial class AlbumView : UserControl
    {
        public event EventHandler<TitleViewModel> TrackSelected;

        public AlbumView()
        {
            InitializeComponent();
        }

        private void OnTracksDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in this.TracksLB.Items.Cast<TitleViewModel>())
            {
                if (item == (e.OriginalSource as FrameworkElement).DataContext)
                {
                    if (this.TrackSelected != null)
                        this.TrackSelected(this, item);

                    e.Handled = true;

                    return;
                }
            }
        }
    }
}

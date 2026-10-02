using System.Windows;

using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Views
{
    public partial class AlbumViewSmall : AlbumView
    {
        public AlbumViewSmall()
        {
            InitializeComponent();
        }

        private void TracksLB_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var trackViewModel = (e.OriginalSource as FrameworkElement).DataContext as TrackViewModel;

            if (trackViewModel != null)
                Load(trackViewModel);
        }
    }
}

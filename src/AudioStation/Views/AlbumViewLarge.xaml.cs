using System.Windows;
using System.Windows.Input;

using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Views
{
    public partial class AlbumViewLarge : AlbumView
    {
        public event EventHandler<TrackViewModel> TrackSelected;

        public AlbumViewLarge()
        {
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void TracksLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var trackViewModel = (e.OriginalSource as FrameworkElement).DataContext as TrackViewModel;

            if (trackViewModel != null)
                Load(trackViewModel);
        }
    }
}

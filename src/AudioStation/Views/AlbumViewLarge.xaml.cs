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

        private void OnTracksDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in this.TracksLB.Items.Cast<TrackViewModel>())
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

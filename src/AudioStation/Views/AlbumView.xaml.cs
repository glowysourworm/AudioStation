using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Views
{
    public partial class AlbumView : UserControl
    {
        public event EventHandler<LibraryEntryViewModel> TrackSelected;

        public AlbumView()
        {
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void OnTracksDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in this.TracksLB.Items.Cast<LibraryEntryViewModel>())
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

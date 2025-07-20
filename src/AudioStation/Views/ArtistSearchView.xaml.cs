using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Component.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.LibraryManagerViewModels;
using AudioStation.ViewModels.LibraryViewModels;

using EMA.ExtendedWPFVisualTreeHelper;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class ArtistSearchView : UserControl
    {
        private readonly INowPlayingViewModelLoader _nowPlayingViewModelLoader;
        private readonly IViewModelLoader _viewModelLoader;
        private readonly IIocEventAggregator _eventAggregator;

        private int _pageNumber = 0;
        private bool _resizing = false;
        private bool _loading = false;

        public ArtistSearchView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public ArtistSearchView(IViewModelLoader viewModelLoader,
                                IIocEventAggregator eventAggregator,
                                INowPlayingViewModelLoader nowPlayingViewModelLoader)
        {
            _viewModelLoader = viewModelLoader;
            _eventAggregator = eventAggregator;
            _nowPlayingViewModelLoader = nowPlayingViewModelLoader;

            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _resizing = true;

            base.OnRenderSizeChanged(sizeInfo);

            _resizing = false;
        }

        // Primary load method to send playlist to the main view model
        private async Task LoadPlaylist(LibraryEntryViewModel selectedTitle, AlbumViewModel selectedAlbum, ArtistViewModel selectedArtist)
        {
            // Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Loading Playlist..."));

            var nowPlayingData = await _nowPlayingViewModelLoader.LoadPlaylist(selectedArtist, selectedAlbum, selectedTitle);

            var eventData = new LoadPlaylistEventData()
            {
                NowPlayingData = nowPlayingData,
                StartPlayback = true
            };

            // Load Playlist -> Start Playback
            _eventAggregator.GetEvent<LoadPlaylistEvent>().Publish(eventData);

            // Loading Finished
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss(NavigationView.NowPlaying));
        }

        #region Artist / Album (LHS)
        private async void AlbumsLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Load Playlist for the whole album
            var viewModel = this.DataContext as LibraryViewModel;
            var album = WpfVisualFinders.FindParent<ListBoxItem>(e.OriginalSource as DependencyObject).DataContext as AlbumViewModel;
            var artist = this.ArtistLB.SelectedItem as ArtistViewModel;

            if (viewModel != null && album != null && artist != null)
            {
                await LoadPlaylist(album.Tracks.First(), album, artist);
            }
        }

        private void AlbumsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Load Selected Album(s) into the AlbumDetailLB

            if (e.AddedItems != null &&
                e.AddedItems.Count > 0)
            {
                this.AlbumDetailLB.ScrollIntoView(e.AddedItems[0]);
            }
        }
        #endregion

        #region Album Detail
        private async void AlbumDetailLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Load Playlist for the entire album
            var viewModel = this.DataContext as LibraryViewModel;
            var album = (e.OriginalSource as FrameworkElement).DataContext as AlbumViewModel;
            var artist = this.ArtistLB.SelectedItem as ArtistViewModel;

            if (viewModel != null && album != null && artist != null)
            {
                await LoadPlaylist(album.Tracks.First(), album, artist);
            }
        }
        private void AlbumDetailLB_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }
        private async void AlbumViewItem_TrackSelected(object sender, LibraryEntryViewModel selectedTrack)
        {
            var viewModel = this.DataContext as LibraryViewModel;
            var album = (sender as AlbumView).DataContext as AlbumViewModel;
            var artist = this.ArtistLB.SelectedItem as ArtistViewModel;

            if (viewModel != null && album != null && artist != null)
            {
                foreach (var track in album.Tracks)
                {
                    if (track == selectedTrack)
                    {
                        await LoadPlaylist(selectedTrack, album, artist);
                        return;
                    }
                }
            }
        }
        #endregion
    }
}

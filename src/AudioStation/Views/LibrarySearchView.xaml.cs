using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Controller.Interface;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.ServiceComponent;

using EMA.ExtendedWPFVisualTreeHelper;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibrarySearchView : UserControl
    {
        private readonly INowPlayingService _nowPlayingViewModelLoader;
        private readonly IAudioStationComponentController _audioStationComponentController;
        private readonly IIocEventAggregator _eventAggregator;

        private int _pageNumber = 0;
        private bool _resizing = false;
        private bool _loading = false;

        public LibrarySearchView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public LibrarySearchView(IAudioStationComponentController audioStationComponentController,
                                 IIocEventAggregator eventAggregator,
                                 INowPlayingService nowPlayingViewModelLoader)
        {
            _audioStationComponentController = audioStationComponentController;
            _eventAggregator = eventAggregator;
            _nowPlayingViewModelLoader = nowPlayingViewModelLoader;

            InitializeComponent();

            this.DataContextChanged += LibrarySearchView_DataContextChanged;
        }

        private void LibrarySearchView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibrarySearchViewModel;

            // Service Component 
            if (viewModel != null)
            {
                // -> Load
                if (viewModel.CanLoad())
                    viewModel.LoadCommand.Execute(null);

                // -> Execute
                if (viewModel.CanExecute())
                    viewModel.ExecuteCommand.Execute(null);
            }

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _resizing = true;

            base.OnRenderSizeChanged(sizeInfo);

            _resizing = false;
        }

        // Primary load method to send playlist to the main view model
        private void LoadPlaylist(TrackViewModel selectedTitle, AlbumViewModel selectedAlbum, ArtistViewModel selectedArtist)
        {
            //// Loading...
            //_eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Loading Playlist..."));

            //var nowPlayingData = await _nowPlayingViewModelLoader.LoadPlaylist(selectedArtist, selectedAlbum, selectedTitle);

            //var eventData = new LoadPlaylistEventData()
            //{
            //    NowPlayingData = nowPlayingData,
            //    StartPlayback = true
            //};

            //// Load Playlist -> Start Playback
            //_eventAggregator.GetEvent<LoadPlaylistEvent>().Publish(eventData);

            //// Loading Finished
            //_eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss(NavigationView.NowPlaying));
        }

        #region Artist / Album (LHS)
        private void ResultsLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Load Playlist for the whole album
            var viewModel = this.DataContext as LibraryViewModel;
            var album = WpfVisualFinders.FindParent<ListBoxItem>(e.OriginalSource as DependencyObject).DataContext as AlbumViewModel;
            var artist = this.ResultsLB.SelectedItem as ArtistViewModel;

            if (viewModel != null && album != null && artist != null)
            {
                LoadPlaylist(album.Tracks.First(), album, artist);
            }
        }
        private void ResultsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            var artist = this.ResultsLB.SelectedItem as ArtistViewModel;

            if (viewModel != null && album != null && artist != null)
            {
                LoadPlaylist(album.Tracks.First(), album, artist);
            }
        }
        private void AlbumDetailLB_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }
        private async void AlbumViewItem_TrackSelected(object sender, TrackViewModel selectedTrack)
        {
            var viewModel = this.DataContext as LibraryViewModel;
            var album = (sender as AlbumView).DataContext as AlbumViewModel;
            var artist = this.ResultsLB.SelectedItem as ArtistViewModel;

            if (viewModel != null && album != null && artist != null)
            {
                foreach (var track in album.Tracks)
                {
                    if (track == selectedTrack)
                    {
                        LoadPlaylist(selectedTrack, album, artist);
                        return;
                    }
                }
            }
        }
        #endregion
    }
}

using System.Windows;
using System.Windows.Controls;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModels;

using EMA.ExtendedWPFVisualTreeHelper;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class NowPlayingView : UserControl
    {
        private readonly IViewModelLoader _viewModelLoader;
        private readonly IAudioController _audioController;
        private readonly IIocEventAggregator _eventAggregator;

        private int _pageNumber = 0;

        public NowPlayingView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public NowPlayingView(IViewModelLoader viewModelLoader, IAudioController audioController, IIocEventAggregator eventAggregator)
        {
            _viewModelLoader = viewModelLoader;
            _audioController = audioController;
            _eventAggregator = eventAggregator;

            InitializeComponent();

            this.DataContextChanged += NowPlayingView_DataContextChanged;
        }

        private void NowPlayingView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryViewModel;

            if (viewModel != null)
            {
                if (viewModel.Artists.Count == 0)
                    LoadArtistPage(1, true);
            }
        }

        private void LoadArtistPage(int pageNumber, bool reset)
        {
            _pageNumber = pageNumber;

            var viewModel = this.DataContext as LibraryViewModel;

            if (viewModel != null)
            {
                var result = _viewModelLoader.LoadArtistPage(new PageRequest<Mp3FileReferenceArtist, string>()
                {
                    PageNumber = pageNumber,
                    PageSize = 30,                              // TODO: Observable collections don't work w/ the view
                    OrderByCallback = (entity) => entity.Name,
                    WhereCallback = !string.IsNullOrWhiteSpace(viewModel.ArtistSearch) ? this.ArtistContainsCallback : null
                });

                if (result.Results.Any())
                {
                    viewModel.LoadArtists(result, reset);
                }
            }
        }

        private bool ArtistContainsCallback(Mp3FileReferenceArtist artist)
        {
            var viewModel = this.DataContext as LibraryViewModel;

            if (string.IsNullOrWhiteSpace(viewModel?.ArtistSearch))
                return true;

            return artist.Name.Contains(viewModel?.ArtistSearch, StringComparison.OrdinalIgnoreCase);
        }

        private void OnArtistSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryViewModel;

            if (e.AddedItems.Count > 0 && viewModel != null)
            {
                // View Artist Detail
                //this.ArtistDetailLB.ItemsSource = (e.AddedItems[0] as ArtistViewModel).Albums;
            }
        }

        private void OnArtistDetailDoubleClick(object? sender, RoutedEventArgs e)
        {
            var albums = this.ArtistDetailLB.ItemsSource as IEnumerable<AlbumViewModel>;

            if (albums != null)
            {
                var firstAlbum = albums.First();

                LoadPlaylist(firstAlbum.Tracks.First(), firstAlbum);
            }
        }

        private void OnPlaylistDoubleClick(object? sender, RoutedEventArgs e)
        {
            var selectedTrack = (e.Source as Control).DataContext as LibraryEntryViewModel;
            var albums = this.ArtistDetailLB.ItemsSource as IEnumerable<AlbumViewModel>;

            if (selectedTrack != null && albums != null)
            {
                // Contains (by reference) not yet hooked up for sorted observable collection
                var selectedAlbum = albums.First(album => album.Tracks.Any(x => x.FileName == selectedTrack.FileName));

                LoadPlaylist(selectedTrack, selectedAlbum);
            }
        }

        private void AlbumViewItem_TrackSelected(object sender, LibraryEntryViewModel selectedTrack)
        {
            var viewModel = this.DataContext as LibraryViewModel;
            var album = (sender as AlbumView).DataContext as AlbumViewModel;

            if (viewModel != null && album != null)
            {
                foreach (var track in album.Tracks)
                {
                    if (track == selectedTrack)
                    {
                        LoadPlaylist(selectedTrack, album);
                        return;
                    }
                }
            }
        }

        private void LoadPlaylist(LibraryEntryViewModel selectedTitle, AlbumViewModel selectedAlbum)
        {
            _audioController.Play(new NowPlayingViewModel()
            {
                Album = selectedTitle.Album,
                Artist = selectedTitle.PrimaryArtist,
                Source = selectedTitle.FileName,
                SourceType = StreamSourceType.File,
                Title = selectedTitle.Title
            });

            //var playlist = new Playlist();
            //playlist.Name = selectedAlbum.PrimaryArtist + " / " + selectedAlbum.Album;

            // Load tracks for playback
            //foreach (var track in selectedAlbum.Tracks)
            // {
            //playlist.Tracks.Add(track);
            //}

            // Setup playback (needs revision; but this works with the IAudioController)
            //playlist.LoadPlayback(selectedTitle);

            // Set View Model
            //(this.DataContext as MainViewModel).Playlist = playlist;

            // Play Selected Track(s) (the audio controller <-> playlist handle the rest)
            /*
            MainViewModel.AudioController.Play(new NowPlayingViewModel()
            {
                Album = selectedAlbum.Album,
                Artist = selectedAlbum.Artist,
                CurrentTime = TimeSpan.Zero,
                Duration = selectedTitle.Duration,
                Source = selectedTitle.FileName,
                SourceType = StreamSourceType.File,
                Title = selectedTitle.Name
            });
            */
        }

        private void ArtistDetailLB_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }

        private void ArtistLB_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = WpfVisualFinders.FindChild<ScrollViewer>(sender as DependencyObject);
            var viewModel = this.DataContext as LibraryViewModel;

            if (scrollViewer != null && viewModel != null)
            {
                if (scrollViewer.VerticalOffset >= (0.8 * scrollViewer.ScrollableHeight))
                {
                    LoadArtistPage(_pageNumber + 1, false);
                }
            }
        }

        private void OnArtistSearchChanged(object sender, TextChangedEventArgs e)
        {
            LoadArtistPage(1, true);
        }
    }
}

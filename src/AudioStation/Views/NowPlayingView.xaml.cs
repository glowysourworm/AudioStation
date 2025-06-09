using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.ViewModel.LibraryViewModels;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class NowPlayingView : UserControl
    {
        private readonly IAudioController _audioController;
        private readonly IIocEventAggregator _eventAggregator;

        public NowPlayingView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public NowPlayingView(IAudioController audioController, IIocEventAggregator eventAggregator)
        {
            _audioController = audioController;
            _eventAggregator = eventAggregator;

            InitializeComponent();
        }

        private void OnArtistSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryViewModel;

            if (e.AddedItems.Count > 0 && viewModel != null)
            {
                // View Artist Detail
                this.ArtistDetailLB.ItemsSource = (e.AddedItems[0] as ArtistViewModel).Albums;
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
    }
}

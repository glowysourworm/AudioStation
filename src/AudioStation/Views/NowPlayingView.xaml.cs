using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels;

namespace AudioStation.Views
{
    /// <summary>
    /// Interaction logic for NowPlayingView.xaml
    /// </summary>
    public partial class NowPlayingView : UserControl
    {
        public NowPlayingView()
        {
            InitializeComponent();
        }

        private void OnArtistSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

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
            var selectedTrack = (e.Source as Control).DataContext as TitleViewModel;
            var albums = this.ArtistDetailLB.ItemsSource as IEnumerable<AlbumViewModel>;

            if (selectedTrack != null && albums != null)
            {
                // Contains (by reference) not yet hooked up for sorted observable collection
                var selectedAlbum = albums.First(album => album.Tracks.Any(x => x.FileName == selectedTrack.FileName));

                LoadPlaylist(selectedTrack, selectedAlbum);
            }
        }

        private void AlbumViewItem_TrackSelected(object sender, TitleViewModel selectedTrack)
        {
            var viewModel = this.DataContext as MainViewModel;
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

        private void LoadPlaylist(TitleViewModel selectedTitle, AlbumViewModel selectedAlbum)
        {
            var playlist = new Playlist();
            playlist.Name = selectedAlbum.Artist + " / " + selectedAlbum.Album;

            // Load tracks for playback
            foreach (var track in selectedAlbum.Tracks)
            {
                playlist.Tracks.Add(track);
            }

            // Setup playback (needs revision; but this works with the IAudioController)
            playlist.LoadPlayback(selectedTitle);

            // Set View Model
            (this.DataContext as MainViewModel).Playlist = playlist;

            // Play Selected Track(s) (the audio controller <-> playlist handle the rest)
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
        }

        private void ArtistDetailLB_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }
    }
}

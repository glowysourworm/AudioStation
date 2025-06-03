using System.Windows;
using System.Windows.Controls;

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
            var library = (this.DataContext as MainViewModel)?.Library;

            if (e.AddedItems.Count > 0 && library != null)
            {
                // ??? (it was null...)
                if (e.AddedItems[0] != null)
                {
                    // View Artist Detail
                    this.ArtistDetailLB.ItemsSource = (e.AddedItems[0] as ArtistViewModel).Albums;
                }
            }
        }

        private void OnArtistDetailDoubleClick(object? sender, RoutedEventArgs e)
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
            MainViewModel.AudioController.Play(playlist);
        }
    }
}

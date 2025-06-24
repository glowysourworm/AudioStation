using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using AudioStation.Component.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.PlaylistViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class NowPlayingView : UserControl
    {
        private readonly ILastFmClient _lastFmClient;
        private readonly IAudioDBClient _audioDBClient;

        bool _loading = false;

        [IocImportingConstructor]
        public NowPlayingView(ILastFmClient lastFmClient, IAudioDBClient audioDBClient)
        {
            _lastFmClient = lastFmClient;
            _audioDBClient = audioDBClient;

            InitializeComponent();

            this.DataContextChanged += OnDataContextChanged;
        }

        private async Task LoadArtwork()
        {
            if (_loading)
                return;

            _loading = true;

            var viewModel = this.DataContext as PlaylistViewModel;

            if (viewModel != null && 
                viewModel.NowPlaying != null)
            {
                viewModel.LastFmNowPlaying = await _lastFmClient.GetNowPlayingInfo(viewModel.NowPlaying.Artist.Artist, viewModel.NowPlaying.Album.Album);

                // AudioDB
                var artist = await _audioDBClient.SearchArtist(viewModel.NowPlaying.Artist.Artist);

                // Lookup album by name (this could be via Music Brainz)
                if (artist != null)
                {
                    var album = artist.Albums.FirstOrDefault(x => x.AlbumName == viewModel.NowPlaying.Album.Album);

                    if (album != null)
                    {
                        viewModel.AudioDBNowPlaying = await _audioDBClient.CreateNowPlaying(artist.IdArtist, album.IdAlbum);
                    }
                }                
            }

            _loading = false;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldViewModel = e.OldValue as PlaylistViewModel;
            var newViewModel = e.NewValue as PlaylistViewModel;

            if (oldViewModel != null)
                oldViewModel.PropertyChanged -= OnPlaylistChanged;

            if (newViewModel != null)
                newViewModel.PropertyChanged += OnPlaylistChanged;

            LoadArtwork();
        }

        private void OnPlaylistChanged(object? sender, PropertyChangedEventArgs e)
        {
            LoadArtwork();
        }

        private void OnPlaylistDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}

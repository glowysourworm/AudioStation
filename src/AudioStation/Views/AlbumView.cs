using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Core.Model;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    public abstract class AlbumView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly ILibraryLoaderService _libraryLoaderService;

        public AlbumView()
        {
            _eventAggregator = IocContainer.Get<IIocEventAggregator>();
            _libraryLoaderService = IocContainer.Get<ILibraryLoaderService>();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var album = this.DataContext as AlbumViewModel;

            if (album != null)
            {
                LoadAlbum(album);

                e.Handled = true;
            }
        }

        protected void Load(TrackViewModel track)
        {
            var album = this.DataContext as AlbumViewModel;

            if (album != null)
            {
                var nowPlaying = _libraryLoaderService.GetDefaultPlaylist(track);

                //var lastFmResponse = await _lastFmClient.GetNowPlayingInfo(artist.Artist, album.Album);
                //var spotifyResponse = await _spotifyClient.CreateNowPlaying(artist.Artist, album.Album);
                //var musicBrainzResponse = await _musicBrainzClient.QueryArtist(artist.Artist);

                //var musicBrainzArtist = musicBrainzResponse?.FirstOrDefault();

                //var fanartBackgrounds = musicBrainzArtist != null ? await _fanartClient.GetArtistBackgrounds(musicBrainzArtist.Id.ToString()) : null;
                //var fanartArtistThumbs = musicBrainzArtist != null ? await _fanartClient.GetArtistImages(musicBrainzArtist.Id.ToString()) : null;

                // Load Playlist -> Start Playback
                _eventAggregator.GetEvent<LoadPlaybackEvent>().Publish(new LoadPlaybackEventData()
                {
                    Source = track.FileName,
                    SourceType = StreamSourceType.File
                });
                _eventAggregator.GetEvent<StartPlaybackEvent>().Publish();
            }
        }

        private void LoadAlbum(AlbumViewModel album)
        {
            var nowPlaying = _libraryLoaderService.GetDefaultPlaylist(album);

            // Load Playlist -> Start Playback
            if (nowPlaying.CurrentTrack != null)
            {
                _eventAggregator.GetEvent<LoadPlaybackEvent>().Publish(new LoadPlaybackEventData()
                {
                    Source = nowPlaying.CurrentTrack.Track.FileName,
                    SourceType = StreamSourceType.File
                });
                _eventAggregator.GetEvent<StartPlaybackEvent>().Publish();
            }
        }
    }
}

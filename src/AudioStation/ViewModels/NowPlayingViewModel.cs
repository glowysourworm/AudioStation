using System.Collections.ObjectModel;

using AudioStation.Core.Component;
using AudioStation.Core.Model;
using AudioStation.Event;
using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class NowPlayingViewModel : ViewModelBase
    {
        PlaylistViewModel _playlist;

        // Vendor Information
        string _bestImage;
        string _artistArticle;
        string _artistSummary;
        ObservableCollection<string> _externalLinks;

        public PlaylistViewModel Playlist
        {
            get { return _playlist; }
            set { this.RaiseAndSetIfChanged(ref _playlist, value); }
        }

        /// <summary>
        /// Best image to display as the playlist's image, gathered from local and 3rd party remote data
        /// </summary>
        public string BestImage
        {
            get { return _bestImage; }
            set { this.RaiseAndSetIfChanged(ref _bestImage, value); }
        }
        public string ArtistArticle
        {
            get { return _artistArticle; }
            set { this.RaiseAndSetIfChanged(ref _artistArticle, value); }
        }
        public string ArtistSummary
        {
            get { return _artistSummary; }
            set { this.RaiseAndSetIfChanged(ref _artistSummary, value); }
        }

        /// <summary>
        /// List of links related to the album and artist
        /// </summary>
        public ObservableCollection<string> ExternalLinks
        {
            get { return _externalLinks; }
            set { this.RaiseAndSetIfChanged(ref _externalLinks, value); }
        }

        private readonly IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public NowPlayingViewModel(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            this.ExternalLinks = new ObservableCollection<string>();
            this.BestImage = string.Empty;
            this.ArtistArticle = string.Empty;
            this.ArtistSummary = string.Empty;
            this.Playlist = new PlaylistViewModel();

            eventAggregator.GetEvent<LoadPlaylistEvent>().Subscribe(OnLoadPlaylist);
            eventAggregator.GetEvent<LoadNextTrackEvent>().Subscribe(OnLoadNextTrack);
            eventAggregator.GetEvent<LoadPreviousTrackEvent>().Subscribe(OnLoadPreviousTrack);
            eventAggregator.GetEvent<PlaybackStateChangedEvent>().Subscribe(OnPlaybackStateChanged);
        }

        public void SetNowPlaying(IPlaylistEntryViewModel track, bool startTrack)
        {
            if (this.Playlist == null)
                throw new Exception("Must first load playlist before calling SetNowPlaying:  NowPlayingViewModel");

            if (!this.Playlist.Entries.Contains(track))
                throw new ArgumentException("Provided track must be contained in the playlist entries collection:  NowPlayingViewModel");

            this.Playlist.CurrentTrack.IsPlaying = false;
            this.Playlist.CurrentTrack = track;
            this.Playlist.CurrentTrack.IsPlaying = true;

            if (startTrack)
                StartPlayback(track);
        }

        private void OnLoadPlaylist(LoadPlaylistEventData eventData)
        {
            this.ArtistArticle = eventData.NowPlayingData.ArtistArticle;
            this.ArtistSummary = eventData.NowPlayingData.ArtistSummary;
            this.BestImage = eventData.NowPlayingData.BestImage;

            this.ExternalLinks.Clear();
            this.ExternalLinks.AddRange(eventData.NowPlayingData.ExternalLinks);

            this.Playlist.Entries.Clear();
            this.Playlist.Entries.AddRange(eventData.NowPlayingData.Entries);
            this.Playlist.CurrentTrack = eventData.NowPlayingData.NowPlaying;
            this.Playlist.CurrentTrack.IsPlaying = true;

            if (eventData.StartPlayback)
                StartPlayback(this.Playlist.CurrentTrack);
        }
        private void OnLoadNextTrack()
        {
            if (this.Playlist != null)
            {
                if (this.Playlist.CurrentTrack != null)
                {
                    this.Playlist.CurrentTrack.IsPlaying = false;
                    this.Playlist.CurrentTrack = this.Playlist.Entries.Next(this.Playlist.CurrentTrack);
                }
                else
                {
                    this.Playlist.CurrentTrack = this.Playlist.Entries.First();
                }

                // Set current track IsPlaying
                this.Playlist.CurrentTrack.IsPlaying = true;

                StartPlayback(this.Playlist.CurrentTrack);
            }
        }
        private void OnLoadPreviousTrack()
        {
            if (this.Playlist != null)
            {
                if (this.Playlist.CurrentTrack != null)
                {
                    this.Playlist.CurrentTrack.IsPlaying = false;
                    this.Playlist.CurrentTrack = this.Playlist.Entries.Previous(this.Playlist.CurrentTrack);
                }
                else
                {
                    this.Playlist.CurrentTrack = this.Playlist.Entries.First();
                }

                // Set current track IsPlaying
                this.Playlist.CurrentTrack.IsPlaying = true;

                StartPlayback(this.Playlist.CurrentTrack);
            }
        }
        private void StartPlayback(IPlaylistEntryViewModel startTrack)
        {
            _eventAggregator.GetEvent<StopPlaybackEvent>().Publish();
            _eventAggregator.GetEvent<LoadPlaybackEvent>().Publish(new LoadPlaybackEventData()
            {
                Source = startTrack.Track.FileName,
                SourceType = StreamSourceType.File
            });
            _eventAggregator.GetEvent<StartPlaybackEvent>().Publish();
        }
        private void OnPlaybackStateChanged(PlaybackStateChangedEventData data)
        {
            // Not User Initiated -> go ahead through playlist
            if (!data.UserInitiated && data.State == PlayStopPause.Stop)
            {
                OnLoadNextTrack();
            }
        }
    }
}

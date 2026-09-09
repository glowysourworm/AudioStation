using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(ILastFmClient))]
    public class LastFmClient : ILastFmClient
    {
        // Last FM:  Primary client
        private LastfmClient? _client;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private readonly IOutputController _outputController;

        private IAudioStationDataService.Status _status;

        [IocImportingConstructor]
        public LastFmClient(IOutputController outputController)
        {
            _outputController = outputController;
            _client = null;
        }

        public async Task<LastFmNowPlaying> GetNowPlayingInfo(string artist, string album)
        {
            if (_client == null)
                return null;

            try
            {
                // Album / Artist Detail
                var albumResponse = await _client.Album.GetInfoAsync(artist, album, false);
                var artistResponse = await _client.Artist.GetInfoAsync(artist);

                // Status OK -> Create bitmap image from the url
                if (albumResponse.Status == LastResponseStatus.Successful &&
                    artistResponse.Status == LastResponseStatus.Successful)
                {
                    return new LastFmNowPlaying()
                    {
                        AlbumImage = albumResponse.Content.Images?.Largest?.AbsoluteUri ?? string.Empty,
                        AlbumUrl = albumResponse.Content.Url?.AbsoluteUri ?? string.Empty,
                        ArtistMainImage = artistResponse.Content.MainImage?.Largest?.AbsoluteUri ?? string.Empty,
                        ArtistUrl = artistResponse.Content.Url?.AbsoluteUri ?? string.Empty,
                        ArtistYearFormed = artistResponse.Content.Bio?.YearFormed ?? 0,
                        BioContent = artistResponse.Content.Bio?.Content ?? string.Empty,
                        BioSummary = artistResponse.Content.Bio?.Summary ?? string.Empty,
                        Tracks = new List<LastFmTrack>(albumResponse.Content.Tracks.Select(track =>
                        {
                            return new LastFmTrack()
                            {
                                ArtistImage = track.ArtistImages?.Largest?.AbsoluteUri ?? string.Empty,
                                ArtistUrl = track.ArtistUrl?.AbsoluteUri ?? string.Empty,
                                Image = track.Images?.Largest?.AbsoluteUri ?? string.Empty,
                                Url = track.Url?.AbsoluteUri ?? string.Empty
                            };
                        }))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error contacting LastFm:  {0}", LogMessageServiceType.LastFm, LogLevel.Error, ex, ex.Message);

                return null;
            }
        }

        protected LastfmClient? Authenticate(AudioStationConfiguration configuration)
        {
            try
            {
                OnStatusChanged(IAudioStationDataService.Status.Working);

                // Last FM API
                var client = new LastfmClient(configuration.LastFmAPIKey, configuration.LastFmAPISecret);

                if (!client.Auth.Authenticated)
                    OnStatusChanged(IAudioStationDataService.Status.Error);

                else
                    OnStatusChanged(IAudioStationDataService.Status.Idle);

                return client;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.LastFm, LogLevel.Error, ex, ex.Message);

                OnStatusChanged(IAudioStationDataService.Status.Error);

                return null;
            }
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "LastFm Client";
        }
        public string GetDisplayName()
        {
            return "LastFm Client";
        }
        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        public IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            _client = Authenticate(configuration);

            return _status;
        }

        public Task<IAudioStationDataService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public IAudioStationDataService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationDataService.Status.Idle;
        }

        public Task<IAudioStationDataService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationDataService.Status.Idle);
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationDataService.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationDataService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}

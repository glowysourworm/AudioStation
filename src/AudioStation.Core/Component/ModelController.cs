using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IModelController))]
    public class ModelController : IModelController
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IMusicBrainzDbClient _musicBrainzDbClient;         // Local Client
        private readonly IMusicBrainzClient _musicBrainzClient;             // Remote Client

        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        LogLevel _currentLogLevel;
        bool _currentLogVerbosity;

        [IocImportingConstructor]
        public ModelController(IIocEventAggregator eventAggregator,
                               IAudioStationDbClient audioStationDbClient,
                               IMusicBrainzDbClient musicBrainzDbClient,
                               IMusicBrainzClient musicBrainzClient)
        {
            _eventAggregator = eventAggregator;
            _audioStationDbClient = audioStationDbClient;
            _musicBrainzDbClient = musicBrainzDbClient;
            _musicBrainzClient = musicBrainzClient;
            _currentLogLevel = LogLevel.Trace;
            _currentLogVerbosity = true;

            // Update log output configuration
            _eventAggregator.GetEvent<LogConfigurationChangedEvent>().Subscribe(payload =>
            {
                if (payload.Type == LogMessageType.Database)
                {
                    _currentLogLevel = payload.Level;
                    _currentLogVerbosity = payload.Verbose;
                }
            });
        }

        #region Audio Station Database Methods
        public bool AddUpdateLibraryEntry(string fileName, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, TagLib.File tagRef)
        {
            try
            {
                _audioStationDbClient.AddUpdateLibraryEntry(fileName, fileAvailable, fileLoadError, fileLoadErrorMessage, tagRef);
                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
                return false;
            }
        }
        public bool AddUpdateRadioEntry(Core.Model.M3U.M3UStream entry)
        {
            if (string.IsNullOrEmpty(entry.StreamSource) ||
                string.IsNullOrEmpty(entry.Title))
                throw new ArgumentException("M3UStream must have a stream source and a title");

            try
            {
                _audioStationDbClient.AddUpdateRadioEntry(entry);
                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
                return false;
            }
        }
        public bool AddRadioEntries(IEnumerable<Core.Model.M3U.M3UStream> entries)
        {
            try
            {
                _audioStationDbClient.AddRadioEntries(entries);
                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
                return false;
            }
        }
        public IEnumerable<Mp3FileReference> GetArtistFiles(int artistId)
        {
            try
            {
                return _audioStationDbClient.GetArtistFiles(artistId);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
            }

            return Enumerable.Empty<Mp3FileReference>();
        }
        public IEnumerable<Mp3FileReferenceAlbum> GetArtistAlbums(int artistId, bool isPrimaryArtist)
        {
            try
            {
                return _audioStationDbClient.GetArtistAlbums(artistId, isPrimaryArtist);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
            }

            return Enumerable.Empty<Mp3FileReferenceAlbum>();
        }
        public IEnumerable<Mp3FileReference> GetAlbumTracks(int albumId)
        {
            try
            {
                return _audioStationDbClient.GetAlbumTracks(albumId);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
            }

            return Enumerable.Empty<Mp3FileReference>();
        }
        #endregion

        #region Generic Methods
        public PageResult<TEntity> GetAudioStationPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : AudioStationEntityBase
        {
            try
            {
                return _audioStationDbClient.GetPage(request);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return PageResult<TEntity>.GetDefault();
        }
        public IEnumerable<TEntity> GetAudioStationEntities<TEntity>() where TEntity : AudioStationEntityBase
        {
            try
            {
                return _audioStationDbClient.GetEntities<TEntity>();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return Enumerable.Empty<TEntity>();
        }
        public TEntity GetAudioStationEntity<TEntity>(int id) where TEntity : AudioStationEntityBase
        {
            try
            {
                return _audioStationDbClient.GetEntity<TEntity>(id);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return null;
        }
        public bool UpdateAudioStationEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase
        {
            try
            {
                return _audioStationDbClient.UpdateEntity<TEntity>(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return false;
            }
        }
        public PageResult<TEntity> GetMusicBrainzPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                return _musicBrainzDbClient.GetPage(request);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return PageResult<TEntity>.GetDefault();
        }
        public IEnumerable<TEntity> GetMusicBrainzEntities<TEntity>() where TEntity : MusicBrainzEntityBase
        {
            try
            {
                return _musicBrainzDbClient.GetEntities<TEntity>();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return Enumerable.Empty<TEntity>();
        }
        public TEntity GetMusicBrainzEntity<TEntity>(Guid musicBrainzId) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                return _musicBrainzDbClient.GetEntity<TEntity>(musicBrainzId);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return null;
        }
        public bool UpdateMusicBrainzEntity<TEntity>(TEntity entity) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                return _musicBrainzDbClient.UpdateEntity<TEntity>(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return false;
            }
        }
        #endregion

        #region Remote Queries / Local Cache
        public void AddUpdateMusicBrainzArtist(IArtist musicBrainzArtist)
        {
            try
            {
                var entity = new MusicBrainzArtistEntity()
                {
                    Id = musicBrainzArtist.Id,
                    Annotation = musicBrainzArtist.Annotation,
                    Country = musicBrainzArtist.Country,
                    Disambiguation = musicBrainzArtist.Disambiguation,
                    Name = musicBrainzArtist.Name,
                    SortName = musicBrainzArtist.SortName
                };

                // Add
                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddEntity(entity);

                // Update
                else
                    _musicBrainzDbClient.UpdateEntity(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzRecording(IRecording musicBrainzRecording)
        {
            try
            {
                var entity = new MusicBrainzRecordingEntity()
                {
                    Id = musicBrainzRecording.Id,
                    Annotation = musicBrainzRecording.Annotation,
                    Length = musicBrainzRecording.Length,
                    Title = musicBrainzRecording.Title,
                    Disambiguation = musicBrainzRecording.Disambiguation                    
                };

                // Add
                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddEntity(entity);

                // Update
                else
                    _musicBrainzDbClient.UpdateEntity(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzRelease(IRelease musicBrainzRelease)
        {
            try
            {
                var entity = new MusicBrainzReleaseEntity()
                {
                    Id = musicBrainzRelease.Id,
                    Annotation = musicBrainzRelease.Annotation,
                    Asin = musicBrainzRelease.Asin,
                    Barcode = musicBrainzRelease.Barcode,
                    Date = musicBrainzRelease.Date?.NearestDate ?? DateTime.MinValue,
                    Title = musicBrainzRelease.Title,
                    Packaging = musicBrainzRelease.Packaging,
                    Quality = musicBrainzRelease.Quality,
                    Status = musicBrainzRelease.Status,
                    Country = musicBrainzRelease.Country,
                    Disambiguation = musicBrainzRelease.Disambiguation
                };

                // Add
                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddEntity(entity);

                // Update
                else
                    _musicBrainzDbClient.UpdateEntity(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzTrack(ITrack musicBrainzTrack)
        {
            try
            {
                var entity = new MusicBrainzTrackEntity()
                {
                    Id = musicBrainzTrack.Id,
                    Length = musicBrainzTrack.Length,
                    MusicBrainzRecordingId = musicBrainzTrack.Recording.Id,
                    Number = musicBrainzTrack.Number,
                    Position = musicBrainzTrack.Position,
                    Title = musicBrainzTrack.Title
                };

                // Add
                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddEntity(entity);

                // Update
                else
                    _musicBrainzDbClient.UpdateEntity(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzTag(ITag musicBrainzTag, MusicBrainzEntityBase musicBrainzEntity)
        {
            try
            {
                var entity = new MusicBrainzTagEntity()
                {
                    Id = System.Guid.NewGuid(),         // Guess there wasn't an ID
                    Name = musicBrainzTag.Name,
                };

                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddTag(entity, musicBrainzEntity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzGenre(IGenre musicBrainzGenre, MusicBrainzEntityBase musicBrainzEntity)
        {
            try
            {
                var entity = new MusicBrainzGenreEntity()
                {
                    Id = musicBrainzGenre.Id,                    
                    Name = musicBrainzGenre.Name,
                };

                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddGenre(entity, musicBrainzEntity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzUrl(IUrl musicBrainzUrl, MusicBrainzEntityBase musicBrainzEntity)
        {
            try
            {
                var entity = new MusicBrainzUrlEntity()
                {
                    Id = musicBrainzUrl.Id,
                    Url = musicBrainzUrl.Resource?.AbsoluteUri ?? string.Empty,
                };

                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddUrl(entity, musicBrainzEntity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzMedium(Guid musicBrainzReleaseId, IMedium musicBrainzMedium)
        {
            try
            {
                var entity = new MusicBrainzMediumEntity()
                {
                    Id = System.Guid.NewGuid(),                  // No GUID from music brainz. The record belongs to a release entity.
                    Format = musicBrainzMedium.Format,
                    MusicBrainzReleaseId = musicBrainzReleaseId,
                    Position = musicBrainzMedium.Position,
                    Title = musicBrainzMedium.Title,
                    TrackCount = musicBrainzMedium.TrackCount,
                    TrackOffset = musicBrainzMedium.TrackOffset
                };

                // Add
                if (!_musicBrainzDbClient.Any<MusicBrainzMediumEntity>(entity => entity.MusicBrainzReleaseId == musicBrainzReleaseId))
                    _musicBrainzDbClient.AddEntity(entity);

                // Update
                else
                    _musicBrainzDbClient.UpdateEntity(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }
        public void AddUpdateMusicBrainzLabel(ILabel musicBrainzLabel)
        {
            try
            {
                var entity = new MusicBrainzLabelEntity()
                {
                    Id = musicBrainzLabel.Id,
                    LabelCode = musicBrainzLabel.LabelCode,
                    Annotation = musicBrainzLabel.Annotation,
                    Country = musicBrainzLabel.Country,
                    Disambiguation= musicBrainzLabel.Disambiguation,
                    Name = musicBrainzLabel.Name,
                    SortName = musicBrainzLabel.SortName,
                    Type = musicBrainzLabel.Type
                };

                // Add
                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddEntity(entity);

                // Update
                else
                    _musicBrainzDbClient.UpdateEntity(entity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }
        }

        public MusicBrainzArtistEntity GetMusicBrainzArtist(Guid? musicBrainzId, string artistName, string albumName, string trackName)
        {
            try
            {
                // Id
                if (musicBrainzId != null)
                {
                    // Check Guid (Local)
                    var artistEntity = _musicBrainzDbClient.GetEntity<MusicBrainzArtistEntity>(musicBrainzId.Value);

                    if (artistEntity != null)
                        return artistEntity;

                    // Query (Remote)
                    var artists = _musicBrainzClient.QueryArtists(artistName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var artist in artists)
                    {
                        AddUpdateMusicBrainzArtist(artist);
                    }

                    // Try Again
                    artistEntity = _musicBrainzDbClient.GetEntity<MusicBrainzArtistEntity>(musicBrainzId.Value);

                    if (artistEntity == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }

                    return artistEntity;
                }

                // Search
                else
                {
                    // Query (Remote)
                    var artists = _musicBrainzClient.QueryArtists(artistName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var artist in artists)
                    {
                        AddUpdateMusicBrainzArtist(artist);
                    }

                    if (artists.Any())
                    {
                        return _musicBrainzDbClient.GetEntity<MusicBrainzArtistEntity>(artists.First().Id);
                    }

                    else
                    {
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public MusicBrainzRecordingEntity GetMusicBrainzRecording(Guid? musicBrainzId, string artistName, string albumName, string trackName)
        {
            try
            {
                // Id
                if (musicBrainzId != null)
                {
                    // Check Guid (Local)
                    var recordingEntity = _musicBrainzDbClient.GetEntity<MusicBrainzRecordingEntity>(musicBrainzId.Value);

                    if (recordingEntity != null)
                        return recordingEntity;

                    // Query (Remote)
                    var recordings = _musicBrainzClient.QueryRecordings(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var recording in recordings)
                    {
                        AddUpdateMusicBrainzRecording(recording);
                    }

                    // Try Again
                    recordingEntity = _musicBrainzDbClient.GetEntity<MusicBrainzRecordingEntity>(musicBrainzId.Value);

                    if (recordingEntity == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Recording Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }

                    return recordingEntity;
                }

                // Search
                else
                {
                    // Query (Remote)
                    var recordings = _musicBrainzClient.QueryRecordings(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var recording in recordings)
                    {
                        AddUpdateMusicBrainzRecording(recording);
                    }

                    if (recordings.Any())
                    {
                        return _musicBrainzDbClient.GetEntity<MusicBrainzRecordingEntity>(recordings.First().Id);
                    }

                    else
                    {
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public MusicBrainzReleaseEntity GetMusicBrainzRelease(Guid? musicBrainzId, string artistName, string albumName, string trackName)
        {
            try
            {
                // Id
                if (musicBrainzId != null)
                {
                    // Check Guid (Local)
                    var releaseEntity = _musicBrainzDbClient.GetEntity<MusicBrainzReleaseEntity>(musicBrainzId.Value);

                    if (releaseEntity != null)
                        return releaseEntity;

                    // Query (Remote)
                    var releases = _musicBrainzClient.QueryReleases(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var release in releases)
                    {
                        AddUpdateMusicBrainzRelease(release);
                    }

                    // Try Again
                    releaseEntity = _musicBrainzDbClient.GetEntity<MusicBrainzReleaseEntity>(musicBrainzId.Value);

                    if (releaseEntity == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Release Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }

                    return releaseEntity;
                }

                // Search
                else
                {
                    // Query (Remote)
                    var releases = _musicBrainzClient.QueryReleases(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var release in releases)
                    {
                        AddUpdateMusicBrainzRelease(release);
                    }

                    if (releases.Any())
                    {
                        return _musicBrainzDbClient.GetEntity<MusicBrainzReleaseEntity>(releases.First().Id);
                    }

                    else
                    {
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public MusicBrainzTrackEntity GetMusicBrainzTrack(Guid? musicBrainzId, string artistName, string albumName, string trackName)
        {
            try
            {
                // Id
                if (musicBrainzId != null)
                {
                    // Check Guid (Local)
                    var trackEntity = _musicBrainzDbClient.GetEntity<MusicBrainzTrackEntity>(musicBrainzId.Value);

                    if (trackEntity != null)
                        return trackEntity;

                    // Query (Remote)
                    var track = _musicBrainzClient.GetTrack(trackName, albumName, artistName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    if (track != null)
                    {
                        AddUpdateMusicBrainzTrack(track);
                    }

                    // Try Again
                    trackEntity = _musicBrainzDbClient.GetEntity<MusicBrainzTrackEntity>(musicBrainzId.Value);

                    if (trackEntity == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Track Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }

                    return trackEntity;
                }

                // Search
                else
                {
                    // Query (Remote)
                    var track = _musicBrainzClient.GetTrack(trackName, albumName, artistName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    if (track != null)
                    {
                        AddUpdateMusicBrainzTrack(track);

                        return _musicBrainzDbClient.GetEntity<MusicBrainzTrackEntity>(track.Id);
                    }

                    else
                    {
                        ApplicationHelpers.Log("Music Brainz Track Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public IEnumerable<MusicBrainzTagEntity> GetMusicBrainzTags(MusicBrainzEntityBase relatedEntity)
        {
            try
            {
                // Query for all tags
                var tags = _musicBrainzClient.GetAllTags().Result;

                foreach (var tag in tags)
                {
                    AddUpdateMusicBrainzTag(tag, relatedEntity);
                }

                return _musicBrainzDbClient.GetEntities<MusicBrainzTagEntity>();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public IEnumerable<MusicBrainzGenreEntity> GetMusicBrainzGenres(MusicBrainzEntityBase relatedEntity)
        {
            try
            {
                // Query for all tags
                var genres = _musicBrainzClient.GetAllGenres().Result;

                foreach (var genre in genres)
                {
                    AddUpdateMusicBrainzGenre(genre, relatedEntity);
                }

                return _musicBrainzDbClient.GetEntities<MusicBrainzGenreEntity>();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public MusicBrainzLabelEntity GetMusicBrainzLabel(Guid? musicBrainzId, string artistName, string albumName, string trackName)
        {
            try
            {
                // Id
                if (musicBrainzId != null)
                {
                    // Check Guid (Local)
                    var labelEntity = _musicBrainzDbClient.GetEntity<MusicBrainzLabelEntity>(musicBrainzId.Value);

                    if (labelEntity != null)
                        return labelEntity;

                    // Query (Remote)
                    var labels = _musicBrainzClient.QueryLabel(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var label in labels)
                    {
                        AddUpdateMusicBrainzLabel(label);
                    }

                    // Try Again
                    labelEntity = _musicBrainzDbClient.GetEntity<MusicBrainzLabelEntity>(musicBrainzId.Value);

                    if (labelEntity == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Label Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }

                    return labelEntity;
                }

                // Search
                else
                {
                    // Query (Remote)
                    var labels = _musicBrainzClient.QueryLabel(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var label in labels)
                    {
                        AddUpdateMusicBrainzLabel(label);
                    }

                    if (labels.Any())
                    {
                        return _musicBrainzDbClient.GetEntity<MusicBrainzLabelEntity>(labels.First().Id);
                    }

                    else
                    {
                        ApplicationHelpers.Log("Music Brainz Label Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return null;
            }
        }
        public IEnumerable<MusicBrainzUrlEntity> GetMusicBrainzRelatedUrls<TEntity>(Guid musicBrainzEntityId, string artistName, string albumName, string trackName) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                // Check Guid (Local)
                var maps = _musicBrainzDbClient.Where<MusicBrainzUrlEntityMap>(entity => entity.MusicBrainzEntityId == musicBrainzEntityId);

                if (maps.Any())
                    return maps.Select(map => map.MusicBrainzUrl).ToList();

                // Related Entity
                var relatedEntity = _musicBrainzDbClient.GetEntity<TEntity>(musicBrainzEntityId);

                if (relatedEntity == null)
                {
                    ApplicationHelpers.Log("Music Brainz Entity Not Found:  " + musicBrainzEntityId, LogMessageType.Vendor, LogLevel.Error);
                    return Enumerable.Empty<MusicBrainzUrlEntity>();
                }

                // Query (Remote)
                var urlEntities = _musicBrainzClient.GetRelatedUrls(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                foreach (var urlEntity in urlEntities)
                {
                    AddUpdateMusicBrainzUrl(urlEntity, relatedEntity);
                }

                // Try Again
                maps = _musicBrainzDbClient.Where<MusicBrainzUrlEntityMap>(entity => entity.MusicBrainzEntityId == musicBrainzEntityId);

                if (maps.Any())
                    return maps.Select(map => map.MusicBrainzUrl).ToList();

                else
                {
                    ApplicationHelpers.Log("Error retrieving music brainz url's for:  " + musicBrainzEntityId, LogMessageType.Database, LogLevel.Error);
                    return Enumerable.Empty<MusicBrainzUrlEntity>();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return Enumerable.Empty<MusicBrainzUrlEntity>();
            }
        }
        public IEnumerable<MusicBrainzMediumEntity> GetMusicBrainzMedia(Guid musicBrainzReleaseId, string artistName, string albumName, string trackName)
        {
            try
            {
                // Check Guid (Local)
                var releaseMedia = _musicBrainzDbClient.Where<MusicBrainzMediumEntity>(x => x.MusicBrainzReleaseId == musicBrainzReleaseId);

                if (releaseMedia.Any())
                    return releaseMedia;

                // Query (Remote)
                var media = _musicBrainzClient.QueryMedia(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                foreach (var medium in media)
                {
                    AddUpdateMusicBrainzMedium(musicBrainzReleaseId, medium);
                }

                // Try Again
                releaseMedia = _musicBrainzDbClient.Where<MusicBrainzMediumEntity>(x => x.MusicBrainzReleaseId == musicBrainzReleaseId);

                if (!releaseMedia.Any())
                {
                    ApplicationHelpers.Log("Music Brainz Release Media Not Found:  " + musicBrainzReleaseId, LogMessageType.Vendor, LogLevel.Error);
                    return Enumerable.Empty<MusicBrainzMediumEntity>();
                }

                return releaseMedia;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                return Enumerable.Empty<MusicBrainzMediumEntity>();
            }
        }
        #endregion
    }
}

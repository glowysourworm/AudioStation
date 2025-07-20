using System.Globalization;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
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
        public Mp3FileReference AddUpdateLibraryEntry(string fileName, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, TagLib.File tagRef)
        {
            try
            {
                return _audioStationDbClient.AddUpdateLibraryEntry(fileName, fileAvailable, fileLoadError, fileLoadErrorMessage, tagRef);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
                return null;
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return false;
            }
        }
        #endregion

        #region Remote Queries / Local Cache
        public MusicBrainzArtistEntity AddUpdateMusicBrainzArtist(IArtist musicBrainzArtist)
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

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzRecordingEntity AddUpdateMusicBrainzRecording(IRecording musicBrainzRecording)
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

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzReleaseEntity AddUpdateMusicBrainzRelease(IRelease musicBrainzRelease)
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

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzTrackEntity AddUpdateMusicBrainzTrack(ITrack musicBrainzTrack)
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

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzTagEntity AddUpdateMusicBrainzTag(ITag musicBrainzTag, MusicBrainzEntityBase musicBrainzEntity)
        {
            try
            {
                var entity = new MusicBrainzTagEntity()
                {
                    Id = System.Guid.NewGuid(),         // Guess there wasn't an ID
                    Name = musicBrainzTag.Name,
                };

                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddTagRelationship(entity, musicBrainzEntity);

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzGenreEntity AddUpdateMusicBrainzGenre(IGenre musicBrainzGenre, MusicBrainzEntityBase musicBrainzEntity)
        {
            try
            {
                var entity = new MusicBrainzGenreEntity()
                {
                    Id = musicBrainzGenre.Id,                    
                    Name = musicBrainzGenre.Name,
                };

                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddGenreRelationship(entity, musicBrainzEntity);

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzUrlEntity AddUpdateMusicBrainzUrl(IUrl musicBrainzUrl, MusicBrainzEntityBase musicBrainzEntity)
        {
            try
            {
                var entity = new MusicBrainzUrlEntity()
                {
                    Id = musicBrainzUrl.Id,
                    Url = musicBrainzUrl.Resource?.AbsoluteUri ?? string.Empty,
                };

                if (!_musicBrainzDbClient.ContainsEntity(entity))
                    _musicBrainzDbClient.AddUrlRelationship(entity, musicBrainzEntity);

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzMediumEntity AddUpdateMusicBrainzMedium(Guid musicBrainzReleaseId, IMedium musicBrainzMedium)
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

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
            }
        }
        public MusicBrainzLabelEntity AddUpdateMusicBrainzLabel(ILabel musicBrainzLabel)
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

                return entity;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return null;
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
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error, null);
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
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error, null);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                        ApplicationHelpers.Log("Music Brainz Recording Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error, null);
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
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error, null);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                        ApplicationHelpers.Log("Music Brainz Release Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error, null);
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
                        ApplicationHelpers.Log("Music Brainz Artist Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error, null);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                        ApplicationHelpers.Log("Music Brainz Track Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error, null);
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
                        ApplicationHelpers.Log("Music Brainz Track Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error, null);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                        ApplicationHelpers.Log("Music Brainz Label Not Found:  " + musicBrainzId.Value, LogMessageType.Vendor, LogLevel.Error, null);
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
                        ApplicationHelpers.Log("Music Brainz Label Not Found:  " + artistName, LogMessageType.Vendor, LogLevel.Error, null);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                    ApplicationHelpers.Log("Music Brainz Entity Not Found:  " + musicBrainzEntityId, LogMessageType.Vendor, LogLevel.Error, null);
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
                    ApplicationHelpers.Log("Error retrieving music brainz url's for:  " + musicBrainzEntityId, LogMessageType.Database, LogLevel.Error, null);
                    return Enumerable.Empty<MusicBrainzUrlEntity>();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                    ApplicationHelpers.Log("Music Brainz Release Media Not Found:  " + musicBrainzReleaseId, LogMessageType.Vendor, LogLevel.Error, null);
                    return Enumerable.Empty<MusicBrainzMediumEntity>();
                }

                return releaseMedia;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return Enumerable.Empty<MusicBrainzMediumEntity>();
            }
        }


        public IEnumerable<MusicBrainzCombinedLibraryEntryRecord> GetCompleteMusicBrainzRecord(string artistName, string albumName, string trackName)
        {
            try
            {
                var resultSet = new List<MusicBrainzCombinedLibraryEntryRecord>();

                // Music Brainz Caching:
                //
                // 1) Get all artists (with min score) with matching names
                // 2) Take all subsequent data (optimizing for performance based on local stores) (we may already have these by key)
                // 3) Use album and track name to complete records. So, the result should be a cache for ONE TRACK; but with
                //    all MB data pieced together.
                //

                // NOTE:  THERE SHOULD BE JUST ONE RESULT THAT MATTERS! We're taking the top (N) results and caching them
                //        
                var matchingReleases = _musicBrainzClient.QueryReleases(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE)
                                                         .Result
                                                         .Where(x => StringHelpers.CompareIC(x.Title, albumName))
                                                         .Where(x => x.ArtistCredit != null)
                                                         .ToList();

                // Must query releases separately for media data
                //
                var matchingReleaseData = matchingReleases.Select(x => _musicBrainzClient.GetReleaseById(x.Id).Result)
                                                          .Where(x => x.Media != null)
                                                          .Where(x => x.Media.Any(media => media.Tracks != null && 
                                                                                           media.Tracks.Any(track => StringHelpers.CompareIC(track.Title, trackName))))
                                                          .ToList();

                foreach (var matchingRelease in matchingReleaseData)
                {
                    var result = new MusicBrainzCombinedLibraryEntryRecord();

                    
                    // Track:  Guaranteed by the above query (matches track name)
                    var matchingTrack = matchingRelease.Media
                                                       .SelectMany(media => media.Tracks)
                                                       .First(x => StringHelpers.CompareIC(x.Title, trackName));

                    // Recording:  Get by ID matching our results
                    var matchingRecording = _musicBrainzClient.GetRecordingById(matchingTrack.Recording.Id).Result;

                    // Medium:  Also guaranteed by above query
                    var matchingMedium = matchingRelease.Media.First(x => x.Tracks.Any(track => StringHelpers.CompareIC(track.Title, trackName)));

                    // *** Start Matching to our Local Database ***

                    // Recording
                    var existingRecording = _musicBrainzDbClient.GetEntity<MusicBrainzRecordingEntity>(matchingRecording.Id);

                    if (existingRecording == null)
                    {
                        existingRecording = new MusicBrainzRecordingEntity()
                        {
                            Id = matchingRecording.Id,
                            Annotation = matchingRecording.Annotation,
                            Disambiguation = matchingRecording.Disambiguation,
                            Length = matchingRecording.Length,
                            Title = matchingRecording.Title
                        };

                        // Local Recording
                        _musicBrainzDbClient.AddEntity(existingRecording);

                        // Tags / Genres
                        if (matchingRecording.Tags != null)
                        {
                            foreach (var tag in matchingRecording.Tags.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                            {
                                var tagEntity = AddUpdateMusicBrainzTag(tag, existingRecording);

                                // Result Set
                                if (tagEntity != null)
                                    result.RecordingTags.Add(tagEntity);
                            }
                        }

                        if (matchingRecording.Genres != null)
                        {
                            foreach (var genre in matchingRecording.Genres.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                            {
                                var genreEntity = AddUpdateMusicBrainzGenre(genre, existingRecording);

                                // Result Set
                                if (genreEntity != null)
                                    result.RecordingGenres.Add(genreEntity);
                            }
                        }
                    }

                    // Result Set
                    result.Recording = existingRecording;

                    // Release
                    var existingRelease = _musicBrainzDbClient.GetEntity<MusicBrainzReleaseEntity>(matchingRelease.Id);

                    if (existingRelease == null)
                    {
                        // Media (null)
                        existingRelease = new MusicBrainzReleaseEntity()
                        {
                            Annotation = matchingRelease.Annotation,
                            Asin = matchingRelease.Asin,
                            Barcode = matchingRelease.Barcode,
                            Country = matchingRelease.Country,
                            Date = matchingRelease.Date?.NearestDate.ToUniversalTime() ?? DateTime.MinValue.ToUniversalTime(),
                            Disambiguation = matchingRelease.Disambiguation,
                            Id = matchingRelease.Id,
                            Packaging = matchingRelease.Packaging,
                            Quality = matchingRelease.Quality,
                            Status = matchingRelease.Status,
                            Title = matchingRelease.Title
                        };


                        _musicBrainzDbClient.AddEntity(existingRelease);

                        // Tags / Genres
                        if (matchingRelease.Tags != null)
                        {
                            foreach (var tag in matchingRelease.Tags.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                            {
                                AddUpdateMusicBrainzTag(tag, existingRelease);
                            }
                        }

                        if (matchingRelease.Genres != null)
                        {
                            foreach (var genre in matchingRelease.Genres.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                            {
                                AddUpdateMusicBrainzGenre(genre, existingRelease);
                            }
                        }
                    }

                    // Result Set
                    result.Release = existingRelease;

                    // Artist(s)
                    foreach (var matchingArtist in matchingRelease.ArtistCredit.Select(x => x.Artist))
                    {
                        // Fill out our records
                        //
                        var existingArtist = _musicBrainzDbClient.GetEntity<MusicBrainzArtistEntity>(matchingArtist.Id);

                        // Artist
                        if (existingArtist == null)
                        {
                            existingArtist = new MusicBrainzArtistEntity()
                            {
                                Id = matchingArtist.Id,
                                Annotation = matchingArtist.Annotation,
                                Country = matchingArtist.Country,
                                Disambiguation = matchingArtist.Disambiguation,
                                Name = matchingArtist.Name,
                                SortName = matchingArtist.SortName
                            };

                            _musicBrainzDbClient.AddEntity<MusicBrainzArtistEntity>(existingArtist);

                            // Result Set
                            result.Artists.Add(existingArtist);
                        }

                        var existingArtistRecordingMap = _musicBrainzDbClient.Where<MusicBrainzArtistRecordingMap>(x => x.MusicBrainzArtistId == existingArtist.Id &&
                                                                                                                        x.MusicBrainzRecordingId == existingRecording.Id)
                                                                             .FirstOrDefault();

                        var existingArtistReleaseMap = _musicBrainzDbClient.Where<MusicBrainzArtistReleaseMap>(x => x.MusicBrainzArtistId == existingArtist.Id &&
                                                                                                                    x.MusicBrainzReleaseId == existingRelease.Id)
                                                                           .FirstOrDefault();

                        // Artist Map Entities (Recording / Release)
                        //
                        if (existingArtistRecordingMap == null)
                        {
                            existingArtistRecordingMap = new MusicBrainzArtistRecordingMap()
                            {
                                MusicBrainzArtistId = matchingArtist.Id,
                                MusicBrainzRecordingId = matchingRecording.Id,
                            };

                            _musicBrainzDbClient.AddEntity(existingArtistRecordingMap);
                        }
                        if (existingArtistReleaseMap == null)
                        {
                            existingArtistReleaseMap = new MusicBrainzArtistReleaseMap()
                            {
                                MusicBrainzArtistId = matchingArtist.Id,
                                MusicBrainzReleaseId = matchingRelease.Id,
                            };

                            _musicBrainzDbClient.AddEntity(existingArtistReleaseMap);
                        }

                        // Result Set
                        result.Artists.Add(existingArtist);
                    }

                    // Media

                    // NOTE: (I've removed the Disc entity from the local database)
                    //
                    // 1) 1 Medium = 1 Disc (or) Vinyl (we won't know here which format it is for OUR recording)
                    //    So, just keep these together. We may have a recording that doesn't match the meta-data
                    //    but we wouldn't have (necessarily) known anyway.
                    // 
                    // 2) There's no ID for a MusicBrainz IMedium. But, there is for a release. So, we'll assign
                    //    a new ID for our local medium record - which is immediately mapped to a local release.
                    //

                    // Try matching on the rest of the fields
                    var existingMedium = _musicBrainzDbClient.Where<MusicBrainzMediumEntity>(x => StringHelpers.CompareIC(x.Title, matchingMedium.Title) &&
                                                                                                  x.Format == matchingMedium.Format &&
                                                                                                  x.Position == matchingMedium.Position &&
                                                                                                  x.TrackCount == matchingMedium.TrackCount &&
                                                                                                  x.TrackOffset == matchingMedium.TrackOffset)
                                                             .FirstOrDefault();

                    if (existingMedium == null)
                    {
                        existingMedium = new MusicBrainzMediumEntity()
                        {
                            Format = matchingMedium.Format,
                            Id = Guid.NewGuid(),
                            MusicBrainzReleaseId = existingRelease.Id,
                            Position = matchingMedium.Position,
                            Title = matchingMedium.Title,
                            TrackCount = matchingMedium.TrackCount,
                            TrackOffset = matchingMedium.TrackOffset
                        };

                        _musicBrainzDbClient.AddEntity<MusicBrainzMediumEntity>(existingMedium);
                    }

                    // Go ahead and simulate loading of the media here
                    if (!existingRelease.Media.Any(x => StringHelpers.CompareIC(x.Title, matchingMedium.Title) &&
                                                        x.Format == matchingMedium.Format &&
                                                        x.Position == matchingMedium.Position &&
                                                        x.TrackCount == matchingMedium.TrackCount &&
                                                        x.TrackOffset == matchingMedium.TrackOffset))
                        existingRelease.Media.Add(existingMedium);

                    // Result Set
                    result.Medium = existingMedium;


                    // Track
                    var existingTrack = _musicBrainzDbClient.GetEntity<MusicBrainzTrackEntity>(matchingTrack.Id);

                    if (existingTrack == null)
                    {
                        existingTrack = new MusicBrainzTrackEntity()
                        {
                            Id = matchingTrack.Id,
                            Length = matchingTrack.Length,
                            MusicBrainzMediumId = existingMedium.Id,
                            MusicBrainzRecordingId = existingRecording.Id,
                            Number = matchingTrack.Number,
                            Position = matchingTrack.Position,
                            Title = matchingTrack.Title,
                        };

                        _musicBrainzDbClient.AddEntity(existingTrack);
                    }

                    // Result Set
                    result.Track = existingTrack;

                    // Label
                    if (matchingRelease.LabelInfo != null)
                    {
                        foreach (var labelInfo in matchingRelease.LabelInfo.Where(x => x.Label != null))
                        {
                            var existingLabel = _musicBrainzDbClient.GetEntity<MusicBrainzLabelEntity>(labelInfo.Label.Id);

                            if (existingLabel == null)
                            {
                                existingLabel = new MusicBrainzLabelEntity()
                                {
                                    Annotation = labelInfo.Label.Annotation,
                                    Country = labelInfo.Label.Country,
                                    Disambiguation = labelInfo.Label.Disambiguation,
                                    Id = labelInfo.Label.Id,
                                    LabelCode = labelInfo.Label.LabelCode,
                                    Name = labelInfo.Label.Name,
                                    SortName = labelInfo.Label.SortName,
                                    Type = labelInfo.Label.Type
                                };

                                _musicBrainzDbClient.AddEntity(existingLabel);
                            }

                            var existingLabelReleaseMap = _musicBrainzDbClient.Where<MusicBrainzLabelReleaseMap>(x => x.MusicBrainzLabelId == existingLabel.Id &&
                                                                                                                      x.MusicBrainzReleaseId == existingRelease.Id)
                                                                              .FirstOrDefault();

                            // Label Release Map
                            if (existingLabelReleaseMap == null)
                            {
                                existingLabelReleaseMap = new MusicBrainzLabelReleaseMap()
                                {
                                    MusicBrainzLabelId = existingLabel.Id,
                                    MusicBrainzReleaseId = existingRelease.Id,
                                };

                                _musicBrainzDbClient.AddEntity(existingLabelReleaseMap);
                            }

                            // Result Set
                            result.Labels.Add(existingLabel);
                        }
                    }
                    /*
                    // These would be related to each track. (May need a separate query to get the artist URL's)
                    var relatedUrls = _musicBrainzClient.GetRelatedUrls(artistName, albumName, trackName, MUSIC_BRAINZ_MIN_SCORE).Result;

                    foreach (var relatedUrl in relatedUrls)
                    {
                        switch (relatedUrl.EntityType)
                        {
                            case EntityType.Artist:
                                
                                // These should now be available (locally)
                                foreach (var matchingArtist in matchingRelease.ArtistCredit.Select(x => x.Artist))
                                {
                                    var existingArtist = _musicBrainzDbClient.GetEntity<MusicBrainzArtistEntity>(matchingArtist.Id);

                                    if (existingArtist != null)
                                    {
                                        AddUpdateMusicBrainzUrl(relatedUrl, existingArtist);
                                    }
                                    else
                                        throw new Exception("Error retrieving / saving artist data:  ModelController.cs");
                                }
                                break;
                            case EntityType.Label:
                            {
                                if (matchingRelease.LabelInfo == null)
                                    continue;

                                foreach (var label in matchingRelease.LabelInfo.Where(x => x.Label != null))
                                {
                                    var existingLabel = _musicBrainzDbClient.GetEntity<MusicBrainzLabelEntity>(label.Label.Id);

                                    if (existingLabel != null)
                                    {
                                        AddUpdateMusicBrainzUrl(relatedUrl, existingLabel);
                                    }
                                    else
                                        throw new Exception("Error retrieving / saving label data:  ModelController.cs");
                                }
                            }
                            break;
                            case EntityType.Recording:
                                AddUpdateMusicBrainzUrl(relatedUrl, existingRecording);
                                break;
                            case EntityType.Release:
                                AddUpdateMusicBrainzUrl(relatedUrl, existingRelease);
                                break;

                            // Go ahead and skip unknown related URL's (until we need them, or find out the meaning of the URL)
                            default:
                                ApplicationHelpers.Log("Skipped Music Brainz Url Relationship:  " + relatedUrl.ToString(), LogMessageType.Database, LogLevel.Warning);
                                break;
                        }
                    }
                    */
                    // Release Picture Set
                    result.ReleasePictures.AddRange(_musicBrainzClient.GetCoverArt(existingRelease.Id).Result);

                    // Result Set
                    resultSet.Add(result);
                }

                return resultSet;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                return Enumerable.Empty<MusicBrainzCombinedLibraryEntryRecord>();
            }
        }
        #endregion
    }
}

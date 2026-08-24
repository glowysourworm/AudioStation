using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [IocExport(typeof(IAudioStationDbClient))]
    public class AudioStationDbClient : IAudioStationDbClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IIocEventAggregator _eventAggregator;

        LogLevel _currentLogLevel;
        bool _currentLogVerbosity;

        // IAudioStationService
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;
        private string _statusMessage;

        [IocImportingConstructor]
        public AudioStationDbClient(IConfigurationManager configurationManager,
                                    IIocEventAggregator eventAggregator)
        {
            _configurationManager = configurationManager;
            _eventAggregator = eventAggregator;
            _currentLogLevel = LogLevel.Trace;
            _currentLogVerbosity = true;

            _status = IAudioStationService.Status.Disabled;
            _statusMessage = "Not Initialized";

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

        public Track AddUpdateLibraryEntry(string fileName, DateTime creationDate, DateTime modifiedDate, int crc32, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, IAudioStationTag tagRef)
        {
            try
            {
                using (var context = CreateContext())
                {
                    Track entity = null;
                    var fileReference = context.FileReferences.FirstOrDefault(x => x.FileName == fileName);
                    var newEntity = false;

                    // New
                    if (fileReference == null)
                    {
                        fileReference = new FileReference()
                        {
                            FileName = fileName,
                            Created = creationDate,
                            LastModified = modifiedDate,
                            CRC32 = crc32,
                            FileCorruptMessage = string.Empty,
                            FileErrorMessage = fileLoadErrorMessage,
                            IsFileCorrupt = !fileAvailable || fileLoadError,
                            IsFileAvailable = fileAvailable,
                            IsFileLoadError = fileLoadError,
                        };
                        entity = new Track()
                        {
                            FileReference = fileReference,
                            Title = tagRef.Title?.Trim() ?? string.Empty,
                            Number = (int)tagRef.Track,
                            DurationMilliseconds = (int)tagRef.Duration.TotalMilliseconds
                        };
                        newEntity = true;
                    }

                    // Update (file from OS)
                    else
                    {
                        fileReference.Created = creationDate;
                        fileReference.LastModified = modifiedDate;
                        fileReference.CRC32 = crc32;
                        fileReference.FileCorruptMessage = string.Empty;
                        fileReference.FileErrorMessage = fileLoadErrorMessage;
                        fileReference.IsFileCorrupt = !fileAvailable || fileLoadError;
                        fileReference.IsFileAvailable = fileAvailable;
                        fileReference.IsFileLoadError = fileLoadError;
                    }

                    // There could be Null / Empty / or Unknown data. Assume there is.
                    var existingAlbum = tagRef.Album == null ? null : context.Albums.FirstOrDefault(x => x.Name == tagRef.AlbumArtist.Trim());
                    var existingArtist = tagRef.AlbumArtist == null ? null : context.Artists.FirstOrDefault(x => x.Name == tagRef.AlbumArtist.Trim());
                    var existingGenre = tagRef.Genre == null ? null : context.Genres.FirstOrDefault(x => x.Name == tagRef.Genre.Trim());

                    // Just check for null or white space
                    if (existingAlbum == null && !string.IsNullOrWhiteSpace(tagRef.Album))
                    {
                        existingAlbum = new Album()
                        {
                            DiscCount = (int)tagRef.DiscTotal,
                            DiscNumber = (int)tagRef.DiscNumber,
                            Year = (int)tagRef.Year,
                            Name = tagRef.Album.Trim()
                        };

                        context.Albums.Add(existingAlbum);
                    }
                    if (existingArtist == null && !string.IsNullOrWhiteSpace(tagRef.AlbumArtist))
                    {
                        existingArtist = new Artist()
                        {
                            Name = tagRef.AlbumArtist.Trim()
                        };

                        context.Artists.Add(existingArtist);
                    }
                    if (existingGenre == null && !string.IsNullOrWhiteSpace(tagRef.Genre))
                    {
                        existingGenre = new Genre()
                        {
                            Name = tagRef.Genre.Trim()
                        };

                        context.Genres.Add(existingGenre);
                    }

                    entity.PrimaryArtist = existingArtist;
                    entity.Album = existingAlbum;
                    entity.PrimaryGenre = existingGenre;

                    if (newEntity)
                    {
                        context.Add(fileReference);
                        context.Add(entity);
                    }

                    else
                    {
                        context.Update(fileReference);
                        context.Update(entity);
                    }


                    context.SaveChanges();

                    // Add Maps
                    var lastEntity = context.Tracks.First(x => x.Id == entity.Id);

                    // Artist Map(s)
                    foreach (var artist in tagRef.AlbumArtists.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
                    {
                        var artistEntity = context.Artists
                                                  .FirstOrDefault(x => x.Name == artist);

                        var map = context.TrackArtistMaps
                                         .FirstOrDefault(x => x.Artist.Name == artist && x.Id == lastEntity.Id);

                        // New Genre
                        if (artistEntity == null)
                        {
                            artistEntity = new Artist()
                            {
                                Name = artist
                            };
                            context.Artists.Add(artistEntity);
                        }

                        // New Map
                        if (map == null)
                        {
                            map = new TrackArtistMap()
                            {
                                Track = lastEntity,
                                Artist = artistEntity,
                                IsPrimaryArtist = (existingArtist != null) && (artist == existingArtist.Name)
                            };
                            context.TrackArtistMaps.Add(map);
                        }
                    }

                    // Genre Map(s)
                    foreach (var genre in tagRef.Genres.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
                    {
                        var genreEntity = context.Genres
                                                 .FirstOrDefault(x => x.Name == genre);

                        var map = context.TrackGenreMaps
                                         .FirstOrDefault(x => x.Genre.Name == genre && x.Id == lastEntity.Id);

                        // New Genre
                        if (genreEntity == null)
                        {
                            genreEntity = new Genre()
                            {
                                Name = genre
                            };
                            context.Genres.Add(genreEntity);
                        }

                        // New Map
                        if (map == null)
                        {
                            map = new TrackGenreMap()
                            {
                                Track = lastEntity,
                                Genre = genreEntity,
                                IsPrimaryGenre = (existingGenre != null) && (existingGenre.Name == genre)
                            };
                            context.TrackGenreMaps.Add(map);
                        }
                    }

                    context.SaveChanges();

                    return entity;
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageDbType.AudioStation, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public void AddUpdateRadioEntry(Core.Model.M3U.M3UStream entry)
        {
            if (string.IsNullOrEmpty(entry.StreamSource) ||
                string.IsNullOrEmpty(entry.Title))
                throw new ArgumentException("M3UStream must have a stream source and a title");

            try
            {
                using (var context = CreateContext())
                {
                    var newEntry = false;
                    var mediaEntity = context.M3UStreams
                                             .Where(x => x.Name == entry.Title)
                                             .FirstOrDefault();

                    if (mediaEntity == null)
                    {
                        mediaEntity = new M3UStream();
                        newEntry = true;
                    }

                    mediaEntity.Duration = entry.DurationSeconds;
                    mediaEntity.GroupName = entry.GroupName;
                    mediaEntity.HomepageUrl = entry.TvgHomepage;
                    mediaEntity.LogoUrl = entry.TvgLogo;
                    mediaEntity.Name = entry.Title;
                    mediaEntity.StreamSourceUrl = entry.StreamSource;
                    mediaEntity.UserExcluded = false || mediaEntity.UserExcluded;

                    if (newEntry)
                        context.M3UStreams.Add(mediaEntity);

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageDbType.AudioStation, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public void AddRadioEntries(IEnumerable<Core.Model.M3U.M3UStream> entries)
        {
            // Batch Add:  Assume there are no conflicting records. There may be
            //             batches that get thrown out; but the database index
            //             won't be of use for a large table like this one.

            try
            {
                using (var context = CreateContext())
                {
                    // We may need to get rid of the DTO and just use the EF model to help
                    // save time.

                    foreach (var entry in entries)
                    {
                        // Index: [Name]
                        var entity = context.M3UStreams
                                            .Where(x => x.Name == entry.Title)
                                            .FirstOrDefault();

                        if (entity == null)
                        {
                            context.M3UStreams.Add(new M3UStream()
                            {
                                Duration = entry.DurationSeconds,
                                GroupName = entry.GroupName,
                                HomepageUrl = entry.TvgHomepage,
                                LogoUrl = entry.TvgLogo,
                                Name = entry.Title,
                                StreamSourceUrl = entry.StreamSource
                            });
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageDbType.AudioStation, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public IEnumerable<Track> GetArtistFiles(int artistId)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Tracks
                                  .Where(x => x.PrimaryArtistId == artistId)
                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageDbType.AudioStation, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public IEnumerable<Album> GetArtistAlbums(int artistId, bool isPrimaryArtist)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.TrackArtistMaps
                                      .Where(x => x.ArtistId == artistId &&
                                                  isPrimaryArtist == x.IsPrimaryArtist &&
                                                   x.Track.Album != null)
                                      .Select(x => x.Track.Album)
                                      .Distinct()
                                      .ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageDbType.AudioStation, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public IEnumerable<Track> GetAlbumTracks(int albumId)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Tracks
                                  .Where(x => x.AlbumId == albumId)
                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageDbType.AudioStation, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : AudioStationEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    IEnumerable<TEntity> collection = context.Set<TEntity>();
                    long totalRecords = collection.Count();
                    long totalFilteredRecords = 0;

                    if (request.WhereCallback != null)
                        totalFilteredRecords = context.Set<TEntity>().AsEnumerable().Where(x => request.WhereCallback(x as TEntity)).Count();
                    else
                        totalFilteredRecords = totalRecords;

                    // Order By
                    if (request.OrderByCallback != null)
                    {
                        collection = collection.OrderBy(x => request.OrderByCallback(x));
                    }

                    // Where
                    if (request.WhereCallback != null)
                    {
                        collection = collection.Where(x => request.WhereCallback(x));
                    }

                    // Finish Linq Statements (PageStart is a non-index integer)
                    collection = collection.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);

                    return new PageResult<TEntity>()
                    {
                        Results = collection.ToList(),
                        TotalRecordCount = (int)totalRecords,
                        TotalRecordCountFiltered = (int)totalFilteredRecords,
                        PageCount = (int)Math.Ceiling(totalRecords / (double)request.PageSize),
                        PageNumber = request.PageNumber,
                        PageSize = Math.Min(request.PageSize, collection.Count())
                    };
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageDbType.AudioStation, LogLevel.Error, ex);
                throw ex;
            }
        }

        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : AudioStationEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return GetEntitySet<TEntity>(context).ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageDbType.AudioStation, LogLevel.Error, ex);
                throw ex;
            }
        }

        public TEntity? GetEntity<TEntity>(int id) where TEntity : AudioStationEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Find<TEntity>(id);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageDbType.AudioStation, LogLevel.Error, ex);
                throw ex;
            }
        }

        public bool AddEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    context.Add<TEntity>(entity);
                    context.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageDbType.AudioStation, LogLevel.Error, ex);
                throw ex;
            }
        }

        public TEntity? FindEntity<TEntity, TProperty>(TProperty property, Func<TEntity, TProperty> selector) where TEntity : AudioStationEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Set<TEntity>().FirstOrDefault(x => selector(x).Equals(property));
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error finding entity data:  " + ex.Message, LogMessageDbType.AudioStation, LogLevel.Error, ex);
                throw ex;
            }
        }

        public bool UpdateEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    context.Update<TEntity>(entity);
                    context.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageDbType.AudioStation, LogLevel.Error, ex);
                throw ex;
            }
        }

        // The Set<> method has postgres / EF / npgsql issues. Probably related to configuration; but I'm running out of
        // options.
        //
        private DbSet<TEntity> GetEntitySet<TEntity>(AudioStationDbContext context) where TEntity : AudioStationEntityBase
        {
            if (typeof(TEntity) == typeof(M3UStream))
                return context.M3UStreams as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Track))
                return context.Tracks as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Album))
                return context.Albums as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Artist))
                return context.Artists as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(TrackArtistMap))
                return context.TrackArtistMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Genre))
                return context.Genres as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(TrackGenreMap))
                return context.TrackGenreMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(RadioBrowserStation))
                return context.RadioBrowserStations as DbSet<TEntity>;

            else
                throw new Exception("Unhandled entity type:  AudioStationDbClient.GetEntitySet");

        }

        private AudioStationDbContext CreateContext()
        {
            var configuration = _configurationManager.GetConfiguration();

            var context = new AudioStationDbContext(configuration, _currentLogLevel, _currentLogVerbosity);

            return context;
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Audio Station Database";
        }
        public string GetDisplayName()
        {
            return "Audio Station Database";
        }
        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        public async Task<IAudioStationService.Status> Initialize()
        {
            var configuration = _configurationManager.GetConfiguration();

            if (string.IsNullOrWhiteSpace(configuration.DatabaseHost))
                OnStatusChanged(IAudioStationService.Status.Error, "database host not specified");

            else if (string.IsNullOrWhiteSpace(configuration.DatabaseName))
                OnStatusChanged(IAudioStationService.Status.Error, "database name not specified");

            else if (string.IsNullOrWhiteSpace(configuration.DatabaseUser))
                OnStatusChanged(IAudioStationService.Status.Error, "database user not specified");

            else if (string.IsNullOrWhiteSpace(configuration.DatabasePassword))
                OnStatusChanged(IAudioStationService.Status.Error, "database password not specified");

            else
                OnStatusChanged(IAudioStationService.Status.Idle, "database configuration OK!");

            // Test Connection
            try
            {
                using (var context = CreateContext())
                {
                    // No-op
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged(IAudioStationService.Status.Error, "database connection failed!");
                //ApplicationHelpers.Log("Database connection failed!", LogMessageType.)
            }

            return _status;
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + ": " + _statusMessage;
        }

        private void OnStatusChanged(IAudioStationService.Status status, string message)
        {
            _status = status;
            _statusMessage = message;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion

        public void Dispose()
        {
            // TODO
        }
    }
}

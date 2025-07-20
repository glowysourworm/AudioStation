using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
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

        [IocImportingConstructor]
        public AudioStationDbClient(IConfigurationManager configurationManager,
                                    IIocEventAggregator eventAggregator)
        {
            _configurationManager = configurationManager;
            _eventAggregator = eventAggregator;
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

        public Mp3FileReference AddUpdateLibraryEntry(string fileName, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, TagLib.File tagRef)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var entity = context.Mp3FileReferences.FirstOrDefault(x => x.FileName == fileName);
                    var newEntity = false;

                    if (entity == null)
                    {
                        entity = new Mp3FileReference()
                        {
                            FileName = fileName,
                            Title = tagRef.Tag.Title?.Trim() ?? string.Empty,
                            Track = (int)tagRef.Tag.Track,
                            DurationMilliseconds = (int)tagRef.Properties.Duration.TotalMilliseconds,
                            FileCorruptMessage = tagRef.CorruptionReasons?.Join(",", x => x) ?? string.Empty,
                            FileErrorMessage = fileLoadErrorMessage,
                            IsFileCorrupt = tagRef.PossiblyCorrupt,
                            IsFileAvailable = fileAvailable,
                            IsFileLoadError = fileLoadError,
                            MusicBrainzTrackId = tagRef.Tag.MusicBrainzTrackId ?? string.Empty
                        };
                        newEntity = true;
                    }

                    // There could be Null / Empty / or Unknown data. Assume there is.
                    var existingAlbum = tagRef.Tag.Album == null ? null : context.Mp3FileReferenceAlbums.FirstOrDefault(x => x.Name == tagRef.Tag.Album.Trim());
                    var existingArtist = tagRef.Tag.FirstAlbumArtist == null ? null : context.Mp3FileReferenceArtists.FirstOrDefault(x => x.Name == tagRef.Tag.FirstAlbumArtist.Trim());
                    var existingGenre = tagRef.Tag.FirstGenre == null ? null : context.Mp3FileReferenceGenres.FirstOrDefault(x => x.Name == tagRef.Tag.FirstGenre.Trim());

                    // Just check for null or white space
                    if (existingAlbum == null && !string.IsNullOrWhiteSpace(tagRef.Tag.Album))
                    {
                        existingAlbum = new Mp3FileReferenceAlbum()
                        {
                            DiscCount = (int)tagRef.Tag.DiscCount,
                            DiscNumber = (int)tagRef.Tag.Disc,
                            Year = (int)tagRef.Tag.Year,
                            Name = tagRef.Tag.Album.Trim(),
                            MusicBrainzReleaseId = tagRef.Tag.MusicBrainzReleaseId ?? string.Empty
                        };

                        context.Mp3FileReferenceAlbums.Add(existingAlbum);
                    }
                    if (existingArtist == null && !string.IsNullOrWhiteSpace(tagRef.Tag.FirstAlbumArtist))
                    {
                        existingArtist = new Mp3FileReferenceArtist()
                        {
                            Name = tagRef.Tag.FirstAlbumArtist.Trim(),
                            MusicBrainzArtistId = tagRef.Tag.MusicBrainzArtistId ?? string.Empty
                        };

                        context.Mp3FileReferenceArtists.Add(existingArtist);
                    }
                    if (existingGenre == null && !string.IsNullOrWhiteSpace(tagRef.Tag.FirstGenre))
                    {
                        existingGenre = new Mp3FileReferenceGenre()
                        {
                            Name = tagRef.Tag.FirstGenre.Trim()
                        };

                        context.Mp3FileReferenceGenres.Add(existingGenre);
                    }

                    entity.PrimaryArtist = existingArtist;
                    entity.Album = existingAlbum;
                    entity.PrimaryGenre = existingGenre;

                    if (newEntity)
                        context.Add(entity);
                    else
                        context.Update(entity);

                    context.SaveChanges();

                    // Add Maps
                    var lastEntity = context.Mp3FileReferences.First(x => x.FileName == fileName);

                    // Artist Map(s)
                    foreach (var artist in tagRef.Tag.AlbumArtists.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
                    {
                        var artistEntity = context.Mp3FileReferenceArtists
                                                  .FirstOrDefault(x => x.Name == artist);

                        var map = context.Mp3FileReferenceArtistMaps
                                         .FirstOrDefault(x => x.Mp3FileReferenceArtist.Name == artist && x.Mp3FileReferenceId == lastEntity.Id);

                        // New Genre
                        if (artistEntity == null)
                        {
                            artistEntity = new Mp3FileReferenceArtist()
                            {
                                Name = artist
                            };
                            context.Mp3FileReferenceArtists.Add(artistEntity);
                        }

                        // New Map
                        if (map == null)
                        {
                            map = new Mp3FileReferenceArtistMap()
                            {
                                Mp3FileReference = lastEntity,
                                Mp3FileReferenceArtist = artistEntity,
                                IsPrimaryArtist = (existingArtist != null) && (artist == existingArtist.Name)
                            };
                            context.Mp3FileReferenceArtistMaps.Add(map);
                        }
                    }

                    // Genre Map(s)
                    foreach (var genre in tagRef.Tag.Genres.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
                    {
                        var genreEntity = context.Mp3FileReferenceGenres
                                                 .FirstOrDefault(x => x.Name == genre);

                        var map = context.Mp3FileReferenceGenreMaps
                                         .FirstOrDefault(x => x.Mp3FileReferenceGenre.Name == genre && x.Mp3FileReferenceId == lastEntity.Id);

                        // New Genre
                        if (genreEntity == null)
                        {
                            genreEntity = new Mp3FileReferenceGenre()
                            {
                                Name = genre
                            };
                            context.Mp3FileReferenceGenres.Add(genreEntity);
                        }

                        // New Map
                        if (map == null)
                        {
                            map = new Mp3FileReferenceGenreMap()
                            {
                                Mp3FileReference = lastEntity,
                                Mp3FileReferenceGenre = genreEntity,
                                IsPrimaryGenre = (existingGenre != null) && (existingGenre.Name == genre)
                            };
                            context.Mp3FileReferenceGenreMaps.Add(map);
                        }
                    }

                    context.SaveChanges();

                    return entity;
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
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
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public IEnumerable<Mp3FileReference> GetArtistFiles(int artistId)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Mp3FileReferences
                                  .Where(x => x.PrimaryArtistId == artistId)
                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public IEnumerable<Mp3FileReferenceAlbum> GetArtistAlbums(int artistId, bool isPrimaryArtist)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Mp3FileReferenceArtistMaps
                                      .Where(x => x.Mp3FileReferenceArtistId == artistId &&
                                                     isPrimaryArtist == x.IsPrimaryArtist &&
                                                   x.Mp3FileReference.Album != null)
                                      .Select(x => x.Mp3FileReference.Album)
                                      .Distinct()
                                      .ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public IEnumerable<Mp3FileReference> GetAlbumTracks(int albumId)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Mp3FileReferences
                                  .Where(x => x.AlbumId == albumId)
                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error,    ex, ex.Message);
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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

            else if (typeof(TEntity) == typeof(Mp3FileReference))
                return context.Mp3FileReferences as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Mp3FileReferenceAlbum))
                return context.Mp3FileReferenceAlbums as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Mp3FileReferenceArtist))
                return context.Mp3FileReferenceArtists as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Mp3FileReferenceArtistMap))
                return context.Mp3FileReferenceArtistMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Mp3FileReferenceGenre))
                return context.Mp3FileReferenceGenres as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Mp3FileReferenceGenreMap))
                return context.Mp3FileReferenceGenreMaps as DbSet<TEntity>;

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

        public void Dispose()
        {
            // TODO
        }
    }
}

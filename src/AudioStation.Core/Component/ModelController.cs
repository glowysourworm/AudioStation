using System.Data;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IModelController))]
    public class ModelController : IModelController
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;
        private readonly IIocEventAggregator _eventAggregator;

        LogLevel _currentLogLevel;
        bool _currentLogVerbosity;

        [IocImportingConstructor]
        public ModelController(IConfigurationManager configurationManager,
                               IOutputController outputController,
                               IIocEventAggregator eventAggregator)
        {
            _configurationManager = configurationManager;
            _outputController = outputController;
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

        public void Initialize()
        {
            /*
            // Procedure: The Library and Radio components are loaded from
            //            any database data that exists. At runtime, the rest
            //            of the data may be queried from this controller.
            //
            this.Library = new Library();
            this.Radio = new Radio();

            try
            {
                var fileRefsDict = new Dictionary<int, Mp3FileReference>();
                var albumRefsDict = new Dictionary<int, Mp3FileReferenceAlbum>();
                var artistRefsDict = new Dictionary<int, Mp3FileReferenceArtist>();
                //var genreMapRefsDict = new Dictionary<int, Mp3FileReferenceGenreMap>();

                Dictionary<int, Mp3FileReferenceGenre> primaryGenreDict;
                Dictionary<int, Mp3FileReferenceGenre> genreDict;

                // Npgsql / EF:  Frontloading database entities. The NpgsqlCommand (tried) didn't save
                //               much time on loading. I was expecting a lot of extra lag from EF. So,
                //               the library seems tuned well to "normal MSFT guys". :)
                //
                using (var context = CreateContext())
                {
                    // Load data from the database that will be used to group / view entries. The
                    // rest of the data will be loaded lazily. The mp3 tag data will be called once
                    // the view loads the Album, Artist, or Entry (views). So, don't discard the 
                    // data. Flag the Library (local entites) that they have been read / not-read.

                    fileRefsDict = context.Mp3FileReferences.ToDictionary(x => x.Id);
                    albumRefsDict = context.Mp3FileReferenceAlbums.ToDictionary(x => x.Id);
                    artistRefsDict = context.Mp3FileReferenceArtists.ToDictionary(x => x.Id);

                    primaryGenreDict = context.Mp3FileReferenceGenreMaps
                                           .ToList()
                                           .GroupBy(x => x.Mp3FileReferenceId)
                                           .First()
                                           .ToDictionary(pair => pair.Mp3FileReferenceId, pair => pair.Mp3FileReferenceGenre);

                    genreDict = context.Mp3FileReferenceGenres.ToDictionary(x => x.Id);
                }

                var fileRefsByPrimaryArtist = new Dictionary<int, List<LibraryEntry>>();
                var fileRefsByAlbum = new Dictionary<int, List<LibraryEntry>>();
                var primaryArtistIdEntryDict = new Dictionary<int, int>();

                var fileRefs = fileRefsDict.Values.Select(x =>
                {
                    // Careful with context:  Performance issues happen when the framework doesn't know what to do
                    //                        with your Linq query!
                    //
                    var genre = primaryGenreDict.ContainsKey(x.Id) ? primaryGenreDict[x.Id] : null;

                    var entry = new LibraryEntry()
                    {
                        Album = x.Album?.Name ?? string.Empty,
                        FileName = x.FileName,
                        PrimaryArtist = x.PrimaryArtist?.Name ?? string.Empty,
                        Title = x.Title ?? string.Empty,
                        Track = (uint)(x.Track ?? 0),
                        Disc = (uint)(x.Album?.DiscNumber ?? 0),
                        PrimaryGenre = genre?.Name ?? string.Empty,
                        Id = x.Id
                    };

                    if (!primaryArtistIdEntryDict.ContainsKey(entry.Id))
                        primaryArtistIdEntryDict.Add(entry.Id, x.PrimaryArtistId ?? -1);

                    // Group by PrimaryArtistId
                    if (x.PrimaryArtistId != null)
                    {
                        if (!fileRefsByPrimaryArtist.ContainsKey(x.PrimaryArtistId.Value))
                            fileRefsByPrimaryArtist.Add(x.PrimaryArtistId.Value, new List<LibraryEntry>() { entry });

                        else
                            fileRefsByPrimaryArtist[x.PrimaryArtistId.Value].Add(entry);
                    }

                    // Group by AlbumId
                    if (x.AlbumId != null)
                    {
                        if (!fileRefsByAlbum.ContainsKey(x.AlbumId.Value))
                            fileRefsByAlbum.Add(x.AlbumId.Value, new List<LibraryEntry>() { entry });

                        else
                            fileRefsByAlbum[x.AlbumId.Value].Add(entry);
                    }

                    return entry;

                }).ToList();
                var albums = albumRefsDict.Values.Select(x => new Album()
                {
                    Id = x.Id,
                    Name = x.Name

                }).ToList();
                var artists = artistRefsDict.Values.Select(x => new Artist()
                {
                    Id = x.Id,
                    Name = x.Name,

                }).ToList();

                var artistAlbums = new Dictionary<int, List<Album>>();      // Albums belonging to primary artist "X"

                // Aggregated data
                foreach (var album in albums)
                {
                    album.Tracks = fileRefsByAlbum[album.Id];

                    var entryId = album.Tracks.FirstOrDefault()?.Id ?? -1;
                    var primaryArtistId = entryId >= 0 ? primaryArtistIdEntryDict[entryId] : -1;

                    if (primaryArtistId != -1)
                    {
                        if (!artistAlbums.ContainsKey(primaryArtistId))
                            artistAlbums.Add(primaryArtistId, new List<Album> { album });

                        else
                            artistAlbums[primaryArtistId].Add(album);
                    }
                }
                foreach (var artist in artists)
                {
                    if (artistAlbums.ContainsKey(artist.Id))
                        artist.Albums = artistAlbums[artist.Id];

                    else
                        artist.Albums = new List<Album>();
                }

                this.Library.Albums = albums;
                this.Library.Artists = artists;
                this.Library.Genres = genreDict.Values.Select(x => new Genre()
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToList();
                this.Library.Entries = fileRefs;
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error initializing IModelController:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                throw ex;
            }
            */
        }

        public void AddUpdateLibraryEntry(string fileName, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, TagLib.File tagRef)
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
                }
            }
            catch (Exception ex)
            {
                RaiseLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
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
                        mediaEntity = new Core.Database.M3UStream();
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
                RaiseLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
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
                RaiseLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
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
                RaiseLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
            }

            return Enumerable.Empty<Mp3FileReference>();
        }

        public IEnumerable<Mp3FileReferenceAlbum> GetArtistAlbums(int artistId, bool isPrimaryArtist)
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Mp3FileReferenceArtistMaps
                                          .Where(x => x.Mp3FileReferenceArtistId == artistId && isPrimaryArtist == x.IsPrimaryArtist)
                                          .Select(x => x.Mp3FileReference.Album)
                                          .Distinct()
                                          .ToList();
                }
            }
            catch (Exception ex)
            {
                RaiseLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
            }

            return Enumerable.Empty<Mp3FileReferenceAlbum>();
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
                RaiseLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.Database, LogLevel.Error, ex.Message);
            }

            return Enumerable.Empty<Mp3FileReference>();
        }

        public PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : class
        {
            try
            {
                using (var context = CreateContext())
                {
                    IEnumerable<TEntity> collection;
                    int totalRecords = 0;
                    int totalFilteredRecords = 0;

                    // Flat entities may be queried
                    if (typeof(TEntity) == typeof(M3UStream))
                    {
                        collection = (IEnumerable<TEntity>)context.M3UStreams;
                        totalRecords = context.M3UStreams.Count();

                        if (request.WhereCallback != null)
                            totalFilteredRecords = context.M3UStreams.Where(x => request.WhereCallback(x as TEntity)).Count();
                        else
                            totalFilteredRecords = totalRecords;
                    }
                    else if (typeof(TEntity) == typeof(Mp3FileReference))
                    {
                        collection = (IEnumerable<TEntity>)context.Mp3FileReferences;
                        totalRecords = context.Mp3FileReferences.Count();

                        if (request.WhereCallback != null)
                            totalFilteredRecords = context.Mp3FileReferences.AsEnumerable().Where(x => request.WhereCallback(x as TEntity)).Count();
                        else
                            totalFilteredRecords = totalRecords;
                    }
                    else if (typeof(TEntity) == typeof(Mp3FileReferenceArtist))
                    {
                        collection = (IEnumerable<TEntity>)context.Mp3FileReferenceArtists;
                        totalRecords = context.Mp3FileReferenceArtists.Count();

                        if (request.WhereCallback != null)
                            totalFilteredRecords = context.Mp3FileReferenceArtists.AsEnumerable().Where(x => request.WhereCallback(x as TEntity)).Count();
                        else
                            totalFilteredRecords = totalRecords;
                    }
                    else if (typeof(TEntity) == typeof(Mp3FileReferenceGenre))
                    {
                        collection = (IEnumerable<TEntity>)context.Mp3FileReferenceGenres;
                        totalRecords = context.Mp3FileReferenceGenres.Count();

                        if (request.WhereCallback != null)
                            totalFilteredRecords = context.Mp3FileReferenceGenres.AsEnumerable().Where(x => request.WhereCallback(x as TEntity)).Count();
                        else
                            totalFilteredRecords = totalRecords;
                    }
                    else if (typeof(TEntity) == typeof(RadioBrowserStation))
                    {
                        collection = (IEnumerable<TEntity>)context.RadioBrowserStations;
                        totalRecords = context.RadioBrowserStations.Count();

                        if (request.WhereCallback != null)
                            totalFilteredRecords = context.RadioBrowserStations.AsEnumerable().Where(x => request.WhereCallback(x as TEntity)).Count();
                        else
                            totalFilteredRecords = totalRecords;
                    }
                    else
                    {
                        throw new Exception("IModelController.GetPage only supports flat entity types");
                    }

                    // Order By
                    if (request.OrderByCallback != null)
                    {
                        collection = collection.OrderBy(x => request.OrderByCallback(x));
                    }

                    // Where
                    if (request.WhereCallback != null)
                    {
                        collection = collection.AsEnumerable().Where(x => request.WhereCallback(x));
                    }

                    // Finish Linq Statements (PageStart is a non-index integer)
                    collection = collection.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);

                    return new PageResult<TEntity>()
                    {
                        Results = collection.ToList(),
                        TotalRecordCount = totalRecords,
                        TotalRecordCountFiltered = totalFilteredRecords,
                        PageCount = (int)Math.Ceiling(totalRecords / (double)request.PageSize),
                        PageNumber = request.PageNumber,
                        PageSize = Math.Min(request.PageSize, collection.Count())
                    };
                }
            }
            catch (Exception ex)
            {
                RaiseLog("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
            }

            return PageResult<TEntity>.GetDefault();
        }

        /// <summary>
        /// Invokes logger on the application dispatcher thread
        /// </summary>
        protected void RaiseLog(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(RaiseLog, DispatcherPriority.Background, message, type, level, parameters);

            else
                _outputController.Log(message, type, level, parameters);
        }

        private AudioStationDbContext CreateContext()
        {
            var configuration = _configurationManager.GetConfiguration();

            var context = new AudioStationDbContext(configuration, _outputController, _currentLogLevel, _currentLogVerbosity);

            return context;
        }

        public void Dispose()
        {
            // TODO
        }
    }
}

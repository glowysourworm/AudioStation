using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [IocExport(typeof(IAudioStationDbClient))]
    public class AudioStationDbClient : IAudioStationDbClient
    {
        private AudioStationConfiguration _configuration;
        private readonly IIocEventAggregator _eventAggregator;

        LogLevel _currentLogLevel;
        bool _currentLogVerbosity;

        // IAudioStationService
        //
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private IAudioStationDataService.Status _status;
        private string _statusMessage;

        [IocImportingConstructor]
        public AudioStationDbClient(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _currentLogLevel = LogLevel.Trace;
            _currentLogVerbosity = true;

            _status = IAudioStationDataService.Status.Disabled;
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

        public void AddUpdateRadioEntry(Core.Model.M3U.M3UStream entry)
        {
            if (string.IsNullOrEmpty(entry.StreamSource) ||
                string.IsNullOrEmpty(entry.Title))
                throw new ArgumentException("M3UStream must have a stream source and a title");

            ExecuteDbContextAction("AddUpdateRadioEntry", "Adds entry for a radio station from M3U file", true, context =>
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
            });
        }
        public void AddRadioEntries(IEnumerable<Core.Model.M3U.M3UStream> entries)
        {
            ExecuteDbContextAction("AddRadioEntries", "Adds entries for radio stations from M3U file(s)", true, context =>
            {

                // Batch Add:  Assume there are no conflicting records. There may be
                //             batches that get thrown out; but the database index
                //             won't be of use for a large table like this one.


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
            });
        }

        public IEnumerable<Track> GetArtistTracks(Guid artistId)
        {
            return ExecuteDbContextFunc<IEnumerable<Track>>("GetArtistTracks", "Retrieves all tracks with matching artist ID (=" + artistId.ToString() + ")", true, context =>
            {
                return context.Tracks
                              .Where(x => x.ArtistId == artistId)
                              .ToList();
            });
        }
        public IEnumerable<Album> GetArtistAlbums(Guid artistId)
        {
            return ExecuteDbContextFunc<IEnumerable<Album>>("GetAlbumTracks", "Retrieves all albums with matching artist ID (=" + artistId.ToString() + ")", true, context =>
            {
                return context.Albums
                              .Where(x => x.ArtistId == artistId)
                              .ToList();
            });
        }
        public IEnumerable<Track> GetAlbumTracks(Guid albumId)
        {
            return ExecuteDbContextFunc<IEnumerable<Track>>("GetAlbumTracks", "Retrieves all tracks with matching album ID (=" + albumId.ToString() + ")", true, context =>
            {
                return context.Tracks
                              .Where(x => x.AlbumId == albumId)
                              .ToList();
            });
        }


        public int GetCount<TEntity>() where TEntity : AudioStationEntityBase
        {
            return ExecuteDbContextFunc<int>("GetCount", "Retrieves entity row count from the database", true, context =>
            {
                return GetEntitySet<TEntity>(context).Count();
            });
        }
        public PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : AudioStationEntityBase
        {
            return ExecuteDbContextFunc<PageResult<TEntity>>("GetPage", "Retrieves a page of the given entity data from the database", true, context =>
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
            });
        }
        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : AudioStationEntityBase
        {
            return ExecuteDbContextFunc<IEnumerable<TEntity>>("GetEntities", "Retrieves entity set from the database", true, context =>
            {
                return GetEntitySet<TEntity>(context).ToList();
            });
        }
        public IEnumerable<TEntity> GetEntitiesWhere<TEntity>(Func<TEntity, bool> predicate) where TEntity : AudioStationEntityBase
        {
            return ExecuteDbContextFunc<IEnumerable<TEntity>>("GetEntitiesWhere", "Retrieves entity set from the database matching the predicate", true, context =>
            {
                // Must bring these into memory to execute search
                return GetEntitySet<TEntity>(context).ToList().Where(predicate);
            });
        }
        public IEnumerable<TView> GetViewEntities<TView>() where TView : AudioStationViewEntityBase
        {
            return ExecuteDbContextFunc<IEnumerable<TView>>("GetViewEntities", "Retrieves entity set from the database (view)", true, context =>
            {
                return GetViewEntitySet<TView>(context).ToList();
            });
        }
        public TEntity? GetEntity<TEntity>(Guid id) where TEntity : AudioStationEntityBase
        {
            return ExecuteDbContextFunc<TEntity?>("GetEntity", "Retrieves entity in the database by ID (primary key) (=" + id.ToString() + ")", true, context =>
            {
                return context.Find<TEntity>(id);
            });
        }
        public TEntity? GetEnumEntity<TEnum, TEntity>(string enumName) where TEntity : AudioStationEnumEntityBase
        {
            return ExecuteDbContextFunc("GetEnumEntity", "Gets entity from the database (enum table) (=" + enumName + ")", true, context =>
            {
                // Enum Tables:  The tables will be setup when the application starts. The name consistency
                //               is built into the attribute.
                //
                return GetEntitySet<TEntity>(context).First(entity => entity.Name == enumName);
            });
        }
        public TEntity? GetEnumEntity<TEnum, TEntity>(TEnum enumValue) where TEntity : AudioStationEnumEntityBase
        {
            return ExecuteDbContextFunc("GetEnumEntity", "Gets entity from the database (enum table) (=" + enumValue.ToString() + ")", true, context =>
            {
                // Enum Tables:  The tables will be setup when the application starts. The name consistency
                //               is built into the attribute.
                //

                // Unfortunately, these have to be iterated
                var values = Enum.GetValues(typeof(TEnum));

                foreach (Enum value in values)
                {
                    if (value.Equals(enumValue))
                    {
                        var enumName = value.GetAttribute<DisplayAttribute>().Name;

                        return GetEntitySet<TEntity>(context).First(entity => entity.Name == enumName);
                    }
                }

                return null;
            });
        }

        public bool AddEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase
        {
            if (entity.Id == Guid.Empty)
                throw new ArgumentException("Trying to add entity with no ID");

            return ExecuteDbContextAction("AddEntity", "Adds entity to the database", true, context =>
            {
                context.Add<TEntity>(entity);
                context.SaveChanges();
            });
        }
        public TEntity? FirstEntity<TEntity>(Func<TEntity, bool> predicate) where TEntity : AudioStationEntityBase
        {
            return ExecuteDbContextFunc<TEntity?>("FirstEntity", "Retrieves the first entity in the database matching the predicate", true, context =>
            {
                return context.Set<TEntity>().AsEnumerable().FirstOrDefault(x => predicate(x));
            });
        }
        public bool UpdateEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase
        {
            if (entity.Id == Guid.Empty)
                throw new ArgumentException("Trying to update entity with no ID");

            return ExecuteDbContextAction("UpdateEntity", "Updates an entity in the database", true, context =>
            {
                context.Update<TEntity>(entity);
                context.SaveChanges();
            });
        }

        private TResult ExecuteDbContextFunc<TResult>(string actionName, string actionDescription, bool throwException, Func<AudioStationDbContext, TResult> func)
        {
            TResult result = default(TResult);

            ExecuteDbContextAction(actionName, actionDescription, throwException, (context) =>
            {
                result = func(context);
            });

            return result;
        }
        private bool ExecuteDbContextAction(string actionName, string actionDescription, bool throwException, Action<AudioStationDbContext> action)
        {
            try
            {
                ApplicationHelpers.Log("Execution started {0}: {1}", LogMessageDbType.AudioStation, LogLevel.Trace, null, actionName, actionDescription);

                using (var context = CreateContext())
                {
                    action(context);
                }

                ApplicationHelpers.Log("Execution completed {0}", LogMessageDbType.AudioStation, LogLevel.Trace, null, actionName);

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Execution error {0}:  {1}", LogMessageDbType.AudioStation, LogLevel.Error, ex, actionName, ex.Message);

                if (throwException)
                    throw new Exception(string.Format("Execution error {0}", actionName));
            }

            return false;
        }
        private DbSet<TView> GetViewEntitySet<TView>(AudioStationDbContext context) where TView : AudioStationViewEntityBase
        {
            if (typeof(TView) == typeof(MusicBrainzAcoustIDResult))
                return context.MusicBrainzAcoustIDResults as DbSet<TView>;

            else
                throw new Exception("Unhandled entity type:  AudioStationDbClient.GetViewEntitySet");
        }

        // The Set<> method has postgres / EF / npgsql issues. Probably related to configuration; but I'm running out of
        // options.
        //
        private DbSet<TEntity> GetEntitySet<TEntity>(AudioStationDbContext context) where TEntity : AudioStationEntityBase
        {
            if (typeof(TEntity) == typeof(AcoustIDLookupResult))
                return context.AcoustIDLookupResults as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Album))
                return context.Albums as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(AlbumFileReferenceMap))
                return context.AlbumFileReferenceMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Artist))
                return context.Artists as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(ArtistFileReferenceMap))
                return context.ArtistFileReferenceMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(FileReference))
                return context.FileReferences as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(FileType))
                return context.FileTypes as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Genre))
                return context.Genres as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(M3UStream))
                return context.M3UStreams as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(RadioBrowserStation))
                return context.RadioBrowserStations as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(TagSmall))
                return context.TagSmalls as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(TagSmallVendorMap))
                return context.TagSmallVendorMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(TagSmallFileReferenceMap))
                return context.TagSmallFileReferenceMaps as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Track))
                return context.Tracks as DbSet<TEntity>;

            else if (typeof(TEntity) == typeof(Vendor))
                return context.Vendors as DbSet<TEntity>;

            else
                throw new Exception("Unhandled entity type:  AudioStationDbClient.GetEntitySet");

        }

        private AudioStationDbContext CreateContext()
        {
            var context = new AudioStationDbContext(_configuration, _currentLogLevel, _currentLogVerbosity);

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
        public LogMessageServiceType GetLogServiceType()
        {
            return LogMessageServiceType.None;
        }
        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        public IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            _configuration = configuration;

            if (string.IsNullOrWhiteSpace(configuration.DatabaseHost))
                OnStatusChanged(IAudioStationDataService.Status.Error, "database host not specified");

            else if (string.IsNullOrWhiteSpace(configuration.DatabaseName))
                OnStatusChanged(IAudioStationDataService.Status.Error, "database name not specified");

            else if (string.IsNullOrWhiteSpace(configuration.DatabaseUser))
                OnStatusChanged(IAudioStationDataService.Status.Error, "database user not specified");

            else if (string.IsNullOrWhiteSpace(configuration.DatabasePassword))
                OnStatusChanged(IAudioStationDataService.Status.Error, "database password not specified");

            // Test Connection (Initialize:  Vendor table must be filled out)
            try
            {
                using (var context = CreateContext())
                {
                    // Vendor Names
                    foreach (var enumValue in Enum.GetValues<VendorNames>())
                    {
                        var enumName = enumValue.GetAttribute<DisplayAttribute>().Name;

                        // Check for existing entity
                        var entity = context.Vendors.ToList().FirstOrDefault(x => x.Name == enumName);

                        // Add (NO UPDATE)
                        if (entity == null)
                        {
                            entity = new Vendor()
                            {
                                Name = enumName!
                            };

                            context.Add<Vendor>(entity);
                        }
                    }

                    context.SaveChanges();

                    // File Types
                    foreach (var enumValue in Enum.GetValues<FileTypes>())
                    {
                        var enumName = enumValue.GetAttribute<DisplayAttribute>().Name;

                        // Check for existing entity
                        var entity = context.FileTypes.ToList().FirstOrDefault(x => x.Name == enumName);

                        // Add (NO UPDATE)
                        if (entity == null)
                        {
                            entity = new FileType()
                            {
                                Name = enumName!
                            };

                            context.Add<FileType>(entity);
                        }
                    }

                    context.SaveChanges();
                }

                OnStatusChanged(IAudioStationDataService.Status.Idle, "database configuration OK!");
            }
            catch (Exception ex)
            {
                OnStatusChanged(IAudioStationDataService.Status.Error, "database connection failed!");
                //ApplicationHelpers.Log("Database connection failed!", LogMessageType.)
            }

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
            return this.GetDisplayName() + ": " + _statusMessage;
        }

        private void OnStatusChanged(IAudioStationDataService.Status status, string message)
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

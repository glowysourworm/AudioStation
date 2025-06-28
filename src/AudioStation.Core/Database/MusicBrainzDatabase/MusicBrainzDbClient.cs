using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [IocExport(typeof(IMusicBrainzDbClient))]
    public class MusicBrainzDbClient : IMusicBrainzDbClient
    {
        private readonly IConfigurationManager _configurationManager;

        LogLevel _currentLogLevel;
        bool _currentLogVerbosity;

        [IocImportingConstructor]
        public MusicBrainzDbClient(IConfigurationManager configurationManager, IIocEventAggregator eventAggregator)
        {
            _configurationManager = configurationManager;

            // Update log output configuration
            eventAggregator.GetEvent<LogConfigurationChangedEvent>().Subscribe(payload =>
            {
                if (payload.Type == LogMessageType.Database)
                {
                    _currentLogLevel = payload.Level;
                    _currentLogVerbosity = payload.Verbose;
                }
            });
        }

        public PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    IEnumerable<TEntity> collection = context.Set<TEntity>();
                    int totalRecords = context.Set<TEntity>().Count();
                    int totalFilteredRecords = 0;

                    if (request.WhereCallback != null)
                        totalFilteredRecords = context.Set<TEntity>().Where(x => request.WhereCallback(x as TEntity)).Count();
                    else
                        totalFilteredRecords = totalRecords;

                    // Order By
                    if (request.OrderByCallback != null)
                    {
                        collection = context.Set<TEntity>().OrderBy(x => request.OrderByCallback(x));
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                throw ex;
            }
        }

        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : MusicBrainzEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Set<TEntity>().AsEnumerable();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                throw ex;
            }
        }

        public TEntity? GetEntity<TEntity>(Guid musicBrainzId) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Find<TEntity>(musicBrainzId);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                throw ex;
            }
        }

        public bool UpdateEntity<TEntity>(TEntity entity) where TEntity : MusicBrainzEntityBase
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error);
                throw ex;
            }
        }

        private MusicBrainzDbContext CreateContext()
        {
            var configuration = _configurationManager.GetConfiguration();

            var context = new MusicBrainzDbContext(configuration, _currentLogVerbosity);

            return context;
        }
    }
}

﻿using System.Linq.Expressions;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz;

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

        public void AddUrlRelationship<TEntity>(MusicBrainzUrlEntity entity, TEntity relatedEntity) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                // Get Type ID for the related entity
                var typeId = GetEntityTypeId<MusicBrainzUrlEntity>();

                using (var context = CreateContext())
                {
                    if (context.Find<MusicBrainzUrlEntity>(entity.Id) == null)
                        context.Add(entity);

                    if (!context.Set<MusicBrainzUrlEntityMap>().Any(x => x.MusicBrainzEntityId == relatedEntity.Id &&
                                                                         x.MusicBrainzUrlId == entity.Id))
                    {
                        context.Add(new MusicBrainzUrlEntityMap()
                        {
                            MusicBrainzEntityId = relatedEntity.Id,
                            MusicBrainzEntityTypeId = typeId,
                            MusicBrainzUrlId = entity.Id,
                        });
                    }
                    

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public void AddGenreRelationship<TEntity>(MusicBrainzGenreEntity entity, TEntity relatedEntity) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                // Get Type ID for the related entity
                var typeId = GetEntityTypeId<MusicBrainzGenreEntity>();

                using (var context = CreateContext())
                {
                    if (context.Find<MusicBrainzGenreEntity>(entity.Id) == null)
                        context.Add(entity);

                    if (!context.Set<MusicBrainzGenreEntityMap>().Any(x => x.MusicBrainzEntityId == relatedEntity.Id &&
                                                                           x.MusicBrainzGenreId == entity.Id))
                    {
                        context.Add(new MusicBrainzGenreEntityMap()
                        {
                            MusicBrainzEntityId = relatedEntity.Id,
                            MusicBrainzEntityTypeId = typeId,
                            MusicBrainzGenreId = entity.Id,
                        });
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public void AddTagRelationship<TEntity>(MusicBrainzTagEntity entity, TEntity relatedEntity) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                // Get Type ID for the related entity
                var typeId = GetEntityTypeId<MusicBrainzUrlEntity>();

                using (var context = CreateContext())
                {
                    if (context.Find<MusicBrainzTagEntity>(entity.Id) == null)
                        context.Add(entity);

                    if (!context.Set<MusicBrainzTagEntityMap>().Any(x => x.MusicBrainzEntityId == relatedEntity.Id &&
                                                                         x.MusicBrainzTagId == entity.Id))
                    {
                        context.Add(new MusicBrainzTagEntityMap()
                        {
                            MusicBrainzEntityId = relatedEntity.Id,
                            MusicBrainzEntityTypeId = typeId,
                            MusicBrainzTagId = entity.Id,
                        });
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
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
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public IEnumerable<TEntity> Where<TEntity>(Func<TEntity, bool> predicate) where TEntity : class
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Set<TEntity>().Where(predicate).ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : MusicBrainzEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Set<TEntity>().ToList();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data page:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
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
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public void AddEntity<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                using (var context = CreateContext())
                {
                    context.Add<TEntity>(entity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error retrieving data:  " + ex.Message, LogMessageType.Database, LogLevel.Error , ex);
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
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public bool ContainsEntity<TEntity>(TEntity entity) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Find<TEntity>(entity.Id) != null;
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        public bool Any<TEntity>(Func<TEntity, bool> predicate) where TEntity : MusicBrainzEntityBase
        {
            try
            {
                using (var context = CreateContext())
                {
                    return context.Set<TEntity>().Where(predicate).Any();
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error querying entity data:  " + ex.Message, LogMessageType.Database, LogLevel.Error, ex);
                throw ex;
            }
        }

        private int GetEntityTypeId<TEntity>() where TEntity : MusicBrainzEntityBase
        {
            using (var context = CreateContext())
            {
                EntityType type = EntityType.Unknown;

                if (typeof(TEntity) == typeof(MusicBrainzArtistEntity))
                {
                    type = EntityType.Artist;
                }
                else if (typeof(TEntity) == typeof(MusicBrainzGenreEntity))
                {
                    type = EntityType.Genre;
                }
                else if (typeof(TEntity) == typeof(MusicBrainzLabelEntity))
                {
                    type = EntityType.Label;
                }
                else if (typeof(TEntity) == typeof(MusicBrainzMediumEntity))
                {
                    throw new Exception("Can't support relationships to disc entity");
                }
                else if (typeof(TEntity) == typeof(MusicBrainzRecordingEntity))
                {
                    type = EntityType.Recording;
                }
                else if (typeof(TEntity) == typeof(MusicBrainzReleaseEntity))
                {
                    type = EntityType.Release;
                }
                else if (typeof(TEntity) == typeof(MusicBrainzTagEntity))
                {
                    throw new Exception("Can't support relationships to disc entity");
                }
                else if (typeof(TEntity) == typeof(MusicBrainzTrackEntity))
                {
                    throw new Exception("Can't support relationships to disc entity");
                }
                else if (typeof(TEntity) == typeof(MusicBrainzUrlEntity))
                {
                    type = EntityType.Url;
                }
                else
                    throw new Exception("Unhandled Music Brainz Type:  MusicBrainzDbClient.cs");

                return context.Set<MusicBrainzEntityType>().Where(x => x.Name == type.ToString()).First().Id;
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

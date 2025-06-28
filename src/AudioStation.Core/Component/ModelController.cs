using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

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

        LogLevel _currentLogLevel;
        bool _currentLogVerbosity;

        [IocImportingConstructor]
        public ModelController(IIocEventAggregator eventAggregator,
                               IAudioStationDbClient audioStationDbClient)
        {
            _eventAggregator = eventAggregator;
            _audioStationDbClient = audioStationDbClient;
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

        public PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : AudioStationEntityBase
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

        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : AudioStationEntityBase
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

        public TEntity GetEntity<TEntity>(int id) where TEntity : AudioStationEntityBase
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

        public bool UpdateEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase
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
    }
}

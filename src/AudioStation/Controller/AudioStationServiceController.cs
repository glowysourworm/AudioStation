using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationServiceController))]
    public class AudioStationServiceController : IAudioStationServiceController
    {
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> ComponentInitializedEvent;
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> ComponentStatusChangedEvent;

        // IAudioStationService
        private readonly ICDImportService _cdImportService;
        private readonly ILibraryLoaderService _libraryLoaderService;
        private readonly ILibraryLoaderWorkerService _libraryLoaderWorkerService;
        private readonly ILibraryMapperService _libraryMapperService;
        private readonly INowPlayingService _nowPlayingService;

        // IAudioStationDataService
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IAudioStationLogService _outputController;
        private readonly IAudioController _audioController;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IBandcampClient _bandcampClient;
        private readonly IDiscogsClient _discogsClient;
        private readonly IFanartClient _fanartClient;
        private readonly IITunesClient _iTunesClient;
        private readonly ILastFmClient _lastFmClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly ISpotifyClient _spotifyClient;

        [IocImportingConstructor]
        public AudioStationServiceController(ICDImportService cdImportService,
                                             ILibraryLoaderService libraryLoaderService,
                                             ILibraryLoaderWorkerService libraryLoaderWorkerService,
                                             ILibraryMapperService libraryMapperService,
                                             INowPlayingService nowPlayingService,

                                             IAudioStationDbClient audioStationDbClient,
                                             IAudioController audioController,
                                             IAudioStationLogService outputController,
                                             IAcoustIDClient acoustIDClient,
                                             IBandcampClient bandcampClient,
                                             IDiscogsClient discogsClient,
                                             IFanartClient fanartClient,
                                             IITunesClient itunesClient,
                                             ILastFmClient lastFmClient,
                                             IMusicBrainzClient musicBrainzClient,
                                             ISpotifyClient spotifyClient)
        {
            _cdImportService = cdImportService;
            _libraryLoaderService = libraryLoaderService;
            _libraryLoaderWorkerService = libraryLoaderWorkerService;
            _libraryMapperService = libraryMapperService;
            _nowPlayingService = nowPlayingService;

            _audioStationDbClient = audioStationDbClient;
            _audioController = audioController;
            _outputController = outputController;
            _acoustIDClient = acoustIDClient;
            _bandcampClient = bandcampClient;
            _discogsClient = discogsClient;
            _fanartClient = fanartClient;
            _iTunesClient = itunesClient;
            _lastFmClient = lastFmClient;
            _musicBrainzClient = musicBrainzClient;
            _spotifyClient = spotifyClient;

            _acoustIDClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _audioStationDbClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _audioController.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _bandcampClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _discogsClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _fanartClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _iTunesClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _lastFmClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _musicBrainzClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _outputController.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _spotifyClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
        }

        public void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Procedure
            // 
            // 0) Load IAudioStationService instances
            //      -> Errors:  Show User / Exit (optionally)
            //      -> Success: Continue
            //
            // 1) Load IAudioStationDataService instances
            //      -> Errors:  Show User / Exit (optionally)
            //      -> Success: Continue
            //
            // 2) Report between components
            //

            var taskCount = 11;
            var task = 0;

            // IAudioStationService (these are primary service components)
            //
            _cdImportService.Initialize(configuration);
            _libraryLoaderService.Initialize(configuration, audioStationController, progressHandler);
            _libraryLoaderWorkerService.Initialize(configuration, audioStationController, progressHandler);
            _libraryMapperService.Initialize(configuration, audioStationController, progressHandler);
            _nowPlayingService.Initialize(configuration, audioStationController, progressHandler);

            // IAudioStationDataService (these display their status on the status bar)
            //
            InitializeImpl(_outputController, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_audioStationDbClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_audioController, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_bandcampClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_acoustIDClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_discogsClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_fanartClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_iTunesClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_lastFmClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_musicBrainzClient, configuration, task++, taskCount, progressHandler);
            InitializeImpl(_spotifyClient, configuration, task++, taskCount, progressHandler);
        }

        private void InitializeImpl(IAudioStationDataService service, AudioStationConfiguration configuration, int taskNumber, int taskCount, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            progressHandler(taskCount, taskNumber, 0, string.Format("Initializing {0}", service.GetDisplayName()));
            var status = service.Initialize(configuration);

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(service, status);
        }

        public T GetService<T>() where T : IAudioStationService
        {
            if (typeof(T) == typeof(ICDImportService))
                return (T)_cdImportService;

            else if (typeof(T) == typeof(ILibraryLoaderService))
                return (T)_libraryLoaderService;

            else if (typeof(T) == typeof(ILibraryLoaderWorkerService))
                return (T)_libraryLoaderWorkerService;

            else if (typeof(T) == typeof(ILibraryMapperService))
                return (T)_libraryMapperService;

            else if (typeof(T) == typeof(INowPlayingService))
                return (T)_nowPlayingService;

            else
                throw new Exception("Unhandled IAudioStationService type");
        }

        public T GetDataService<T>() where T : IAudioStationDataService
        {
            if (typeof(T) == typeof(IAudioStationLogService))
                return (T)_outputController;

            else if (typeof(T) == typeof(IAudioController))
                return (T)_audioController;

            else if (typeof(T) == typeof(IAcoustIDClient))
                return (T)_acoustIDClient;

            else if (typeof(T) == typeof(IAudioStationDbClient))
                return (T)_audioStationDbClient;

            else if (typeof(T) == typeof(IBandcampClient))
                return (T)_bandcampClient;

            else if (typeof(T) == typeof(ICDImportService))
                return (T)_cdImportService;

            else if (typeof(T) == typeof(IDiscogsClient))
                return (T)_discogsClient;

            else if (typeof(T) == typeof(IFanartClient))
                return (T)_fanartClient;

            else if (typeof(T) == typeof(IITunesClient))
                return (T)_iTunesClient;

            else if (typeof(T) == typeof(ILastFmClient))
                return (T)_lastFmClient;

            else if (typeof(T) == typeof(IMusicBrainzClient))
                return (T)_musicBrainzClient;

            else if (typeof(T) == typeof(ISpotifyClient))
                return (T)_spotifyClient;

            else
                throw new Exception("Unhandled IAudioStationDataService type");
        }

        private void IAudioStationComponent_StatusChangeEvent(IAudioStationDataService sender, IAudioStationDataService.Status status)
        {
            if (this.ComponentStatusChangedEvent != null)
                this.ComponentStatusChangedEvent(sender, status);
        }
    }
}

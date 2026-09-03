using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationServiceController))]
    public class AudioStationServiceController : IAudioStationServiceController
    {
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> ComponentInitializedEvent;
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> ComponentStatusChangedEvent;

        // IAudioStationService
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IOutputController _outputController;
        private readonly IAudioController _audioController;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IBandcampClient _bandcampClient;
        private readonly ICDImportService _cdImportService;
        private readonly IDiscogsClient _discogsClient;
        private readonly IFanartClient _fanartClient;
        private readonly IITunesClient _iTunesClient;
        private readonly ILastFmClient _lastFmClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly ISpotifyClient _spotifyClient;

        [IocImportingConstructor]
        public AudioStationServiceController(IAudioStationDbClient audioStationDbClient,
                                             IAudioController audioController,
                                             IOutputController outputController,
                                             IAcoustIDClient acoustIDClient,
                                             IBandcampClient bandcampClient,
                                             ICDImportService cdImportService,
                                             IDiscogsClient discogsClient,
                                             IFanartClient fanartClient,
                                             IITunesClient itunesClient,
                                             ILastFmClient lastFmClient,
                                             IMusicBrainzClient musicBrainzClient,
                                             ISpotifyClient spotifyClient)
        {
            _audioStationDbClient = audioStationDbClient;
            _audioController = audioController;
            _outputController = outputController;
            _acoustIDClient = acoustIDClient;
            _bandcampClient = bandcampClient;
            _cdImportService = cdImportService;
            _discogsClient = discogsClient;
            _fanartClient = fanartClient;
            _iTunesClient = itunesClient;
            _lastFmClient = lastFmClient;
            _musicBrainzClient = musicBrainzClient;
            _spotifyClient = spotifyClient;

            _audioStationDbClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _audioController.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _outputController.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _acoustIDClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _bandcampClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _discogsClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _fanartClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _iTunesClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _lastFmClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _musicBrainzClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
            _spotifyClient.StatusChangeEvent += IAudioStationComponent_StatusChangeEvent;
        }

        public void Initialize(AudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Procedure
            // 
            // 1) Load IAudioStationComponent instances
            //      -> Errors:  Show User / Exit
            //      -> Success: Continue
            //
            // 2) Report between components
            //

            var taskCount = 11;
            var task = 0;

            // IAudioStationComponent (these display their status on the status bar)
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

        private void InitializeImpl(IAudioStationService service, AudioStationConfiguration configuration, int taskNumber, int taskCount, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            progressHandler(taskCount, taskNumber, 0, string.Format("Initializing {0}", service.GetDisplayName()));
            var status = service.Initialize(configuration);

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(service, status);
        }

        public T GetComponent<T>() where T : IAudioStationService
        {
            if (typeof(T) == typeof(IOutputController))
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
                throw new Exception("Unhandled IAudioStationComponent type");
        }

        private void IAudioStationComponent_StatusChangeEvent(IAudioStationService sender, IAudioStationService.Status status)
        {
            if (this.ComponentStatusChangedEvent != null)
                this.ComponentStatusChangedEvent(sender, status);
        }
    }
}

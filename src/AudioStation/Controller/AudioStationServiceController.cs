using AudioStation.Controller.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.EventHandler;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationComponentController))]
    public class AudioStationServiceController : IAudioStationComponentController
    {
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> ComponentInitializedEvent;
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> ComponentStatusChangedEvent;

        // IAudioStationComponent
        private readonly IOutputController _outputController;
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
        public AudioStationServiceController(IAudioController audioController,
                                               IOutputController outputController,
                                               IAcoustIDClient acoustIDClient,
                                               IBandcampClient bandcampClient,
                                               IDiscogsClient discogsClient,
                                               IFanartClient fanartClient,
                                               IITunesClient itunesClient,
                                               ILastFmClient lastFmClient,
                                               IMusicBrainzClient musicBrainzClient,
                                               ISpotifyClient spotifyClient)
        {

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

        private void IAudioStationComponent_StatusChangeEvent(IAudioStationService sender, IAudioStationService.Status status)
        {
            if (this.ComponentStatusChangedEvent != null)
                this.ComponentStatusChangedEvent(sender, status);
        }

        public async Task Initialize(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Procedure
            // 
            // 1) Load IAudioStationComponent instances
            //      -> Errors:  Show User / Exit
            //      -> Success: Continue
            //
            // 2) Report between components
            //

            var taskCount = 10;
            var task = 0;
            var status = IAudioStationService.Status.Disabled;

            // IAudioStationComponent (these display their status on the status bar)
            //
            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _outputController.GetDisplayName()));
            status = await _outputController.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_outputController, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _audioController.GetDisplayName()));
            status = await _audioController.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_audioController, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _bandcampClient.GetDisplayName()));
            status = await _bandcampClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_bandcampClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _acoustIDClient.GetDisplayName()));
            status = await _acoustIDClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_acoustIDClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _discogsClient.GetDisplayName()));
            status = await _discogsClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_discogsClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _fanartClient.GetDisplayName()));
            status = await _fanartClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_fanartClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _iTunesClient.GetDisplayName()));
            status = await _iTunesClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_iTunesClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _lastFmClient.GetDisplayName()));
            status = await _lastFmClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_lastFmClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _musicBrainzClient.GetDisplayName()));
            status = await _musicBrainzClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_musicBrainzClient, status);

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _spotifyClient.GetDisplayName()));
            status = await _spotifyClient.Initialize();

            if (this.ComponentInitializedEvent != null)
                this.ComponentInitializedEvent(_spotifyClient, status);
        }

        public T GetComponent<T>() where T : IAudioStationService
        {
            if (typeof(T) == typeof(IOutputController))
                return (T)_outputController;

            else if (typeof(T) == typeof(IAudioController))
                return (T)_audioController;

            else if (typeof(T) == typeof(IAcoustIDClient))
                return (T)_acoustIDClient;

            else if (typeof(T) == typeof(IBandcampClient))
                return (T)_bandcampClient;

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
    }
}

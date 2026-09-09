using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IITunesClient))]
    public class ITunesClient : IITunesClient
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private IAudioStationDataService.Status _status;

        public async Task<ITunesNowPlaying> SearchArtist(string artistName, string albumName)
        {
            return null;
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "ITunes Client";
        }
        public string GetDisplayName()
        {
            return "ITunes Client";
        }
        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        public IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            return IAudioStationDataService.Status.Idle;
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
            return this.GetDisplayName() + " " + IAudioStationDataService.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationDataService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}

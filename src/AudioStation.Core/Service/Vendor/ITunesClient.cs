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
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

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
        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        public IAudioStationService.Status Initialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }

        public Task<IAudioStationService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public IAudioStationService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }

        public Task<IAudioStationService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationService.Status.Idle);
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationService.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}

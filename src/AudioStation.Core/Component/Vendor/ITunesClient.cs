using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model.Vendor;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IITunesClient))]
    public class ITunesClient : IITunesClient
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationComponent, IAudioStationComponent.Status> StatusChangeEvent;

        private IAudioStationComponent.Status _status;

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
        public IAudioStationComponent.Status GetStatus()
        {
            return _status;
        }
        public async Task<IAudioStationComponent.Status> Initialize()
        {
            // -> Idle
            OnStatusChanged(IAudioStationComponent.Status.Disabled);

            return _status;
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationComponent.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationComponent.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}

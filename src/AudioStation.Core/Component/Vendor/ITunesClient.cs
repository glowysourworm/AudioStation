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
            // TODO
            return IAudioStationComponent.Status.Idle;
        }
        public async Task<IAudioStationComponent.Status> Initialize()
        {
            // TODO
            return IAudioStationComponent.Status.Idle;
        }
        public string GetStatusMessage()
        {
            return "TODO (ITunes Client)";
        }
        #endregion
    }
}

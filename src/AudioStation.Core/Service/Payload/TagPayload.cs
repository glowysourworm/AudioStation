using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Service.Payload
{
    public class TagPayload : PayloadBase
    {
        public IAudioStationTag Data { get; }

        public TagPayload(IAudioStationTag data)
        {
            this.Data = data;
        }
    }
}

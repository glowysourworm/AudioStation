using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service
{
    public class AudioStationTagServiceRequest
    {
        public AudioStationTagRequestType Type { get; }
        public ITagServiceInputPayload Payload { get; }

        public AudioStationTagServiceRequest(AudioStationTagRequestType type, ITagServiceInputPayload payload)
        {
            this.Type = type;
            this.Payload = payload;
        }
    }
}

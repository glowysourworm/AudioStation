using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service
{
    public class AudioStationTagServiceResponse
    {
        public ITagServiceOutputPayload Payload { get; private set; }
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public AudioStationTagServiceResponse(ITagServiceOutputPayload payload, bool success, string message)
        {
            this.Payload = payload;
            this.Success = success;
            this.Message = message;
        }
    }
}

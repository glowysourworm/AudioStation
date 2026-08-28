using AudioStation.Core.Service.Payload;

namespace AudioStation.Core.Service
{
    public class AudioStationTagServiceResponse
    {
        public PayloadBase Payload { get; private set; }
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public AudioStationTagServiceResponse(PayloadBase payload, bool success, string message)
        {
            this.Payload = payload;
            this.Success = success;
            this.Message = message;
        }
    }
}

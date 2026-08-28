namespace AudioStation.Core.Service.Interface
{
    /// <summary>
    /// Service that provides tag data with the specified AudioStationTagServiceModel tag properties
    /// filled in for other components to use.
    /// </summary>
    public interface IAudioStationTagService
    {
        Task<AudioStationTagServiceResponse> ProcessRequestAsync(AudioStationTagServiceRequest serviceModel);
        AudioStationTagServiceResponse ProcessRequest(AudioStationTagServiceRequest serviceModel);
    }
}

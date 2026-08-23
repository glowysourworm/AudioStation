using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Service.Interface
{
    /// <summary>
    /// Service that provides tag data with the specified AudioStationTagServiceModel tag properties
    /// filled in for other components to use.
    /// </summary>
    public interface IAudioStationTagService
    {
        Task<IAudioStationTag?> GetTagData(AudioStationTagServiceModel serviceModel);
        Task<ITagSmall> GetTagSmallData(AudioStationTagServiceModel serviceModel);
    }
}

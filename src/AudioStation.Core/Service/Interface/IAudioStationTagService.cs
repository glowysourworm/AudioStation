using ATL;

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
        Task<IAudioStationTag?> GetTagAsync(AudioStationTagServiceModel serviceModel);
        Task<ITagSmall?> GetTagSmallAsync(AudioStationTagServiceModel serviceModel);

        IAudioStationTag? GetTag(AudioStationTagServiceModel serviceModel);
        ITagSmall? GetTagSmall(AudioStationTagServiceModel serviceModel);

        Task<PictureInfo?> GetFrontArtAsync(AudioStationTagServiceModel serviceModel);
        PictureInfo? GetFrontArt(AudioStationTagServiceModel serviceModel);
        Task<PictureInfo?> GetBackArtAsync(AudioStationTagServiceModel serviceModel);
        PictureInfo? GetBackArt(AudioStationTagServiceModel serviceModel);
    }
}

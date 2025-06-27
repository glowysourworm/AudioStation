using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IWikipediaClient
    {
        Task<WikipediaData> GetExcerpt(string artistName);
    }
}

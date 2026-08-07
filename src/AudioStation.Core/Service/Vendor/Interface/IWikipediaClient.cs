using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IWikipediaClient
    {
        Task<WikipediaData> GetExcerpt(string artistName);
    }
}

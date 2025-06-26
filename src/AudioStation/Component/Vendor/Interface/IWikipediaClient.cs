using AudioStation.Component.Vendor.Wikipedia;

namespace AudioStation.Component.Vendor.Interface
{
    public interface IWikipediaClient
    {
        Task<WikipediaData> GetExcerpt(string artistName);
    }
}

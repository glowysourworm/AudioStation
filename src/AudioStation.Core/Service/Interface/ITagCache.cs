using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Service.Interface
{
    public interface ITagCache : IAudioStationCache
    {
        /// <summary>
        /// Gets a tag file from the cache
        /// </summary>
        TagSmall Get(string fileName);

        /// <summary>
        /// Gets a copy of the tag file from the cache
        /// </summary>
        TagSmall GetCopy(string fileName);

        /// <summary>
        /// Returns the full tag after reading the specified file. These tags are not cached.
        /// </summary>
        TagFull GetFullTag(string fileName);

        /// <summary>
        /// Sets a tag file into the cache. You must set the full tag explicitly. Otherwise,
        /// the tag cache will only set the small portion of the file (ITagSmall).
        /// </summary>
        void Set(string fileName);

        /// <summary>
        /// Sets tag data into the cache; and, optionally, saves it to file. The ITagSmall only
        /// represents a portion of the tag data; and only this data will be saved, or overwrite
        /// the existing tag data.
        /// </summary>
        void SetData(string fileName, ITagSmall tagData, bool save = true);

        /// <summary>
        /// Removes a file from the cache. Please use this during any file move procedure.
        /// </summary>
        void Evict(string fileName);
    }
}

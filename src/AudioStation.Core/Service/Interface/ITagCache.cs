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
        TagFull Get(string fileName);

        /// <summary>
        /// Gets a small portion of the file's information and stores it in the cache
        /// </summary>
        TagSmall GetSmall(string fileName);

        /// <summary>
        /// Gets a copy of the tag file from the cache
        /// </summary>
        TagFull GetCopy(string fileName);

        /// <summary>
        /// Gets a copy of the tag file from the cache
        /// </summary>
        TagSmall GetCopySmall(string fileName);

        /// <summary>
        /// Sets a tag file into the cache. You must set the full tag explicitly. Otherwise,
        /// the tag cache will only set the small portion of the file (ITagSmall).
        /// </summary>
        void Set(string fileName, bool fullTag = false);

        /// <summary>
        /// Sets tag data and, optionally, saves tag to file.
        /// </summary>
        void SetData(string fileName, ITagFull tagData, bool save = true);

        /// <summary>
        /// Sets tag data into the cache. This will not set the full tag as
        /// the ITagSmall data only represents a portion of the full tag supported
        /// by AudioStation
        /// </summary>
        void SetData(string fileName, ITagSmall tagData);

        /// <summary>
        /// Verifies that the file exists and is valid
        /// </summary>
        bool Verify(string fileName);

        /// <summary>
        /// Removes a file from the cache. Please use this during any file move procedure.
        /// </summary>
        void Evict(string fileName);
    }
}

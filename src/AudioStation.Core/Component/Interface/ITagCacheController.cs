using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Component.Interface
{
    public interface ITagCacheController
    {
        /// <summary>
        /// Gets a tag file from the cache
        /// </summary>
        AudioStationTag Get(string fileName);

        /// <summary>
        /// Gets a copy of the tag file from the cache
        /// </summary>
        AudioStationTag GetCopy(string fileName);

        /// <summary>
        /// Sets a tag file into the cache
        /// </summary>
        void Set(string fileName);

        /// <summary>
        /// Sets tag data using AutoMapper, and, optionally, saves tag to file.
        /// </summary>
        void SetData(string fileName, IAudioStationTag tagData, bool save = true);

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

namespace AudioStation.Core.Component.Interface
{
    public interface ITagCacheController
    {
        /// <summary>
        /// Gets a tag file from the cache
        /// </summary>
        TagLib.File Get(string fileName);

        /// <summary>
        /// Sets a tag file into the cache
        /// </summary>
        void Set(string fileName);

        /// <summary>
        /// Sets tag data using AutoMapper, and, optionally, saves tag to file.
        /// </summary>
        void SetData(string fileName, TagLib.Tag tagData, bool save = true);

        /// <summary>
        /// Copy tag data to clipboard
        /// </summary>
        bool CopyToClipboard(TagLib.Tag tag);

        /// <summary>
        /// Copy last tag data from clipboard
        /// </summary>
        TagLib.Tag CopyFromClipboard();
    }
}

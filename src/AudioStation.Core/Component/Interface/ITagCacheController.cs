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
        /// Copy tag data to clipboard
        /// </summary>
        bool CopyToClipboard(TagLib.Tag tag);

        /// <summary>
        /// Copy last tag data from clipboard
        /// </summary>
        TagLib.Tag CopyFromClipboard();

        /// <summary>
        /// Maps to / from another tag type that inherits from a tag-lib tag
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource tagSource) where TSource : TagLib.Tag
                                                                          where TDestination : TagLib.Tag;

        /// <summary>
        /// Maps to / from another tag type that inherits from a tag-lib tag
        /// </summary>
        public TDestination MapOnto<TSource, TDestination>(TSource tagSource, TDestination tagDestination) where TSource : TagLib.Tag
                                                                                                           where TDestination : TagLib.Tag;
    }
}

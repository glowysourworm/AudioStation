namespace AudioStation.Core.Controller.ImageCacheModel
{
    public enum ImageCacheType
    {
        /// <summary>
        /// Thumbnail cache will stay in memory and not require much re-caching.
        /// </summary>
        Thumbnail = 0,

        /// <summary>
        /// Small cache image will usually stay in memory unless there has been a
        /// lot of other loading.
        /// </summary>
        Small = 1,

        /// <summary>
        /// Medium cache image will usually stay in memory unless there has been some
        /// other loading.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Full size cache will only stay in memory for a short period of time - usually
        /// for virtual scrolling purposes.
        /// </summary>
        FullSize = 3
    }
}

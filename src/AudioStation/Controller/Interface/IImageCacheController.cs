using System.Windows.Media;

using AudioStation.Controller.Model;

using TagLib;

namespace AudioStation.Controller.Interface
{
    public interface IImageCacheController
    {
        /// <summary>
        /// Creates or returns image source(s) for the specified artist. The images are prepared (if there are any
        /// specifications) for viewing the artist.
        /// </summary>
        Task<ImageSource> GetForArtist(int artistId, ImageCacheType cacheAsType);

        /// <summary>
        /// Creates or returns image source(s) for the specified album. The images are prepared (if there are any
        /// specifications) for viewing the album.
        /// </summary>
        Task<ImageSource> GetForAlbum(int albumId, ImageCacheType cacheAsType);

        /// <summary>
        /// Returns a web image with the desired size. The cache is kept based on your cache type input; and no
        /// other media web types are considered for separate caching.
        /// </summary>
        Task<ImageSource> GetFromEndpoint(string endpoint, PictureType cacheType, ImageCacheType cacheAsType);
    }
}

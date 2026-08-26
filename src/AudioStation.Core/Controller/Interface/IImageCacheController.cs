using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Controller.ImageCacheModel;

using PictureType = ATL.PictureInfo.PIC_TYPE;

namespace AudioStation.Core.Controller.Interface
{
    public interface IImageCacheController
    {
        /// <summary>
        /// Creates or returns image source(s) for the specified artist. The images are prepared (if there are any
        /// specifications) for viewing the artist.
        /// </summary>
        Task<BitmapImageData> GetForArtist(int artistId, ImageCacheType cacheAsType);

        /// <summary>
        /// Creates or returns image source(s) for the specified album. The images are prepared (if there are any
        /// specifications) for viewing the album.
        /// </summary>
        Task<BitmapImageData> GetForAlbum(int albumId, ImageCacheType cacheAsType);

        /// <summary>
        /// Returns a web image with the desired size. The cache is kept based on your cache type input; and no
        /// other media web types are considered for separate caching.
        /// </summary>
        Task<BitmapImageData> GetFromEndpoint(string endpoint, PictureType cacheType, ImageCacheType cacheAsType);

        /// <summary>
        /// Returns default image source
        /// </summary>
        BitmapImageData GetDefaultImage(ImageCacheType cacheAsType);
    }
}

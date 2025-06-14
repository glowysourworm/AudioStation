using System.Net.Http;
using System.Windows.Media;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

using TagLib;

namespace AudioStation.Controller
{
    [IocExport(typeof(IImageCacheController))]
    public class ImageCacheController : IImageCacheController
    {
        readonly IModelController _modelController;
        readonly IBitmapConverter _bitmapConverter;
        readonly IOutputController _outputController;

        private const int FULL_CACHE_MAX_ENTRIES = 30;
        private const int MEDIUM_CACHE_MAX_ENTRIES = 50;
        private const int SMALL_CACHE_MAX_ENTRIES = 5000;
        private const int THUMBNAIL_CACHE_MAX_ENTRIES = 20000;

        private const int WEB_CACHE_MAX_ENTRIES = 300;              // Currently, one virtual scroller

        protected ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem> ArtistCacheSet;
        protected ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem> AlbumCacheSet;

        protected ImageCacheSet<ImageCacheType, string, ImageCacheItem> WebImageCacheSet;

        [IocImportingConstructor]
        public ImageCacheController(IModelController modelController,
                                    IBitmapConverter bitmapConverter,
                                    IOutputController outputController)
        {
            _modelController = modelController;
            _bitmapConverter = bitmapConverter;
            _outputController = outputController;

            this.ArtistCacheSet = new ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem>();
            this.AlbumCacheSet = new ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem>();

            // TASK THREAD ONLY!
            this.WebImageCacheSet = new ImageCacheSet<ImageCacheType, string, ImageCacheItem>();

            // Add Caches
            this.ArtistCacheSet.AddCache(ImageCacheType.Thumbnail, new ImageCache<ImageCacheKey, ImageCacheItem>(THUMBNAIL_CACHE_MAX_ENTRIES));
            this.ArtistCacheSet.AddCache(ImageCacheType.Small, new ImageCache<ImageCacheKey, ImageCacheItem>(SMALL_CACHE_MAX_ENTRIES));
            this.ArtistCacheSet.AddCache(ImageCacheType.Medium, new ImageCache<ImageCacheKey, ImageCacheItem>(MEDIUM_CACHE_MAX_ENTRIES));
            this.ArtistCacheSet.AddCache(ImageCacheType.FullSize, new ImageCache<ImageCacheKey, ImageCacheItem>(FULL_CACHE_MAX_ENTRIES));

            this.AlbumCacheSet.AddCache(ImageCacheType.Thumbnail, new ImageCache<ImageCacheKey, ImageCacheItem>(THUMBNAIL_CACHE_MAX_ENTRIES));
            this.AlbumCacheSet.AddCache(ImageCacheType.Small, new ImageCache<ImageCacheKey, ImageCacheItem>(SMALL_CACHE_MAX_ENTRIES));
            this.AlbumCacheSet.AddCache(ImageCacheType.Medium, new ImageCache<ImageCacheKey, ImageCacheItem>(MEDIUM_CACHE_MAX_ENTRIES));
            this.AlbumCacheSet.AddCache(ImageCacheType.FullSize, new ImageCache<ImageCacheKey, ImageCacheItem>(FULL_CACHE_MAX_ENTRIES));

            this.WebImageCacheSet.AddCache(ImageCacheType.Thumbnail, new ImageCache<string, ImageCacheItem>(THUMBNAIL_CACHE_MAX_ENTRIES));
            this.WebImageCacheSet.AddCache(ImageCacheType.Small, new ImageCache<string, ImageCacheItem>(SMALL_CACHE_MAX_ENTRIES));
            this.WebImageCacheSet.AddCache(ImageCacheType.Medium, new ImageCache<string, ImageCacheItem>(MEDIUM_CACHE_MAX_ENTRIES));
            this.WebImageCacheSet.AddCache(ImageCacheType.FullSize, new ImageCache<string, ImageCacheItem>(FULL_CACHE_MAX_ENTRIES));
        }

        public ImageSource GetForArtist(int artistId, ImageCacheType cacheAsType)
        {
            try
            {
                return Get(artistId, cacheAsType, true);
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error loading bitmaps for artist:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public ImageSource GetForAlbum(int albumId, ImageCacheType cacheAsType)
        {
            try
            {
                return Get(albumId, cacheAsType, false);
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error loading bitmaps for album:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public ImageSource GetFromEndpoint(string endpoint, PictureType cacheType, ImageCacheType cacheAsType)
        {
            try
            {
                // TASK THREAD ONLY!
                    if (this.WebImageCacheSet.GetCache(cacheAsType).Contains(endpoint))
                        return this.WebImageCacheSet.GetCache(cacheAsType).Get(endpoint).GetFirstImage();

                var data = GetWebImage(endpoint);

                if (data == null)
                    return null;

                var imageSource = _bitmapConverter.BitmapDataToBitmapSource(data, new ImageSize(cacheAsType));
                
                this.WebImageCacheSet.GetCache(cacheAsType).Add(endpoint, new ImageCacheItem(cacheType, imageSource));

                return imageSource;
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error loading web image:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        private byte[]? GetWebImage(string endpoint)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var result = client.GetByteArrayAsync(endpoint).ConfigureAwait(false).GetAwaiter();

                    return result.GetResult();
                }
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error connecting to web image:  {0}", LogMessageType.General, LogLevel.Error, endpoint);
                _outputController.AddLog("Error trying to get web image:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        private ImageSource Get(int entityId, ImageCacheType cacheAsType, bool forArtist)
        {
            var cacheKey = CreateKey(entityId, cacheAsType);

            if (forArtist)
            {
                if (this.ArtistCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                    return this.ArtistCacheSet.GetCache(cacheAsType).Get(cacheKey).GetArtistImage();
            }
            else
            {
                if (this.AlbumCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                    return this.AlbumCacheSet.GetCache(cacheAsType).Get(cacheKey).GetAlbumImage();
            }

            // Fetch the mp3 files for this artist
            var files = forArtist ? _modelController.GetArtistFiles(entityId) : _modelController.GetAlbumTracks(entityId);

            // Take all the artwork - consolidating the images
            var images = files.Select(entity => TagLib.File.Create(entity.FileName))
                              .Where(tagRef => !tagRef.PossiblyCorrupt)
                              .SelectMany(tagRef => tagRef.Tag.Pictures)
                              .DistinctBy(picture => picture.Type);                     // See Enumeration

            // Convert all images
            Dictionary<PictureType, ImageSource> imageSources;

            // Contention for web image loading (Task)
            imageSources = images.ToDictionary(picture => picture.Type,
                                                picture => (ImageSource)_bitmapConverter.BitmapDataToBitmapSource(picture.Data.Data, new ImageSize(cacheAsType)));

            var cacheItem = new ImageCacheItem(imageSources);

            // Cache the result
            if (forArtist)
                this.ArtistCacheSet.GetCache(cacheAsType).Add(cacheKey, cacheItem);
            else
                this.AlbumCacheSet.GetCache(cacheAsType).Add(cacheKey, cacheItem);

            return forArtist ? cacheItem.GetArtistImage() : cacheItem.GetAlbumImage();
        }

        private ImageCacheKey CreateKey(int entityId, ImageCacheType cacheAsType)
        {
            return new ImageCacheKey(entityId, new ImageSize(cacheAsType));
        }
    }
}

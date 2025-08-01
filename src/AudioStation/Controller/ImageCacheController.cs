﻿using System.IO;
using System.Net.Http;
using System.Reflection;

using AudioStation.Component.Interface;
using AudioStation.Component.Model;
using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.SimpleCollections.Collection;

using PictureType = ATL.PictureInfo.PIC_TYPE;

namespace AudioStation.Controller
{
    [IocExport(typeof(IImageCacheController))]
    public class ImageCacheController : IImageCacheController
    {
        readonly IModelController _modelController;
        readonly IBitmapConverter _bitmapConverter;
        readonly IOutputController _outputController;
        readonly ITagCacheController _tagCacheController;

        private const int FULL_CACHE_MAX_ENTRIES = 30;
        private const int MEDIUM_CACHE_MAX_ENTRIES = 50;
        private const int SMALL_CACHE_MAX_ENTRIES = 5000;
        private const int THUMBNAIL_CACHE_MAX_ENTRIES = 20000;

        private const int WEB_CACHE_MAX_ENTRIES = 300;              // Currently, one virtual scroller

        protected ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem> ArtistCacheSet;
        protected ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem> AlbumCacheSet;
        protected ImageCacheSet<ImageCacheType, string, ImageCacheItem> WebImageCacheSet;

        protected SimpleDictionary<ImageCacheType, BitmapImageData> DefaultImageCache { get; private set; }

        private object _artistLock = new object();
        private object _albumLock = new object();
        private object _webLock = new object();

        private HttpClient _httpClient;

        [IocImportingConstructor]
        public ImageCacheController(IModelController modelController,
                                    IBitmapConverter bitmapConverter,
                                    IOutputController outputController,
                                    ITagCacheController tagCacheController)
        {
            _modelController = modelController;
            _bitmapConverter = bitmapConverter;
            _outputController = outputController;
            _tagCacheController = tagCacheController;

            this.ArtistCacheSet = new ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem>();
            this.AlbumCacheSet = new ImageCacheSet<ImageCacheType, ImageCacheKey, ImageCacheItem>();

            // TASK THREAD ONLY!
            this.WebImageCacheSet = new ImageCacheSet<ImageCacheType, string, ImageCacheItem>();

            // Default image
            this.DefaultImageCache = new SimpleDictionary<ImageCacheType, BitmapImageData>();

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

            _httpClient = new HttpClient();

            Initialize();
        }

        private void Initialize()
        {
            // Can't (yet) resize PNG image using GDI. So, these were bilinear resized before embedding them as resources. Let them get transformed
            // as "FullSized"; but cache them as their destination size.
            //
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.Resources.Images.placeholder_FullSized.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.FullSize, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.Resources.Images.placeholder_Medium.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.Medium, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.Resources.Images.placeholder_Small.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.Small, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.Resources.Images.placeholder_Thumbnail.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.Thumbnail, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
        }

        public async Task<BitmapImageData> GetForArtist(int artistId, ImageCacheType cacheAsType)
        {
            try
            {
                return await Get(artistId, cacheAsType, true);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading bitmaps for artist:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
            }

            return this.DefaultImageCache[cacheAsType];
        }

        public async Task<BitmapImageData> GetForAlbum(int albumId, ImageCacheType cacheAsType)
        {
            try
            {
                return await Get(albumId, cacheAsType, false);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading bitmaps for album:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
            }

            return this.DefaultImageCache[cacheAsType];
        }

        public BitmapImageData GetDefaultImage(ImageCacheType cacheAsType)
        {
            return this.DefaultImageCache[cacheAsType];
        }

        public async Task<BitmapImageData> GetFromEndpoint(string endpoint, PictureType cacheType, ImageCacheType cacheAsType)
        {
            try
            {
                // TASK THREAD ONLY!
                lock (_webLock)
                {
                    if (this.WebImageCacheSet.GetCache(cacheAsType).Contains(endpoint))
                        return this.WebImageCacheSet.GetCache(cacheAsType).Get(endpoint).GetFirstImage();
                }

                var data = await GetWebImage(endpoint);

                if (data == null)
                    return this.DefaultImageCache[cacheAsType];

                var imageSource = _bitmapConverter.BitmapDataToBitmapSource(data, new ImageSize(cacheAsType), null);

                // Two threads may have entered the critical section before this point
                //
                lock (_webLock)
                {
                    if (!this.WebImageCacheSet.GetCache(cacheAsType).Contains(endpoint))
                        this.WebImageCacheSet.GetCache(cacheAsType).Add(endpoint, new ImageCacheItem(cacheType, imageSource));
                }


                return imageSource;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading web image:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
            }

            return this.DefaultImageCache[cacheAsType];
        }

        private async Task<byte[]?> GetWebImage(string endpoint)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(endpoint);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error connecting to web image:  {0}", LogMessageType.General, LogLevel.Error, ex, endpoint);
                ApplicationHelpers.Log("Error trying to get web image:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
            }

            return null;
        }

        private async Task<BitmapImageData> Get(int entityId, ImageCacheType cacheAsType, bool forArtist)
        {
            var cacheKey = CreateKey(entityId, cacheAsType);

            if (forArtist)
            {
                lock (_artistLock)
                {
                    if (this.ArtistCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        return this.ArtistCacheSet.GetCache(cacheAsType).Get(cacheKey).GetArtistImage();
                }
            }
            else
            {
                lock (_albumLock)
                {
                    if (this.AlbumCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        return this.AlbumCacheSet.GetCache(cacheAsType).Get(cacheKey).GetAlbumImage();
                }
            }

            // Fetch the mp3 files for this artist
            var files = forArtist ? _modelController.GetArtistFiles(entityId) : _modelController.GetAlbumTracks(entityId);

            // Take all the artwork - consolidating the images
            var images = files.Select(entity => _tagCacheController.Get(entity.FileName))
                              .Where(tagRef => tagRef != null)                              // TODO: Application Level Validation (Library Maintenance)
                              .SelectMany(tagRef => tagRef.EmbeddedPictures)
                              .DistinctBy(picture => picture.PicType);                         // See Enumeration

            // Convert all images
            Dictionary<PictureType, BitmapImageData> imageSources;

            // Contention for web image loading (Task)
            imageSources = images.ToDictionary(picture => picture.PicType,
                                               picture => (BitmapImageData)_bitmapConverter.BitmapDataToBitmapSource(picture.PictureData, new ImageSize(cacheAsType), picture.MimeType));

            var cacheItem = new ImageCacheItem(imageSources);

            // Cache the result
            if (forArtist)
            {
                lock (_artistLock)
                {
                    if (!this.ArtistCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        this.ArtistCacheSet.GetCache(cacheAsType).Add(cacheKey, cacheItem);
                }
            }
            else
            {
                lock (_albumLock)
                {
                    if (!this.AlbumCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        this.AlbumCacheSet.GetCache(cacheAsType).Add(cacheKey, cacheItem);
                }
            }

            return forArtist ? cacheItem.GetArtistImage() : cacheItem.GetAlbumImage();
        }

        private ImageCacheKey CreateKey(int entityId, ImageCacheType cacheAsType)
        {
            return new ImageCacheKey(entityId, new ImageSize(cacheAsType));
        }
    }
}

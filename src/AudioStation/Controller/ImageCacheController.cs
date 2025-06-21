using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.SimpleCollections.Collection;

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

        protected SimpleDictionary<ImageCacheType, ImageSource> DefaultImageCache { get; private set; }

        private object _artistLock = new object();
        private object _albumLock = new object();
        private object _webLock = new object();

        private HttpClient _httpClient;

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

            // Default image
            this.DefaultImageCache = new SimpleDictionary<ImageCacheType, ImageSource>();

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
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.placeholder_FullSized.png"))
            {
                using (var memoryStream =  new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.FullSize, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.placeholder_Medium.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.Medium, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.placeholder_Small.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.Small, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
            using (var stream = Assembly.GetAssembly(typeof(ImageCacheController)).GetManifestResourceStream("AudioStation.placeholder_Thumbnail.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    this.DefaultImageCache.Add(ImageCacheType.Thumbnail, _bitmapConverter.BitmapDataToBitmapSource(memoryStream.GetBuffer(), new ImageSize(ImageCacheType.FullSize), "image/png"));
                }
            }
        }

        public async Task<ImageSource> GetForArtist(int artistId, ImageCacheType cacheAsType)
        {
            try
            {
                return await Get(artistId, cacheAsType, true);
            }
            catch (Exception ex)
            {
                RaiseLog("Error loading bitmaps for artist:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return this.DefaultImageCache[cacheAsType];
        }

        public async Task<ImageSource> GetForAlbum(int albumId, ImageCacheType cacheAsType)
        {
            try
            {
                return await Get(albumId, cacheAsType, false);
            }
            catch (Exception ex)
            {
                RaiseLog("Error loading bitmaps for album:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return this.DefaultImageCache[cacheAsType];
        }

        public ImageSource GetDefaultImage(ImageCacheType cacheAsType)
        {
            return this.DefaultImageCache[cacheAsType];
        }

        public async Task<ImageSource> GetFromEndpoint(string endpoint, PictureType cacheType, ImageCacheType cacheAsType)
        {
            try
            {
                // TASK THREAD ONLY!
                lock(_webLock)
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
                lock(_webLock)
                {
                    if (!this.WebImageCacheSet.GetCache(cacheAsType).Contains(endpoint))
                        this.WebImageCacheSet.GetCache(cacheAsType).Add(endpoint, new ImageCacheItem(cacheType, imageSource));
                }
                

                return imageSource;
            }
            catch (Exception ex)
            {
                RaiseLog("Error loading web image:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
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
                RaiseLog("Error connecting to web image:  {0}", LogMessageType.General, LogLevel.Error, endpoint);
                RaiseLog("Error trying to get web image:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        private async Task<ImageSource> Get(int entityId, ImageCacheType cacheAsType, bool forArtist)
        {
            var cacheKey = CreateKey(entityId, cacheAsType);

            if (forArtist)
            {
                lock(_artistLock)
                {
                    if (this.ArtistCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        return this.ArtistCacheSet.GetCache(cacheAsType).Get(cacheKey).GetArtistImage();
                }
            }
            else
            {
                lock(_albumLock)
                {
                    if (this.AlbumCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        return this.AlbumCacheSet.GetCache(cacheAsType).Get(cacheKey).GetAlbumImage();
                }
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
                                                picture => (ImageSource)_bitmapConverter.BitmapDataToBitmapSource(picture.Data.Data, new ImageSize(cacheAsType), picture.MimeType));

            var cacheItem = new ImageCacheItem(imageSources);

            // Cache the result
            if (forArtist)
            {
                lock(_artistLock)
                {
                    if (!this.ArtistCacheSet.GetCache(cacheAsType).Contains(cacheKey))
                        this.ArtistCacheSet.GetCache(cacheAsType).Add(cacheKey, cacheItem);
                }
            }
            else
            {
                lock(_albumLock)
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

        private void RaiseLog(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(RaiseLog, DispatcherPriority.Background, message, type, level, parameters);
            else
            {
                _outputController.AddLog(message, type, level, parameters);
            }
        }
    }
}

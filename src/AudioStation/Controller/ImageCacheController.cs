using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Media;

using AudioStation.Component;
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

        protected SimpleDictionary<ImageCacheKey, ImageCacheItem> CacheByArtist { get; private set; }
        protected SimpleDictionary<ImageCacheKey, ImageCacheItem> CacheByAlbum { get; private set; }

        protected SimpleDictionary<int, ImageCacheItem> ArtistThumbnails { get; private set; }
        protected SimpleDictionary<int, ImageCacheItem> AlbumThumbnails { get; private set; }

        protected SimpleDictionary<string, ImageSource> WebImageCache { get; private set;}

        private object _bitmapConverterLock = new object();

        [IocImportingConstructor]
        public ImageCacheController(IModelController modelController,
                                    IBitmapConverter bitmapConverter,
                                    IOutputController outputController)
        {
            _modelController = modelController;
            _bitmapConverter = bitmapConverter;
            _outputController = outputController;

            this.CacheByAlbum = new SimpleDictionary<ImageCacheKey, ImageCacheItem>();
            this.CacheByArtist = new SimpleDictionary<ImageCacheKey, ImageCacheItem>();
            this.ArtistThumbnails = new SimpleDictionary<int, ImageCacheItem>();
            this.AlbumThumbnails = new SimpleDictionary<int, ImageCacheItem>();

            // TASK THREAD ONLY!
            this.WebImageCache = new SimpleDictionary<string, ImageSource>();
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

        public Task<ImageSource> GetFromEndpoint(string endpoint, PictureType cacheType, ImageCacheType cacheAsType)
        {
            try
            {
                return Task.Run<ImageSource>(async () =>
                {
                    // TASK THREAD ONLY!
                    if (this.WebImageCache.ContainsKey(endpoint))
                        return this.WebImageCache[endpoint];

                    var data = await GetWebImage(endpoint);

                    if (data == null)
                        return null;

                    ImageSource imageSource = null;

                    lock(_bitmapConverterLock)
                    {
                        imageSource = _bitmapConverter.BitmapDataToBitmapSource(data, new ImageSize(cacheAsType));
                    }

                    this.WebImageCache.Add(endpoint, imageSource);

                    return imageSource;
                });
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error loading web image:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        private async Task<byte[]?> GetWebImage(string endpoint)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.GetByteArrayAsync(endpoint);

                client.Dispose();
                client = null;
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

            if (this.CacheByArtist.ContainsKey(cacheKey))
                return this.CacheByArtist[cacheKey].GetArtistImage();

            // Fetch the mp3 files for this artist
            var files = forArtist ? _modelController.GetArtistFiles(entityId) : null;

            // Take all the artwork - consolidating the images
            var images = files.Select(entity => TagLib.File.Create(entity.FileName))
                              .Where(tagRef => !tagRef.PossiblyCorrupt)
                              .SelectMany(tagRef => tagRef.Tag.Pictures)
                              .DistinctBy(picture => picture.Type);                     // See Enumeration

            // Convert all images
            Dictionary<PictureType, ImageSource> imageSources;

            // Contention for web image loading (Task)
            lock(_bitmapConverterLock)
            {
                imageSources = images.ToDictionary(picture => picture.Type,
                                                   picture => (ImageSource)_bitmapConverter.BitmapDataToBitmapSource(picture.Data.Data, new ImageSize(cacheAsType)));
            }

            var cacheItem = new ImageCacheItem(imageSources);

            // Cache the result
            if (forArtist)
                this.CacheByArtist.Add(cacheKey, cacheItem);
            else
                this.CacheByAlbum.Add(cacheKey, cacheItem);

            return forArtist ? cacheItem.GetArtistImage() : cacheItem.GetAlbumImage();
        }

        private ImageCacheKey CreateKey(int entityId, ImageCacheType cacheAsType)
        {
            return new ImageCacheKey(entityId, new ImageSize(cacheAsType));
        }
    }
}

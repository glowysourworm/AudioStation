using System.IO;
using System.Windows;
using System.Windows.Media;

using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Component
{
    // TODO: move this to DI using IocFramework
    public static class LibraryImageCache
    {
        private static readonly int FULL_CACHE_MAX_ENTRIES = 30;

        private class ImageCache
        {
            public readonly IEnumerable<ImageSource> Images;
            public readonly string FileName;

            public ImageCache(string fileName, IEnumerable<ImageSource> images)
            {
                this.Images = images;
                this.FileName = fileName;
            }
        }

        /// <summary>
        /// Cache of image sources by file name. These are read using the TagLib.
        /// </summary>
        private static SimpleDictionary<string, ImageCache> FullCache;

        static LibraryImageCache()
        {
            FullCache = new SimpleDictionary<string, ImageCache>();
        }

        public static IEnumerable<ImageSource> Get(string fileName, int desiredWidth, int desiredHeight)
        {
            if (FullCache.ContainsKey(fileName))
            {
                return FullCache[fileName].Images;
            }

            Load(fileName, desiredWidth, desiredHeight);

            return FullCache[fileName].Images;
        }

        private static IEnumerable<ImageSource> Load(string fileName, int desiredWidth, int desiredHeight)
        {
            // TagLib:  Will open the mp3 file and load all of the artwork
            var fileRef = TagLib.File.Create(fileName);
            var images = new List<ImageSource>();

            foreach (var picture in fileRef.Tag.Pictures)
            {
                using (var stream = new MemoryStream(picture.Data.Data))
                {
                    var bitmapSource = BitmapConverter.BitmapDataToBitmapSource(picture.Data.Data, desiredWidth, desiredHeight);

                    if (bitmapSource != null)
                    {
                        bitmapSource.Freeze();

                        images.Add(bitmapSource);
                    }
                }
            }

            // Max Entries (Full) (Thumbnail cache might be useful)
            if (FullCache.Count > FULL_CACHE_MAX_ENTRIES)
                FullCache.Remove(FullCache.Keys.Last());

            FullCache.Add(fileName, new ImageCache(fileName, images));

            return images;
        }


    }
}

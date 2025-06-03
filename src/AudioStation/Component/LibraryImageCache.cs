using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SimpleWpf.SimpleCollections.Collection;

using TagLib;

namespace AudioStation.Component
{
    // TODO: move this to DI using IocFramework
    public static class LibraryImageCache
    {
        private static readonly int FULL_CACHE_MAX_ENTRIES = 50;

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

        public static IEnumerable<ImageSource> Get(string fileName)
        {
            /*
            if (FullCache.ContainsKey(fileName))
            {
                return FullCache[fileName].Images;
            }
            */

            return Load(fileName);

            //return FullCache[fileName].Images;
        }

        private static IEnumerable<ImageSource> Load(string fileName)
        {
            // TagLib:  Will open the mp3 file and load all of the artwork
            var fileRef = TagLib.File.Create(fileName);
            var images = new List<ImageSource>();

            foreach (var picture in fileRef.Tag.Pictures)
            {
                using (var stream = new MemoryStream(picture.Data.Data))
                {
                    var bitmap = new Bitmap(stream, false);
                    var bitmapSource = BitmapToBitmapSource(bitmap);

                    if (bitmapSource != null)
                    {
                        bitmapSource.Freeze();

                        images.Add(bitmapSource);
                    }
                    

                    /*
                    BitmapDecoder decoder = null;

                    switch (picture.MimeType)
                    {
                        case "image/bmp":
                            decoder = BmpBitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case "image/jpeg":
                        case "image/jpg":
                            decoder = JpegBitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case "image/gif":
                            decoder = GifBitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case "image/png":
                            decoder = PngBitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        default:
                            throw new Exception("Unhandled mp3 picture mime/type:  LibraryImageCache.cs");
                    }

                    foreach (var frame in decoder.Frames)
                    {
                        images.Add(frame);
                        thumbnails.Add(frame.Thumbnail);
                    }
                    */
                }
            }

            // Max Entries (Full)
            //if (FullCache.Count > FULL_CACHE_MAX_ENTRIES)
            //    FullCache.Remove(FullCache.Keys.Last());

            //FullCache.Add(fileName, new ImageCache(fileName, images));

            return images;
        }

        /// https://stackoverflow.com/a/30729291
        public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
        {
            try
            {
                var bitmapData = bmp.LockBits(
                       new Rectangle(0, 0, bmp.Width, bmp.Height),
                       ImageLockMode.ReadOnly, bmp.PixelFormat);

                // TODO: Handle Pixel Formats
                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bmp.HorizontalResolution, bmp.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bmp.UnlockBits(bitmapData);

                return bitmapSource;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

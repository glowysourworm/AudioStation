using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static IEnumerable<ImageSource> Get(string fileName)
        {
            if (FullCache.ContainsKey(fileName))
            {
                return FullCache[fileName].Images;
            }

            Load(fileName);

            return FullCache[fileName].Images;
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
                }
            }

            // Max Entries (Full) (Thumbnail cache might be useful)
            if (FullCache.Count > FULL_CACHE_MAX_ENTRIES)
                FullCache.Remove(FullCache.Keys.Last());

            FullCache.Add(fileName, new ImageCache(fileName, images));

            return images;
        }

        /// https://stackoverflow.com/a/30729291
        private static BitmapSource BitmapToBitmapSource(Bitmap bmp)
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
                    GetWpfPixelFormat(bmp.PixelFormat), null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bmp.UnlockBits(bitmapData);

                return bitmapSource;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static System.Windows.Media.PixelFormat GetWpfPixelFormat(System.Drawing.Imaging.PixelFormat format)
        {
            switch (format)
            {
                // Many of the old bitmap formats ARE NOT HANDLED! So, there had better be some way of knowing 
                // how to translate these formats!
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return System.Windows.Media.PixelFormats.Indexed1;
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    return System.Windows.Media.PixelFormats.Indexed4;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return System.Windows.Media.PixelFormats.Indexed8;
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return System.Windows.Media.PixelFormats.Gray16;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr24;             // THIS WAS BACKWARDS!!!! CHECK FOR YOURSELF! (RGB != BGR)
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return System.Windows.Media.PixelFormats.Rgb48;
                default:
                    throw new Exception("Unhandled pixel format transfer (GDI -> WPF):  LibraryImageCache");
            }
        }
    }
}

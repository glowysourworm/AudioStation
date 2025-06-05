using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AudioStation.Component
{
    public static class BitmapConverter
    {
        public static BitmapSource BitmapDataToBitmapSource(byte[] buffer)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer))
                {
                    var bitmap = new Bitmap(memoryStream, false);
                    return BitmapToBitmapSource(bitmap);
                }
            }
            catch (Exception)
            {
                return null;
            }
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

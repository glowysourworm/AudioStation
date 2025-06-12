using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AudioStation.Component
{
    public static class BitmapConverter
    {
        public static BitmapSource BitmapDataToBitmapSource(byte[] buffer, int desiredWidth, int desiredHeight)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer))
                {
                    var bitmap = new Bitmap(memoryStream, false);
                    return BitmapToBitmapSource(bitmap, desiredWidth, desiredHeight);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// https://stackoverflow.com/a/30729291
        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap, int width, int height)
        {
            try
            {
                // GDI Graphics:  Apply image interpolation
                //
                // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-use-interpolation-mode-to-control-image-quality-during-scaling
                //
                // Procedure:
                //
                // 1) Create empty image with the desired size
                // 2) Load graphics from the current bitmap
                // 3) Apply interpolation
                // 4) Draw scaled image to the result
                //
                var resultBitmap = new Bitmap(width, height, bitmap.PixelFormat);                
                var graphics = Graphics.FromImage(resultBitmap);

                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(bitmap, 
                                   new Rectangle(0, 0, width, height),
                                   new Rectangle(0,0,bitmap.Width, bitmap.Height), 
                                   GraphicsUnit.Pixel);
                graphics.Flush();
                graphics.Dispose();
                graphics = null;


                var bitmapData = resultBitmap.LockBits(
                       new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.ReadOnly, resultBitmap.PixelFormat);

                // TODO: Handle Pixel Formats
                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    resultBitmap.HorizontalResolution, resultBitmap.VerticalResolution,
                    GetWpfPixelFormat(resultBitmap.PixelFormat), null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                resultBitmap.UnlockBits(bitmapData);

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

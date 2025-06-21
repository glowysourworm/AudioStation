using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

using AudioStation.Component.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component
{
    [IocExport(typeof(IBitmapConverter))]
    public class BitmapConverter : IBitmapConverter
    {
        private readonly IOutputController _outputController;

        [IocImportingConstructor]
        public BitmapConverter(IOutputController outputController)
        {
            _outputController = outputController;
        }

        public BitmapSource BitmapDataToBitmapSource(byte[] buffer, ImageSize imageSize, string mimeType)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer))
                {
                    if (mimeType == "image/bmp")
                    {
                        var bitmap = new Bitmap(memoryStream, false);
                        return BitmapToBitmapSource(bitmap, imageSize);
                    }
                    else if (mimeType == "image/jpeg")
                    {
                        var decoder = JpegBitmapDecoder.Create(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        var bitmap = BitmapFrameToBitmapSource(decoder.Frames[0], imageSize, mimeType);

                        // MEMORY LEAK!
                        decoder = null;

                        return bitmap;
                    } 
                    else if (mimeType == "image/png")
                    {
                        var decoder = PngBitmapDecoder.Create(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        var bitmap = BitmapFrameToBitmapSource(decoder.Frames[0], imageSize, mimeType);

                        // MEMORY LEAK!
                        decoder = null;

                        return bitmap;
                    }
                    else // Default to Bitmap
                    {
                        if (!string.IsNullOrEmpty(mimeType))
                            _outputController.AddLog("Unhandled Bitmap mime/type:  {0}", LogMessageType.General, LogLevel.Warning, mimeType);

                        var bitmap = new Bitmap(memoryStream, false);
                        return BitmapToBitmapSource(bitmap, imageSize);
                    }
                }
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error Converting Bitmap:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                return null;
            }
        }

        public BitmapSource BitmapFrameToBitmapSource(BitmapFrame bitmapFrame, ImageSize imageSize, string mimeType)
        {
            try
            {
                BitmapSource source = null;

                // Save to File / Re-open
                using (var writeStream = new MemoryStream())
                {
                    BitmapEncoder encoder = null;

                    if (mimeType == "image/bmp")
                    {
                        encoder = new BmpBitmapEncoder();
                    }
                    else if (mimeType == "image/jpeg")
                    {
                        encoder = new JpegBitmapEncoder();
                    }
                    else if (mimeType == "image/png")
                    {
                        encoder = new PngBitmapEncoder();
                    }
                    else
                        throw new Exception("Unhandled mime type:  BitmapConverter.cs");
                    
                    encoder.Frames.Add(bitmapFrame);
                    encoder.Save(writeStream);

                    writeStream.Seek(0, SeekOrigin.Begin);

                    var bitmap = new Bitmap(writeStream, false);
                    source = BitmapToBitmapSource(bitmap, imageSize);

                    encoder = null;

                    writeStream.Flush();
                    writeStream.Dispose();
                }

                return source;
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error Converting Bitmap:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                return null;
            }
        }

        /// https://stackoverflow.com/a/30729291
        public BitmapSource BitmapToBitmapSource(Bitmap bitmap, ImageSize imageSize)
        {
            try
            {
                // GDI Graphics:  Apply image interpolation
                //
                // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-use-interpolation-mode-to-control-image-quality-during-scaling
                //

                // Scale bitmap to result size (this accounts for memory)
                var resultBitmap = ScaleBitmap(bitmap, imageSize);

                var bitmapData = resultBitmap.LockBits(
                       new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.ReadOnly, resultBitmap.PixelFormat);

                // TODO: Handle Pixel Formats (see output message log to find errors)
                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    resultBitmap.HorizontalResolution, resultBitmap.VerticalResolution,
                    GetWpfPixelFormat(resultBitmap.PixelFormat), GetWpfPalette(bitmap.Palette),
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                resultBitmap.UnlockBits(bitmapData);

                return bitmapSource;
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error Converting Bitmap:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                return null;
            }
        }

        private Bitmap ScaleBitmap(Bitmap bitmap, ImageSize imageSize)
        {
            // Leave full sized bitmaps as is
            if (imageSize.IsFullSized())
                return bitmap;

            // Procedure:
            //
            // 1) Create empty image with the desired size
            // 2) Load graphics from the current bitmap
            // 3) Apply interpolation
            // 4) Draw scaled image to the result
            //

            var resultBitmap = new Bitmap(imageSize.Width, imageSize.Height, bitmap.PixelFormat);
            var graphics = Graphics.FromImage(resultBitmap);

            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(bitmap,
                               new Rectangle(0, 0, imageSize.Width, imageSize.Height),
                               new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                               GraphicsUnit.Pixel);
            graphics.Flush();
            graphics.Dispose();
            graphics = null;

            return resultBitmap;
        }

        private System.Windows.Media.PixelFormat GetWpfPixelFormat(System.Drawing.Imaging.PixelFormat format)
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
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return System.Windows.Media.PixelFormats.Bgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    return System.Windows.Media.PixelFormats.Pbgra32;
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return System.Windows.Media.PixelFormats.Rgb48;
                default:
                    throw new Exception("Unhandled pixel format transfer (GDI -> WPF):  " + format.ToString());
            }
        }

        private System.Windows.Media.Imaging.BitmapPalette GetWpfPalette(System.Drawing.Imaging.ColorPalette palette)
        {
            if (palette == null)
                return null;

            if (palette.Entries.Length <= 1)
                return null;

            return new BitmapPalette(palette.Entries.Select(x => System.Windows.Media.Color.FromArgb(x.A, x.R, x.G, x.B)).ToList());
        }
    }
}

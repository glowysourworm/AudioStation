using System.Drawing;

using AudioStation.Component.Model;
using AudioStation.Controller.Model;

namespace AudioStation.Component.Interface
{
    /// <summary>
    /// This component is responsible for building BitmapSource objects that are less-prone to memory
    /// leaks. This uses the GDI framework; and also supports scaling modes for bitmaps.
    /// </summary>
    public interface IBitmapConverter
    {
        BitmapImageData BitmapDataToBitmapSource(byte[] buffer, ImageSize size, string mimeType);

        /// https://stackoverflow.com/a/30729291
        BitmapImageData BitmapToBitmapSource(Bitmap bitmap, ImageSize size);
    }
}

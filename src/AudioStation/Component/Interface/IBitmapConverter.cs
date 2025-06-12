using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using AudioStation.Controller.Model;

namespace AudioStation.Component.Interface
{
    /// <summary>
    /// This component is responsible for building BitmapSource objects that are less-prone to memory
    /// leaks. This uses the GDI framework; and also supports scaling modes for bitmaps.
    /// </summary>
    public interface IBitmapConverter
    {
        BitmapSource BitmapDataToBitmapSource(byte[] buffer, ImageSize size);

        /// https://stackoverflow.com/a/30729291
        BitmapSource BitmapToBitmapSource(Bitmap bitmap, ImageSize size);
    }
}

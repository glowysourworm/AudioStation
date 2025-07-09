using System.Windows.Media.Imaging;

namespace AudioStation.Component.Model
{
    public class BitmapImageData
    {
        /// <summary>
        /// Prepared source for the image
        /// </summary>
        public BitmapSource Source { get; private set; }

        /// <summary>
        /// Copied buffer for saving to file (not part of WPF's standard ImageSource API to expose the buffer)
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Stride of the bitmap color array
        /// </summary>
        public int Stride { get; private set; }

        public BitmapImageData(BitmapSource source, byte[] buffer, int stride)
        {
            this.Source = source;
            this.Buffer = buffer;
            this.Stride = stride;
        }
    }
}

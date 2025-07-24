using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AudioStation.Component.Model
{
    public class BitmapImageData : IDisposable
    {
        /// <summary>
        /// Prepared source for the image
        /// </summary>
        public BitmapSource Source { get; private set; }

        public BitmapImageData(BitmapSource source)
        {
            this.Source = source;
        }

        public void Save()
        {
            // Use the bitmap encoder to save the byte[]. Otherwise, there are pixel format calculations
            // to consider; and I'd rather not even use WPF's API. If there is a memory leak here - we'll
            // just fix it.
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                var frame = BitmapFrame.Create(this.Source);
                encoder.Frames.Add(frame);
                encoder.Save(memoryStream);
            }
        }

        public byte[] GetBuffer()
        {
            // See above comment...
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                var frame = BitmapFrame.Create(this.Source);
                encoder.Frames.Add(frame);
                encoder.Save(memoryStream);

                return memoryStream.GetBuffer();
            }
        }

        public void Dispose()
        {
            this.Source = null;
        }
    }
}

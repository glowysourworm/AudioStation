using System.IO;

namespace AudioStation.Core.Model
{
    public class Image
    {
        public byte[] Buffer { get; set; }
        public string MimeType { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }

        public void SetFrom(Stream stream, string mimeType)
        {
            if (stream is MemoryStream)
            {
                this.Buffer = (stream as MemoryStream).GetBuffer();
                this.MimeType = mimeType;
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    this.Buffer = memoryStream.GetBuffer();
                    this.MimeType = mimeType;
                }
            }
        }

        public Image() { }
        public Image(Stream stream, string mimeType)
        {
            SetFrom(stream, mimeType);
        }
    }
}

using System.IO;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels
{
    public class ImageViewModel : ViewModelBase
    {
        byte[] _buffer;
        string _mimeType;

        public byte[] Buffer
        {
            get { return _buffer; }
            set { this.RaiseAndSetIfChanged(ref _buffer, value); }
        }
        public string MimeType
        {
            get { return _mimeType; }
            set { this.RaiseAndSetIfChanged(ref _mimeType, value); }
        }

        public void SetFrom(Stream stream, string mimeType)
        {
            if (stream is MemoryStream)
            {
                _buffer = (stream as MemoryStream).GetBuffer();
                _mimeType = mimeType;
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    _buffer = memoryStream.GetBuffer();
                    _mimeType = mimeType;
                }
            }
        }
        public void SetFrom(byte[] buffer, string mimeType)
        {
            _buffer = buffer;
            _mimeType = mimeType;
        }

        public ImageViewModel()
        {
            this.Buffer = new byte[0];
            this.MimeType = string.Empty;
        }
        public ImageViewModel(Stream stream, string mimeType)
        {
            SetFrom(stream, mimeType);
        }
    }
}

using System.ComponentModel;
using System.IO;

using IdSharp.Tagging.ID3v2;
using IdSharp.Tagging.ID3v2.Frames;

using SixLabors.ImageSharp;

namespace AudioStation.Core.Model.Vendor.IdSharp
{
    public class AttachedPicture : IAttachedPicture
    {
        public string MimeType { get; set; }
        public PictureType PictureType { get; set; }
        public string Description { get; set; }
        public byte[] PictureData { get; set; }
        public Image Picture { get; set; }
        public string PictureExtension { get; }
        public IFrameHeader FrameHeader { get; }
        public EncodingType TextEncoding { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public byte[] GetBytes(ID3v2TagVersion tagVersion)
        {
            throw new NotImplementedException();
        }

        public string GetFrameID(ID3v2TagVersion tagVersion)
        {
            throw new NotImplementedException();
        }

        public void Read(TagReadingInfo tagReadingInfo, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

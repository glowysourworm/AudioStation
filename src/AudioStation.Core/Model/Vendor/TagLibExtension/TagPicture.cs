using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

using TagLib;

namespace AudioStation.Core.Model.Vendor.TagLibExtension
{
    public class TagPicture : IPicture, IRecursiveSerializable
    {
        public string MimeType { get; set; }
        public PictureType Type { get; set; }
        public string Description { get; set; }
        public ByteVector Data { get; set; }

        public TagPicture()
        {
            this.MimeType = string.Empty;
            this.Type = PictureType.Other;
            this.Description = string.Empty;
            this.Data = new ByteVector();
        }
        public TagPicture(IPicture picture)
        {
            this.MimeType = picture.MimeType;
            this.Type = picture.Type;
            this.Description = picture.Description;
            this.Data = new ByteVector(picture.Data);
        }
        public TagPicture(IPropertyReader reader)
        {
            this.MimeType = reader.Read<string>("MimeType");
            this.Type = reader.Read<PictureType>("Type");
            this.Description = reader.Read<string>("Description");

            var buffer = reader.Read<List<byte>>("Data")?.ToArray() ?? new byte[] { };

            this.Data = new ByteVector(buffer);
        }

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("MimeType", this.MimeType);
            writer.Write("Type", this.Type);
            writer.Write("Description", this.Description);
            writer.Write("Data", this.Data.Data.ToList());
        }
    }
}

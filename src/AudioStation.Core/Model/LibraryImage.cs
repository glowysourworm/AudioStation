using IdSharp.Tagging.ID3v2;

namespace AudioStation.Core.Model
{
    public class LibraryImage
    {
        /// <summary>
        /// Unique ID for the image: this will greatly help performance (not to have duplicate pictures cached). So, we will
        /// make a readonly hash code of the image data in the constructor.
        /// </summary>
        public int Id { get; private set; }
        public PictureType PictureType { get; private set; }
        public string MimeType { get; private set; }
        public string Description { get; private set; }
        public byte[] Data { get; private set; }

        public LibraryImage(PictureType type, string mimeType, string description, byte[] data)
        {
            this.MimeType = mimeType ?? string.Empty;
            this.PictureType = type;
            this.Description = description ?? string.Empty;
            this.Data = data ?? new byte[0];

            this.Id = CalculateHash();
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        private int CalculateHash()
        {
            var hash = HashCode.Combine(this.MimeType, this.PictureType, this.Description);

            foreach (var dataByte in this.Data)
            {
                hash = HashCode.Combine(hash, dataByte);
            }

            return hash;
        }

        public override string ToString()
        {
            return string.Format("Id={0} Data Length={1}", this.Id, this.Data.Length);
        }
    }
}

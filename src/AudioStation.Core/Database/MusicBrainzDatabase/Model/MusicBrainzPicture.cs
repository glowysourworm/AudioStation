using TagLib;

namespace AudioStation.Core.Database.MusicBrainzDatabase.Model
{
    public class MusicBrainzPicture : IPicture
    {
        public string MimeType { get; set; }
        public PictureType Type { get; set; }
        public string Description { get; set; }
        public ByteVector Data { get; set; }
    }
}

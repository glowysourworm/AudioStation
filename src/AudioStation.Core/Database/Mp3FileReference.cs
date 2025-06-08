using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("Mp3FileReference", Schema = "public")]
    public class Mp3FileReference
    {
        public int Id { get; set; }

        [ForeignKey("Album")]
        public int? AlbumId { get; set; }

        [ForeignKey("PrimaryArtist")]
        public int? PrimaryArtistId { get; set; }
        public string FileName { get; set; }
        public string? Title { get; set; }
        public int? Track { get; set; }

        // Relationship properties

        public Mp3FileReferenceAlbum? Album { get; set; }
        public Mp3FileReferenceArtist? PrimaryArtist { get; set; }

        public Mp3FileReference() { }
    }
}

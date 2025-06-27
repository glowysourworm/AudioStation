using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database
{
    [PrimaryKey("Id")]
    [Table("Mp3FileReference", Schema = "public")]
    public class Mp3FileReference : EntityBase
    {        
        [ForeignKey("Album")]
        public int? AlbumId { get; set; }

        [ForeignKey("PrimaryArtist")]
        public int? PrimaryArtistId { get; set; }

        [ForeignKey("PrimaryGenre")]
        public int? PrimaryGenreId { get; set; }
        public string FileName { get; set; }
        public string? Title { get; set; }
        public int? Track { get; set; }
        public int? DurationMilliseconds { get; set; }

        public bool IsFileAvailable { get; set; }
        public bool IsFileCorrupt { get; set; }
        public bool IsFileLoadError { get; set; }
        public string? FileErrorMessage { get; set; }
        public string? FileCorruptMessage { get; set; }

        public string? AmazonId { get; set; }
        public string? MusicBrainzTrackId { get; set; }

        // Relationship properties
        public Mp3FileReferenceAlbum? Album { get; set; }
        public Mp3FileReferenceArtist? PrimaryArtist { get; set; }
        public Mp3FileReferenceGenre? PrimaryGenre { get; set; }

        public Mp3FileReference() { }
    }
}

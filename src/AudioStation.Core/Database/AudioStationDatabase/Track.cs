using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Track", Schema = "public")]
    public class Track : AudioStationEntityBase
    {
        [ForeignKey("FileReference")]
        public int FileReferenceId { get; set; }

        [ForeignKey("Album")]
        public int? AlbumId { get; set; }

        [ForeignKey("PrimaryArtist")]
        public int? PrimaryArtistId { get; set; }

        [ForeignKey("PrimaryGenre")]
        public int? PrimaryGenreId { get; set; }

        public string? Title { get; set; }
        public int? Number { get; set; }
        public int? DurationMilliseconds { get; set; }

        public string? AmazonId { get; set; }
        public Guid? MusicBrainzTrackId { get; set; }

        // Relationship properties
        public FileReference FileReference { get; set; }
        public Album? Album { get; set; }
        public Artist? PrimaryArtist { get; set; }
        public Genre? PrimaryGenre { get; set; }

        public Track() { }
    }
}

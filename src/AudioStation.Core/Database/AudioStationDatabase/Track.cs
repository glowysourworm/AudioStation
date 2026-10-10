using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Track", Schema = "public")]
    public class Track : AudioStationEntityBase
    {
        [ForeignKey("FileReference")]
        public Guid FileReferenceId { get; set; }

        [ForeignKey("Album")]
        public Guid AlbumId { get; set; }

        [ForeignKey("PrimaryArtist")]
        public Guid ArtistId { get; set; }

        [ForeignKey("PrimaryGenre")]
        public Guid GenreId { get; set; }

        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public int MediaNumber { get; set; }
        public int DurationMilliseconds { get; set; }

        // Relationship properties
        public FileReference FileReference { get; set; }
        public Album Album { get; set; }
        public Artist Artist { get; set; }
        public Genre Genre { get; set; }

        public Track() { }
    }
}

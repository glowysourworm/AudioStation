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
        public int AlbumId { get; set; }

        [ForeignKey("PrimaryArtist")]
        public int ArtistId { get; set; }

        [ForeignKey("PrimaryGenre")]
        public int GenreId { get; set; }

        public string Title { get; set; }
        public int Number { get; set; }
        public int DurationMilliseconds { get; set; }

        // Relationship properties
        public FileReference FileReference { get; set; }
        public Album Album { get; set; }
        public Artist Artist { get; set; }
        public Genre Genre { get; set; }

        public Track() { }
    }
}

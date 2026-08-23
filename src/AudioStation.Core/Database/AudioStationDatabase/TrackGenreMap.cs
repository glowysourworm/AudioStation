using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("TrackGenreMap", Schema = "public")]
    public class TrackGenreMap : AudioStationEntityBase
    {
        [ForeignKey("Track")]
        public int TrackId { get; set; }

        [ForeignKey("Genre")]
        public int GenreId { get; set; }

        public bool IsPrimaryGenre { get; set; }

        public Track Track { get; set; }
        public Genre Genre { get; set; }


        public TrackGenreMap()
        {

        }
    }
}

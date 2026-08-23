using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("TrackArtistMap", Schema = "public")]
    public class TrackArtistMap : AudioStationEntityBase
    {
        [ForeignKey("Track")]
        public int TrackId { get; set; }

        [ForeignKey("Artist")]
        public int ArtistId { get; set; }

        public Track Track { get; set; }
        public Artist Artist { get; set; }
        public bool IsPrimaryArtist { get; set; }

        public TrackArtistMap()
        {

        }
    }
}

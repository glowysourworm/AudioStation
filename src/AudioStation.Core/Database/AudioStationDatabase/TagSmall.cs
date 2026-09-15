using System.ComponentModel.DataAnnotations.Schema;

using AudioStation.Core.Model.Interface;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("TagSmall", Schema = "public")]
    public class TagSmall : AudioStationEntityBase, ITagSmall
    {
        public string? AlbumArtist { get; set; }
        public string? Album { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int? TrackNumber { get; set; }
        public int? TrackTotal { get; set; }
        public int? MediaNumber { get; set; }
        public int? MediaTotal { get; set; }
        public string? MediaFormat { get; set; }
        public int? DurationMilliseconds { get; set; }
        public int? Year { get; set; }

        public TagSmall()
        {
        }
    }
}

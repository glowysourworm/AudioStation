using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Mp3FileReferenceArtist", Schema = "public")]
    public class Mp3FileReferenceArtist : AudioStationEntityBase
    {
        public string Name { get; set; }
        public string? MusicBrainzArtistId { get; set; }

        public Mp3FileReferenceArtist() { }
        public Mp3FileReferenceArtist(int Id_, string Name_)
        {
            this.Id = Id_;
            this.Name = Name_;
        }
    }
}

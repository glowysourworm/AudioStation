using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Mp3FileReferenceGenre", Schema = "public")]
    public class Mp3FileReferenceGenre : AudioStationEntityBase
    {
        public string Name { get; set; }

        public Mp3FileReferenceGenre() { }
        public Mp3FileReferenceGenre(int Id_, string Name_)
        {
            this.Id = Id_;
            this.Name = Name_;
        }
    }
}

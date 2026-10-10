using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("TagSmallFileReferenceMap", Schema = "public")]
    public class TagSmallFileReferenceMap : AudioStationEntityBase
    {
        [ForeignKey("TagSmall")]
        public Guid TagSmallId { get; set; }

        [ForeignKey("FileReference")]
        public Guid FileReferenceId { get; set; }

        public TagSmall TagSmall { get; set; }
        public FileReference FileReference { get; set; }

        public TagSmallFileReferenceMap() { }
    }
}

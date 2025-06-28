using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    public class AudioStationEntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzTagEntityMap", Schema = "public")]
    public class MusicBrainzTagEntityMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzTag")]
        public Guid MusicBrainzTagId { get; set; }

        // Not a hard foreign key constraint
        public Guid MusicBrainzEntityId {  get; set; }

        [ForeignKey("MusicBrainzEntityType")]
        public int MusicBrainzEntityTypeId { get; set; }

        public MusicBrainzTagEntity MusicBrainzTag { get; set; }
        public MusicBrainzEntityType MusicBrainzEntityType { get; set; }

        public MusicBrainzTagEntityMap() { }
    }
}

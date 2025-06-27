using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzTagEntityMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid MusicBrainzTagId { get; set; }
        public Guid MusicBrainzEntityId {  get; set; }
        public int MusicBrainzEntityTypeId { get; set; }
    }
}

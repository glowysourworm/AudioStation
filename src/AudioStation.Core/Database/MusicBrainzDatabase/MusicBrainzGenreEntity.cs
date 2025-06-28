using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzGenre", Schema = "public")]
    public class MusicBrainzGenreEntity : MusicBrainzEntityBase
    {
        public string Name { get; set; }

        public MusicBrainzGenreEntity() { }
    }
}

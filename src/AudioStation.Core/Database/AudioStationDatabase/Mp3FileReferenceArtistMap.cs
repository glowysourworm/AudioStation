﻿using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Mp3FileReferenceArtistMap", Schema = "public")]
    public class Mp3FileReferenceArtistMap : AudioStationEntityBase
    {
        [ForeignKey("Mp3FileReference")]
        public int Mp3FileReferenceId { get; set; }

        [ForeignKey("Mp3FileReferenceArtist")]
        public int Mp3FileReferenceArtistId { get; set; }

        public Mp3FileReference Mp3FileReference { get; set; }
        public Mp3FileReferenceArtist Mp3FileReferenceArtist { get; set; }
        public bool IsPrimaryArtist { get; set; }

        public Mp3FileReferenceArtistMap()
        {

        }
    }
}

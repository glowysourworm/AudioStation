﻿using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Index("Name", Name = "NameIndex")]
    [Table("M3UStream", Schema = "public")]
    public class M3UStream : AudioStationEntityBase
    {
        public int Duration { get; set; }
        public string Name { get; set; }
        public string? GroupName { get; set; }
        public string? LogoUrl { get; set; }
        public string? HomepageUrl { get; set; }
        public string StreamSourceUrl { get; set; }
        public bool UserExcluded { get; set; }

        public M3UStream() 
        {
        }
    }
}

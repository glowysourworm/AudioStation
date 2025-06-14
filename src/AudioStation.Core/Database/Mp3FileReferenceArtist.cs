﻿using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database
{
    [PrimaryKey("Id")]
    [Table("Mp3FileReferenceArtist", Schema = "public")]
    public class Mp3FileReferenceArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Mp3FileReferenceArtist() { }
        public Mp3FileReferenceArtist(int Id_, string Name_)
        {
            this.Id = Id_;
            this.Name = Name_;
        }
    }
}

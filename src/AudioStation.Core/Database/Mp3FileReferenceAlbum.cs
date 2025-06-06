using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database
{
    public class Mp3FileReferenceAlbum
    {
        public int Id { get; set; }
        public int Mp3FileReferenceId { get; set; }
        public string Name { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }

        public Mp3FileReferenceAlbum(int Id_, int Mp3FileReferenceId_, string Name_, int DiscNumber_, int DiscCount_)
        {
            this.Id = Id_;
            this.Mp3FileReferenceId = Mp3FileReferenceId_;
            this.Name = Name_;
            this.DiscNumber = DiscNumber_;
            this.DiscCount = DiscCount_;
        }
    }
}

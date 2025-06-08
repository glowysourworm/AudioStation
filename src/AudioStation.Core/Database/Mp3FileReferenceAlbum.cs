using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("Mp3FileReferenceAlbum", Schema = "public")]
    public class Mp3FileReferenceAlbum
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }

        public Mp3FileReferenceAlbum() { }
        public Mp3FileReferenceAlbum(int Id_, string Name_, int DiscNumber_, int DiscCount_)
        {
            this.Id = Id_;
            this.Name = Name_;
            this.DiscNumber = DiscNumber_;
            this.DiscCount = DiscCount_;
        }
    }
}

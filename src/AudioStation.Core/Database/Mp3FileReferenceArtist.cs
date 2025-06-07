using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("Mp3FileReferenceArtist", Schema = "public")]
    public class Mp3FileReferenceArtist
    {
        public int Id { get; set; }
        public int Mp3FileReferenceId { get; set; }
        public string Name { get; set; }

        public Mp3FileReferenceArtist() { }
        public Mp3FileReferenceArtist(int Id_, int Mp3FileReferenceId_, string Name_)
        {
            this.Id = Id_;
            this.Mp3FileReferenceId = Mp3FileReferenceId_;
            this.Name = Name_;
        }
    }
}

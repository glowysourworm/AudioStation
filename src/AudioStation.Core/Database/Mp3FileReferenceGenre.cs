using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("Mp3FileReferenceGenre", Schema = "public")]
    public class Mp3FileReferenceGenre
    {
        public int Id { get; set; }
        public int Mp3FileReferenceId { get; set; }
        public string Name { get; set; }

        public Mp3FileReferenceGenre() { }
        public Mp3FileReferenceGenre(int Id_, int Mp3FileReferenceId_, string Name_)
        {
            this.Id = Id_;
            this.Mp3FileReferenceId = Mp3FileReferenceId_;
            this.Name = Name_;
        }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database
{
    [PrimaryKey("Id")]
    [Table("M3UInfoWarning", Schema = "public")]
    public class M3UInfoWarning
    {
        public int Id { get; set; }

        [ForeignKey("M3UInfo")]
        public int M3UInfoId { get; set; }
        public string Warning { get; set; }

        // Relationship properties
        public M3UInfo? M3UInfo { get; set; }

        public M3UInfoWarning() { }
        public M3UInfoWarning(int Id_, int M3UInfoId_, string Warning_)
        {
            this.Id = Id_;
            this.M3UInfoId = M3UInfoId_;
            this.Warning = Warning_;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database
{
    [Table("M3UInfo", Schema = "public")]
    public class M3UInfo
    {
        public int Id { get; set; }
        public string PlaylistType { get; set; }
        public int? TargetDurationMilliseconds { get; set; }
        public int? Version { get; set; }
        public int? MediaSequence { get; set; }
        public bool UserExcluded { get; set; }

        public M3UInfo() { }
        public M3UInfo(int Id_, string PlaylistType_, int? TargetDurationMilliseconds_, int? Version_, int? MediaSequence_, bool UserExcluded_)
        {
            this.Id = Id_;
            this.PlaylistType = PlaylistType_;
            this.TargetDurationMilliseconds = TargetDurationMilliseconds_;
            this.Version = Version_;
            this.MediaSequence = MediaSequence_;
            this.UserExcluded = UserExcluded_;
        }
    }
}

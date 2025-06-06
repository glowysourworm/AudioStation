using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database
{
    public class M3UMedia
    {
        public int Id { get; set; }
        public int DurationMilliseconds { get; set; }
        public string RawTitle { get; set; }
        public string InnerTitle { get; set; }
        public string MediaFile { get; set; }
        public bool UserExcluded { get; set; }

        public M3UMedia(int Id_, int DurationMilliseconds_, string RawTitle_, string InnerTitle_, string MediaFile_, bool UserExcluded_)
        {
            this.Id = Id_;
            this.DurationMilliseconds = DurationMilliseconds_;
            this.RawTitle = RawTitle_;
            this.InnerTitle = InnerTitle_;
            this.MediaFile = MediaFile_;
            this.UserExcluded = UserExcluded_;
        }
    }
}

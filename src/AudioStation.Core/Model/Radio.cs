using System.Collections.ObjectModel;

namespace AudioStation.Core.Model
{
    public class Radio
    {
        public List<RadioEntry> RadioStreams { get; set; }             // M3U file data (or) other online services

        public Radio()
        {
            this.RadioStreams = new List<RadioEntry>();
        }
    }
}

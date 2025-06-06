using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database
{
    public class M3UInfoWarning
    {
        public int Id { get; set; }
        public int M3UInfoId { get; set; }
        public string Warning { get; set; }

        public M3UInfo? M3UInfo { get; set; }

        public M3UInfoWarning(int Id_, int M3UInfoId_, string Warning_)
        {
            this.Id = Id_;
            this.M3UInfoId = M3UInfoId_;
            this.Warning = Warning_;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database
{
    public class M3UInfoAttributes
    {
        public int Id { get; set; }
        public int? M3UInfoId { get; set; }
        public int? M3UMediaId { get; set; }
        public string GroupTitle { get; set; }
        public string TvgShift { get; set; }
        public string TvgName { get; set; }
        public string TvgLogo { get; set; }
        public string AudioTrack { get; set; }
        public string AspectRatio { get; set; }
        public string TvgId { get; set; }
        public string UrlTvg { get; set; }
        public int? M3UAutoLoad { get; set; }
        public int? Cache { get; set; }
        public int? Deinterlace { get; set; }
        public int? Refresh { get; set; }
        public int? ChannelNumber { get; set; }

        public M3UInfo? M3UInfo { get; set; }
        public M3UMedia? M3UInfoMedia { get; set; }

        public M3UInfoAttributes(int Id_, int? M3UInfoId_, string GroupTitle_, string TvgShift_, string TvgName_, string TvgLogo_, string AudioTrack_, string AspectRatio_, string TvgId_, string UrlTvg_, int? M3UAutoLoad_, int? Cache_, int? Deinterlace_, int? Refresh_, int? ChannelNumber_, int? M3UMediaId_)
        {
            this.Id = Id_;
            this.M3UInfoId = M3UInfoId_;
            this.GroupTitle = GroupTitle_;
            this.TvgShift = TvgShift_;
            this.TvgName = TvgName_;
            this.TvgLogo = TvgLogo_;
            this.AudioTrack = AudioTrack_;
            this.AspectRatio = AspectRatio_;
            this.TvgId = TvgId_;
            this.UrlTvg = UrlTvg_;
            this.M3UAutoLoad = M3UAutoLoad_;
            this.Cache = Cache_;
            this.Deinterlace = Deinterlace_;
            this.Refresh = Refresh_;
            this.ChannelNumber = ChannelNumber_;
            this.M3UMediaId = M3UMediaId_;
        }
    }
}

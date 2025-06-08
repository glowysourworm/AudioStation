using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("M3UInfoAttributes", Schema = "public")]
    public class M3UInfoAttributes
    {
        public int Id { get; set; }

        [ForeignKey("M3UInfo")]
        public int? M3UInfoId { get; set; }
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

        // Relationship Properties
        public M3UInfo? M3UInfo { get; set; }

        public M3UInfoAttributes() { }
    }
}

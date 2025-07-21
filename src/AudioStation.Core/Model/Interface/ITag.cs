using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Core.Model.Vendor.TagLibExtension;

namespace AudioStation.Core.Model.Interface
{
    /// <summary>
    /// Our tag interface - which is very close to TagLib. However, there are some standards issues that
    /// I'm still learning. Most of these fields are direct to/from taglib. The unhandled properties may
    /// contain data we want. So, we'll be monitoring them.
    /// </summary>
    public interface ITag : ISimpleTag
    {
        public string AlbumSort { get; set; }
        public string[] AlbumArtists { get; set; }
        public string[] AlbumArtistsSort { get; set; }
        public string TitleSort { get; set; }
        public string[] Performers { get; set; }
        public string[] PerformersSort { get; set; }
        public string[] Composers { get; set; }
        public string[] ComposersSort { get; set; }                
        public string[] Genres { get; set; }
        public uint Year { get; set; }
        public string Comment { get; set; }
        public string Lyrics { get; set; }
        public string MusicBrainzArtistId { get; set; }
        public string MusicBrainzReleaseId { get; set; }
        public string MusicBrainzReleaseArtistId { get; set; }
        public string MusicBrainzTrackId { get; set; }
        public string MusicBrainzDiscId { get; set; }
        public string MusicIpId { get; set; }
        public string AmazonId { get; set; }
        public string MusicBrainzReleaseStatus { get; set; }
        public string MusicBrainzReleaseType { get; set; }
        public string MusicBrainzReleaseCountry { get; set; }
        public TagPicture[] Pictures { get; set; }        
    }
}

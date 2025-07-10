using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

using TagLib;

namespace AudioStation.Core.Model.Vendor.TagLibExtension
{
    public class TagExtension : TagLib.Tag, IRecursiveSerializable
    {
        TagTypes _tagTypes;

        public override TagTypes TagTypes { get; }
        public override string Album { get; set; }
        public override string[] AlbumArtists { get; set; }
        public override string[] AlbumArtistsSort { get; set; }
        public override string Title { get; set; }
        public override string TitleSort { get; set; }
        public override string[] Performers { get; set; }
        public override string[] PerformersSort { get; set; }
        public override string[] Composers { get; set; }
        public override string[] ComposersSort { get; set; }
        public override string AlbumSort { get; set; }
        public override string Comment { get; set; }
        public override string[] Genres { get; set; }
        public override uint Year { get; set; }
        public override uint Track { get; set; }
        public override uint TrackCount { get; set; }
        public override uint Disc { get; set; }
        public override uint DiscCount { get; set; }
        public override string Lyrics { get; set; }
        public override string Grouping { get; set; }
        public override uint BeatsPerMinute { get; set; }
        public override string Conductor { get; set; }
        public override string Copyright { get; set; }
        public override string MusicBrainzArtistId { get; set; }
        public override string MusicBrainzReleaseId { get; set; }
        public override string MusicBrainzReleaseArtistId { get; set; }
        public override string MusicBrainzTrackId { get; set; }
        public override string MusicBrainzDiscId { get; set; }
        public override string MusicIpId { get; set; }
        public override string AmazonId { get; set; }
        public override string MusicBrainzReleaseStatus { get; set; }
        public override string MusicBrainzReleaseType { get; set; }
        public override string MusicBrainzReleaseCountry { get; set; }
        public override double ReplayGainTrackGain { get; set; }
        public override double ReplayGainTrackPeak { get; set; }
        public override double ReplayGainAlbumGain { get; set; }
        public override double ReplayGainAlbumPeak { get; set; }
        public override IPicture[] Pictures { get; set; }
        public override string[] Artists { get; set; }
        public override bool IsEmpty { get; }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public TagExtension()
        {

        }
        public TagExtension(IPropertyReader reader)
        {
            this.TagTypes = reader.Read<TagTypes>("TagTypes");
            this.Album = reader.Read<string>("Album");
            this.AlbumArtists = reader.Read<List<string>>("AlbumArtists")?.ToArray() ?? new string[] { };
            this.AlbumArtistsSort = reader.Read<List<string>>("AlbumArtistsSort")?.ToArray() ?? new string[] { };
            this.Title = reader.Read<string>("Title");
            this.TitleSort = reader.Read<string>("TitleSort");
            this.Performers = reader.Read<List<string>>("Performers")?.ToArray() ?? new string[] { };
            this.PerformersSort = reader.Read<List<string>>("PerformersSort")?.ToArray() ?? new string[] { };
            this.Composers = reader.Read<List<string>>("Composers")?.ToArray() ?? new string[] { };
            this.ComposersSort = reader.Read<List<string>>("ComposersSort")?.ToArray() ?? new string[] { };
            this.AlbumSort = reader.Read<string>("AlbumSort");
            this.Comment = reader.Read<string>("Comment");
            this.Genres = reader.Read<List<string>>("Genres")?.ToArray() ?? new string[] { };
            this.Year = reader.Read<uint>("Year");
            this.Track = reader.Read<uint>("Track");
            this.TrackCount = reader.Read<uint>("TrackCount");
            this.Disc = reader.Read<uint>("Disc");
            this.DiscCount = reader.Read<uint>("DiscCount");
            this.Lyrics = reader.Read<string>("Lyrics");
            this.Grouping = reader.Read<string>("Grouping");
            this.BeatsPerMinute = reader.Read<uint>("BeatsPerMinute");
            this.Conductor = reader.Read<string>("Conductor");
            this.Copyright = reader.Read<string>("Copyright");
            this.MusicBrainzArtistId = reader.Read<string>("MusicBrainzArtistId");
            this.MusicBrainzReleaseId = reader.Read<string>("MusicBrainzReleaseId");
            this.MusicBrainzReleaseArtistId = reader.Read<string>("MusicBrainzReleaseArtistId");
            this.MusicBrainzTrackId = reader.Read<string>("MusicBrainzTrackId");
            this.MusicBrainzDiscId = reader.Read<string>("MusicBrainzDiscId");
            this.MusicIpId = reader.Read<string>("MusicIpId");
            this.AmazonId = reader.Read<string>("AmazonId");
            this.MusicBrainzReleaseStatus = reader.Read<string>("MusicBrainzReleaseStatus");
            this.MusicBrainzReleaseType = reader.Read<string>("MusicBrainzReleaseType");
            this.MusicBrainzReleaseCountry = reader.Read<string>("MusicBrainzReleaseCountry");
            this.ReplayGainTrackGain = reader.Read<double>("ReplayGainTrackGain");
            this.ReplayGainTrackPeak = reader.Read<double>("ReplayGainTrackPeak");
            this.ReplayGainAlbumGain = reader.Read<double>("ReplayGainAlbumGain");
            this.ReplayGainAlbumPeak = reader.Read<double>("ReplayGainAlbumPeak");
            this.Pictures = reader.Read<List<TagPicture>>("Pictures")?.ToArray() ?? new TagPicture[] { };
            this.Artists = reader.Read<List<string>>("Artists")?.ToArray() ?? new string[] { };
            this.IsEmpty = reader.Read<bool>("IsEmpty");
        }
        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("TagTypes", this.TagTypes);
            writer.Write("Album", this.Album);
            writer.Write("AlbumArtists", this.AlbumArtists.ToList());
            writer.Write("AlbumArtistsSort", this.AlbumArtistsSort.ToList());
            writer.Write("Title", this.Title);
            writer.Write("TitleSort", this.TitleSort);
            writer.Write("Performers", this.Performers.ToList());
            writer.Write("PerformersSort", this.PerformersSort.ToList());
            writer.Write("Composers", this.Composers.ToList());
            writer.Write("ComposersSort", this.ComposersSort.ToList());
            writer.Write("AlbumSort", this.AlbumSort);
            writer.Write("Comment", this.Comment);
            writer.Write("Genres", this.Genres.ToList());
            writer.Write("Year", this.Year);
            writer.Write("Track", this.Track);
            writer.Write("TrackCount", this.TrackCount);
            writer.Write("Disc", this.Disc);
            writer.Write("DiscCount", this.DiscCount);
            writer.Write("Lyrics", this.Lyrics);
            writer.Write("Grouping", this.Grouping);
            writer.Write("BeatsPerMinute", this.BeatsPerMinute);
            writer.Write("Conductor", this.Conductor);
            writer.Write("Copyright", this.Copyright);
            writer.Write("MusicBrainzArtistId", this.MusicBrainzArtistId);
            writer.Write("MusicBrainzReleaseId", this.MusicBrainzReleaseId);
            writer.Write("MusicBrainzReleaseArtistId", this.MusicBrainzReleaseArtistId);
            writer.Write("MusicBrainzTrackId", this.MusicBrainzTrackId);
            writer.Write("MusicBrainzDiscId", this.MusicBrainzDiscId);
            writer.Write("MusicIpId", this.MusicIpId);
            writer.Write("AmazonId", this.AmazonId);
            writer.Write("MusicBrainzReleaseStatus", this.MusicBrainzReleaseStatus);
            writer.Write("MusicBrainzReleaseType", this.MusicBrainzReleaseType);
            writer.Write("MusicBrainzReleaseCountry", this.MusicBrainzReleaseCountry);
            writer.Write("ReplayGainTrackGain", this.ReplayGainTrackGain);
            writer.Write("ReplayGainTrackPeak", this.ReplayGainTrackPeak);
            writer.Write("ReplayGainAlbumGain", this.ReplayGainAlbumGain);
            writer.Write("ReplayGainAlbumPeak", this.ReplayGainAlbumPeak);
            writer.Write("Pictures", this.Pictures.Select(x => new TagPicture(x)).ToList());
            writer.Write("Artists", this.Artists.ToList());
            writer.Write("IsEmpty", this.IsEmpty);
        }
    }
}

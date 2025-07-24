using ATL;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzCombined
    {
        public Guid ReleaseId { get; set; }
        public Guid ArtistId { get; set; }
        public Guid TrackId { get; set; }
        public string ArtistCreditName { get; set; }
        public string Asin { get; set; }
        public string Barcode { get; set; }
        public string ReleaseCountry { get; set; }
        public PictureInfo FrontCover { get; set; }
        public PictureInfo BackCover { get; set; }
        public string LabelCatalogNumber { get; set; }
        public int LabelCode { get; set; }
        public string LabelName { get; set; }
        public string LabelCountry { get; set; }
        public IEnumerable<string> LabelIpis { get; set; }
        public int MediumTrackCount { get; set; }
        public int MediumTrackOffset { get; set; }
        public int MediumDiscCount { get; set; }
        public int MediumDiscPosition { get; set; }
        public string MediumFormat { get; set; }
        public string MediumTitle { get; set; }
        public ITrack Track { get; set; }
        public string Packaging { get; set; }
        public string Quality { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseStatus { get; set; }
        public string ReleaseTitle { get; set; }
        public string Annotation { get; set; }
        public IEnumerable<string> AssociatedUrls { get; set; }
        public IEnumerable<string> Genres { get; set; }
        public IEnumerable<string> UserGenres { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> UserTags { get; set; }
        public string Disambiguation { get; set; }
        public string Title { get; set; }

        public MusicBrainzCombined()
        {

        }
    }
}

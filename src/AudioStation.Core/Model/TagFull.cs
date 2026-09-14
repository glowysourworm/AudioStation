using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.IdSharp;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Class that represents a "full" tag - which will be all the fields AudioStation supports. This
    /// will only be loaded into memory when the user interacts with the UI; and not during library
    /// loading unless it is only for a temporary workload process. The cache performance will be 
    /// badly degraded if you try loading it while iterating files.
    /// </summary>
    public class TagFull : ITagFull
    {
        public static string ACOUSTID_ID = "ACOUSTID ID";
        public static string ACOUSTID_FINGERPRINT = "ACOUSTID FINGERPRINT";

        public static string MUSICBRAINZ_ALBUM_ARTIST_ID = "MUSICBRAINZ ALBUM ARTIST ID";
        public static string MUSICBRAINZ_ALBUM_ID = "MUSICBRAINZ ALBUM ID";
        public static string MUSICBRAINZ_ALBUM_RELEASE_COUNTRY = "MUSICBRAINZ ALBUM RELEASE COUNTRY";
        public static string MUSICBRAINZ_ALBUM_STATUS = "MUSICBRAINZ ALBUM STATUS";
        public static string MUSICBRAINZ_ALBUM_TYPE = "MUSICBRAINZ ALBUM TYPE";
        public static string MUSICBRAINZ_ARTIST_ID = "MUSICBRAINZ ARTIST ID";
        public static string MUSICBRAINZ_DISCID = "MUSICBRAINZ DISCID";
        public static string MUSICBRAINZ_ORIGINALALBUMID = "MUSICBRAINZ ORIGINALALBUMID";
        public static string MUSICBRAINZ_ORIGINALARTISTID = "MUSICBRAINZ ORIGINALARTISTID";
        public static string MUSICBRAINZ_RELEASE_GROUP_ID = "MUSICBRAINZ RELEASE GROUP ID";
        public static string MUSICBRAINZ_RELEASE_TRACK_ID = "MUSICBRAINZ RELEASE TRACK ID";
        public static string MUSICBRAINZ_TRACKID = "http://musicbrainz.org";                        // UFID/http://musicbrainz.org
        public static string MUSICBRAINZ_TRMID = "MUSICBRAINZ TRMID";
        public static string MUSICBRAINZ_WORK_ID = "MUSICBRAINZ WORK ID";

        public string? AlbumArtist { get; set; }
        public string? Album { get; set; }
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public string? Copyright { get; set; }
        public int? DurationMilliseconds { get; set; }
        public string? Genre { get; set; }
        public int? TrackNumber { get; set; }
        public int? TrackTotal { get; set; }
        public int? MediaNumber { get; set; }
        public int? MediaTotal { get; set; }
        public string? MediaFormat { get; set; }
        public string? Publisher { get; set; }
        public string? SortAlbumArtist { get; set; }
        public string? SortArtist { get; set; }
        public string? SortAlbum { get; set; }
        public string? SortTitle { get; set; }
        public int? Year { get; set; }
        public IList<UserDefinedField> UserDefinedFields { get; set; }
        public IList<InvolvedPerson> InvolvedPeople { get; set; }
        public IList<UniqueFileIdentifier> UniqueFileIdentifiers { get; set; }

        public TagFull()
        {
            this.UserDefinedFields = new List<UserDefinedField>();
            this.InvolvedPeople = new List<InvolvedPerson>();
            this.UniqueFileIdentifiers = new List<UniqueFileIdentifier>();
        }

        public Guid? GetAcoustIDIdentifier()
        {
            return GetUserDefinedGuid(ACOUSTID_ID);
        }
        public string? GetAcoustIDFingerprint()
        {
            return GetUserDefinedString(ACOUSTID_FINGERPRINT);
        }
        public Guid? GetMusicBrainzAlbumArtistId()
        {
            return GetUserDefinedGuid(MUSICBRAINZ_ALBUM_ARTIST_ID);
        }

        public Guid? GetMusicBrainzAlbumId()
        {
            return GetUserDefinedGuid(MUSICBRAINZ_ALBUM_ID);
        }

        public string? GetMusicBrainzAlbumReleaseCountry()
        {
            return GetUserDefinedString(MUSICBRAINZ_ALBUM_RELEASE_COUNTRY);
        }

        public string? GetMusicBrainzAlbumStatus()
        {
            return GetUserDefinedString(MUSICBRAINZ_ALBUM_STATUS);
        }

        public string? GetMusicBrainzAlbumType()
        {
            return GetUserDefinedString(MUSICBRAINZ_ALBUM_TYPE);
        }

        public Guid? GetMusicBrainzArtistId()
        {
            return GetUserDefinedGuid(MUSICBRAINZ_ARTIST_ID);
        }

        public Guid? GetMusicBrainzReleaseGroupId()
        {
            return GetUserDefinedGuid(MUSICBRAINZ_RELEASE_GROUP_ID);
        }

        public Guid? GetMusicBrainzReleaseTrackId()
        {
            return GetUserDefinedGuid(MUSICBRAINZ_RELEASE_TRACK_ID);
        }

        public Guid? GetMusicBrainzTrackId()
        {
            foreach (var identifier in this.UniqueFileIdentifiers)
            {
                if (identifier.OwnerIdentifier.ToLowerInvariant() == MUSICBRAINZ_TRACKID.ToLowerInvariant())
                {
                    var value = identifier.GetString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        Guid result;
                        if (Guid.TryParse(value, out result))
                            return result;
                    }
                }
            }
            return null;
        }

        public Guid? GetMusicBrainzWorkId()
        {
            return GetUserDefinedGuid(MUSICBRAINZ_WORK_ID);
        }

        private Guid? GetUserDefinedGuid(string txxxDescription)
        {
            foreach (var txxx in this.UserDefinedFields)
            {
                if (txxx.Description.ToLowerInvariant() == txxxDescription.ToLowerInvariant())
                {
                    Guid result;
                    if (Guid.TryParse(txxx.Value, out result))
                        return result;
                }
            }

            return null;
        }
        private string? GetUserDefinedString(string txxxDescription)
        {
            foreach (var txxx in this.UserDefinedFields)
            {
                if (txxx.Description.ToLowerInvariant() == txxxDescription.ToLowerInvariant())
                {
                    return txxx.Value;
                }
            }

            return null;
        }
    }
}

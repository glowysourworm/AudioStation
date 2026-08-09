using System.Linq.Expressions;

using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Service
{
    public class AudioStationTagServiceModel
    {
        public enum AudioStationTagIdentity
        {
            /// <summary>
            /// Minimum required track data for looking up tag metadata from most 3rd party services
            /// </summary>
            ArtistAlbumTitle = 0,

            /// <summary>
            /// Proprietary ID considered "industry standard" for keeping music metadata
            /// </summary>
            MusicBrainzId = 1,

            /// <summary>
            /// Vendor specified ID, other than MusicBrainz, used to identify the specific track and
            /// its related metadata. This must be specified by the IAudioStationTagService and handled
            /// to produce the tag detail - by that service.
            /// </summary>
            VendorId = 2
        }

        // Tag Property Names
        private List<string> _tagProperties;

        public AudioStationTagIdentity IdType { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Title { get; }
        public Guid MusicBrainzId { get; }
        public object? VendorId { get; }

        /// <summary>
        /// Constructor for Artist / Album / Title tag identity
        /// </summary>
        public AudioStationTagServiceModel(string artist, string album, string title)
        {
            _tagProperties = new List<string>();
            this.Artist = artist;
            this.Album = album;
            this.Title = title;
            this.MusicBrainzId = Guid.Empty;
            this.VendorId = null;
            this.IdType = AudioStationTagIdentity.ArtistAlbumTitle;
        }

        /// <summary>
        /// Constructor for Music Brainz Id
        /// </summary>
        public AudioStationTagServiceModel(Guid musicBrainzId)
        {
            _tagProperties = new List<string>();
            this.Artist = string.Empty;
            this.Album = string.Empty;
            this.Title = string.Empty;
            this.MusicBrainzId = musicBrainzId;
            this.VendorId = null;
            this.IdType = AudioStationTagIdentity.MusicBrainzId;
        }

        /// <summary>
        /// Constructor for Music Brainz Id
        /// </summary>
        public AudioStationTagServiceModel(object vendorId)
        {
            _tagProperties = new List<string>();
            this.Artist = string.Empty;
            this.Album = string.Empty;
            this.Title = string.Empty;
            this.MusicBrainzId = Guid.Empty;
            this.VendorId = vendorId;
            this.IdType = AudioStationTagIdentity.VendorId;
        }

        /// <summary>
        /// Adds property to tag service model to be dealt with during any transaction.
        /// </summary>
        public void AddTagProperty(Expression<Func<IAudioStationTag>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression != null &&
                !string.IsNullOrWhiteSpace(memberExpression.Member.Name) &&
                !_tagProperties.Contains(memberExpression.Member.Name))
            {
                _tagProperties.Add(memberExpression.Member.Name);
            }

            else
                throw new Exception("Invalid or duplicate property expression for IAudioStationTag");
        }

        public IEnumerable<string> GetTagProperties()
        {
            return _tagProperties;
        }
    }
}

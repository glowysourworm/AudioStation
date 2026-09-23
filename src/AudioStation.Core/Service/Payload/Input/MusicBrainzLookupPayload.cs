using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service.Payload.Input
{
    public class MusicBrainzLookupPayload : ITagServiceInputPayload
    {
        public MusicBrainzLookupRequestType IdType { get; }
        public string? Artist { get; private set; }
        public string? Album { get; private set; }
        public string? Title { get; private set; }
        public Guid? MusicBrainzId { get; private set; }

        /// <summary>
        /// Constructor for performing a lookup based on artist / album / title
        /// </summary>
        public MusicBrainzLookupPayload(string artist, string album, string title)
        {
            this.IdType = MusicBrainzLookupRequestType.ArtistAlbumTitle;
            this.Artist = artist;
            this.Album = album;
            this.Title = title;
            this.MusicBrainzId = null;
        }

        /// <summary>
        /// Constructor for performing a lookup based on a specific Music Brainz ID
        /// </summary>
        /// <param name="idType">Type of ID for the Music Brainz service</param>
        /// <param name="musicBrainzId">Music Brainz Proprietary ID</param>
        public MusicBrainzLookupPayload(MusicBrainzLookupRequestType idType, Guid? musicBrainzId)
        {
            this.IdType = idType;
            this.Artist = null;
            this.Album = null;
            this.Title = null;
            this.MusicBrainzId = musicBrainzId;
        }
    }
}

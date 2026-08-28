namespace AudioStation.Core.Service
{
    public class AudioStationTagServiceRequest
    {
        public AudioStationTagRequestType Type { get; }
        public AudioStationTagIdentity IdType { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Title { get; }
        public Guid MusicBrainzRecordingId { get; }

        /// <summary>
        /// Constructor for Artist / Album / Title tag identity
        /// </summary>
        public AudioStationTagServiceRequest(AudioStationTagRequestType type, string artist, string album, string title)
        {
            this.Artist = artist;
            this.Album = album;
            this.Title = title;
            this.MusicBrainzRecordingId = Guid.Empty;
            this.IdType = AudioStationTagIdentity.ArtistAlbumTitle;
        }

        /// <summary>
        /// Constructor for Music Brainz Id
        /// </summary>
        public AudioStationTagServiceRequest(AudioStationTagRequestType type, Guid musicBrainzId)
        {
            this.Artist = string.Empty;
            this.Album = string.Empty;
            this.Title = string.Empty;
            this.MusicBrainzRecordingId = musicBrainzId;
            this.IdType = AudioStationTagIdentity.MusicBrainzId;
            this.Type = type;
        }
    }
}

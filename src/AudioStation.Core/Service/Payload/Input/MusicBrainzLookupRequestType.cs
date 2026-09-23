namespace AudioStation.Core.Service.Payload.Input
{
    public enum MusicBrainzLookupRequestType
    {
        /// <summary>
        /// Minimum required track data for looking up tag metadata from most 3rd party services
        /// </summary>
        ArtistAlbumTitle = 0,

        /// <summary>
        /// Proprietary ID considered "industry standard" for keeping music metadata
        /// </summary>
        MusicBrainzRecordingId = 1,

        /// <summary>
        /// Music Brainz ID for track information
        /// </summary>
        MusicBrainzTrackId = 2,

        /// <summary>
        /// Music Brainz ID for release track information
        /// </summary>
        MusicBrainzReleaseTrackId = 3
    }
}

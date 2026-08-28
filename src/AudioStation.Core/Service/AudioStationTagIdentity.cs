namespace AudioStation.Core.Service
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
        MusicBrainzId = 1
    }
}

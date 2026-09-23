using AudioStation.Core.Database.AudioStationDatabase.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input
{
    public class LibraryLoaderMusicBrainzBasicPayload
    {
        /// <summary>
        /// Full file path of the source file
        /// </summary>
        public string FileName { get; set; }
        public IEnumerable<IAcoustIDLookupResult> AcoustIDResults { get; set; }
        public Guid? MusicBrainzReleaseTrackIDTag { get; set; }

        /// <summary>
        /// Set to true to use the Music Brainz ID's to lookup the tag info that was left
        /// in the tag from the previous application.
        /// </summary>
        public bool PerformExtraTagLookup { get; set; }
    }
}

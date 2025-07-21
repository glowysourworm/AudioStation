using AudioStation.Core.Database.MusicBrainzDatabase.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderImportDetailOutput : LibraryLoaderImportBasicOutput
    {
        /// <summary>
        /// Front cover art recovered during the query process
        /// </summary>
        public MusicBrainzPicture? BestFrontCover { get; set; }

        /// <summary>
        /// Back cover art recovered during the query process
        /// </summary>
        public MusicBrainzPicture? BestBackCover { get; set; }

        /// <summary>
        /// Completed MusicBrainz records (expensive queries)
        /// </summary>
        public IEnumerable<MusicBrainzCombinedLibraryEntryRecord> MusicBrainzCombinedLibraryEntryRecords { get; set; }

        /// <summary>
        /// Final record selection for the entry
        /// </summary>
        public MusicBrainzCombinedLibraryEntryRecord FinalQueryRecord { get; set; }

        /// <summary>
        /// Music Brainz combined record was queried successfully
        /// </summary>
        public bool MusicBrainzCombinedRecordQuerySuccess { get; set; }
    }
}

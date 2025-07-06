using AcoustID.Web;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderImportFileResult
    {
        /// <summary>
        /// Destination folder based on configuration settings (DirectoryBase + (music / audio book / ...) + {calculated})
        /// </summary>
        public string DestinationFolderBase { get; set; }

        /// <summary>
        /// Mp3 file destination path (FULL PATH)
        /// </summary>
        public string DestinationPathCalculated { get; set; }

        /// <summary>
        /// Extra folders (based on the destination folder base) to create or make sure are
        /// created before moving the file to its destination.
        /// </summary>
        public IEnumerable<string> DestinationSubFolders { get; set; }

        /// <summary>
        /// Results for AcoustID fingerprinting
        /// </summary>
        public IEnumerable<LookupResult> AcoustIDResults { get; set; }

        /// <summary>
        /// Matches for the AcoustID fingerprints
        /// </summary>
        public IEnumerable<MusicBrainzRecording> MusicBrainzRecordingMatches { get; set; }

        /// <summary>
        /// Completed MusicBrainz records (expensive queries)
        /// </summary>
        public IEnumerable<MusicBrainzCombinedLibraryEntryRecord> MusicBrainzCombinedLibraryEntryRecords { get; set; }

        /// <summary>
        /// Final record selection for the entry
        /// </summary>
        public MusicBrainzCombinedLibraryEntryRecord FinalQueryRecord { get; set; }

        /// <summary>
        /// Front cover art recovered during the query process
        /// </summary>
        public MusicBrainzPicture? BestFrontCover { get; set; }

        /// <summary>
        /// Back cover art recovered during the query process
        /// </summary>
        public MusicBrainzPicture? BestBackCover { get; set; }

        /// <summary>
        /// Final record imported as Mp3FileReference
        /// </summary>
        public Mp3FileReference ImportedRecord { get; set; }

        /// <summary>
        /// AcoustID fingerprinting successful
        /// </summary>
        public bool AcoustIDSuccess { get; set; }

        /// <summary>
        /// Music Brainz record was matched successfully
        /// </summary>
        public bool MusicBrainzRecordingMatchSuccess { get; set; }

        /// <summary>
        /// Music Brainz combined record was queried successfully
        /// </summary>
        public bool MusicBrainzCombinedRecordQuerySuccess { get; set; }

        /// <summary>
        /// Final record stored as tag successfully
        /// </summary>
        public bool TagEmbeddingSuccess { get; set; }

        /// <summary>
        /// Mp3 file successfully moved into library folder
        /// </summary>
        public bool Mp3FileMoveSuccess { get; set; }

        /// <summary>
        /// Mp3 file successfully imported into the database
        /// </summary>
        public bool Mp3FileImportSuccess { get; set; }
    }
}

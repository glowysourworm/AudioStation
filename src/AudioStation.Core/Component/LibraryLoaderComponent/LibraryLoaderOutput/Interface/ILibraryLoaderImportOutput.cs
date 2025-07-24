using AcoustID.Web;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput.Interface
{
    public interface ILibraryLoaderImportOutput
    {
        /// <summary>
        /// Destination folder based on configuration settings (DirectoryBase + (music / audio book / ...) + {calculated})
        /// </summary>
        string DestinationFolderBase { get; set; }

        /// <summary>
        /// Mp3 file destination path (FULL PATH)
        /// </summary>
        string DestinationPathCalculated { get; set; }

        /// <summary>
        /// Results for AcoustID fingerprinting
        /// </summary>
        IEnumerable<LookupResult> AcoustIDResults { get; set; }

        /// <summary>
        /// Matches for the AcoustID fingerprints
        /// </summary>
        IEnumerable<MusicBrainzRecording> MusicBrainzRecordingMatches { get; set; }

        /// <summary>
        /// Final record imported as Mp3FileReference
        /// </summary>
        Mp3FileReference ImportedRecord { get; set; }

        /// <summary>
        /// AcoustID fingerprinting successful
        /// </summary>
        bool AcoustIDSuccess { get; set; }

        /// <summary>
        /// Music Brainz record was matched successfully
        /// </summary>
        bool MusicBrainzRecordingMatchSuccess { get; set; }

        /// <summary>
        /// Final record stored as tag successfully
        /// </summary>
        bool TagEmbeddingSuccess { get; set; }

        /// <summary>
        /// Mp3 file successfully moved into library folder
        /// </summary>
        bool Mp3FileMoveSuccess { get; set; }

        /// <summary>
        /// Mp3 file successfully imported into the database
        /// </summary>
        bool Mp3FileImportSuccess { get; set; }

        /// <summary>
        /// Front cover art recovered during the query process
        /// </summary>
        MusicBrainzPicture? BestFrontCover { get; set; }

        /// <summary>
        /// Back cover art recovered during the query process
        /// </summary>
        MusicBrainzPicture? BestBackCover { get; set; }

        /// <summary>
        /// Completed MusicBrainz records (expensive queries)
        /// </summary>
        IEnumerable<MusicBrainzCombinedLibraryEntryRecord> MusicBrainzCombinedLibraryEntryRecords { get; set; }

        /// <summary>
        /// Final record selection for the entry
        /// </summary>
        MusicBrainzCombinedLibraryEntryRecord FinalQueryRecord { get; set; }

        /// <summary>
        /// Music Brainz combined record was queried successfully
        /// </summary>
        bool MusicBrainzCombinedRecordQuerySuccess { get; set; }
    }
}

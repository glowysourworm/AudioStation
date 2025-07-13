using AcoustID.Web;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderImportBasicOutput : LibraryLoaderOutputBase
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
        /// Final record imported as Mp3FileReference
        /// </summary>
        public Mp3FileReference ImportedRecord { get; set; }

        /// <summary>
        /// Mp3 file used during the import. This would represent the final tag file
        /// imported into the library.
        /// </summary>
        public TagLib.File ImportedTagFile { get; set; }

        // These need to be worked out (some refactoring)
        public bool ImportedTagFileAvailable { get; set; }
        public bool ImportedTagFileLoadError { get; set; }
        public string ImportedTagFileErrorMessage { get; set; }

        /// <summary>
        /// AcoustID fingerprinting successful
        /// </summary>
        public bool AcoustIDSuccess { get; set; }

        /// <summary>
        /// Music Brainz record was matched successfully
        /// </summary>
        public bool MusicBrainzRecordingMatchSuccess { get; set; }

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

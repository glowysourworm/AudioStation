using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input.Interface
{
    /// <summary>
    /// The import load should carry all the information required to (copy|move) a file from a source
    /// directory to a destination directory. Options: (Migration) Delete Unused Folder(s); 
    /// (Migration) Delete Source File; (Migration) Overwrite Destination File.
    /// </summary>
    public interface ILibraryLoaderImportPayload
    {
        /// <summary>
        /// The reference ID for tag information should be used during import. This will facilitate
        /// the read-only options in the library configuration. 
        /// </summary>
        int TagSmallId { get; set; }

        public LibraryImportType ImportType { get; set; }
        public LibraryImportSource TagSourcePreference { get; set; }

        public string SourceFullPath { get; set; }
        public string DestinationFolder { get; set; }

        public TrackCategory TrackCategory { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }

        public string MigrationSourceDirectory { get; set; }
        public bool MigrationDeleteSourceFiles { get; set; }
        public bool MigrationDeleteSourceFolders { get; set; }
        public bool MigrationOverwriteDestinationFiles { get; set; }

        public bool ServiceIncludeAcoustID { get; set; }
        public bool ServiceIncludeMusicBrainzBasic { get; set; }
        public bool ServiceIncludeMusicBrainzArtwork { get; set; }
        public bool ServiceOverwriteAcoustID { get; set; }
        public bool ServiceOverwriteMusicBrainzBasic { get; set; }
        public bool ServiceOverwriteMusicBrainzArtwork { get; set; }

        public bool LibraryOverwriteExistingFiles { get; set; }
        public bool LibraryOverwriteExistingTracks { get; set; }
        public bool LibraryOverwriteExistingAlbums { get; set; }
        public bool LibraryOverwriteExistingArtists { get; set; }
        public bool LibraryOverwriteExistingGenres { get; set; }

        public AudioEncoderInfo ImportFormat { get; set; }
        public bool ConvertAudioFormat { get; set; }

        /// <summary>
        /// This will protect any Library Directories or Application Directories in the configuration.
        /// </summary>
        public bool IsSourceDirectoryReadonly { get; set; }
    }
}

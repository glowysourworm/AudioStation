using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface
{
    /// <summary>
    /// The import load should carry all the information required to (copy|move) a file from a source
    /// directory to a destination directory. Options: (Migration) Delete Unused Folder(s); 
    /// (Migration) Delete Source File; (Migration) Overwrite Destination File.
    /// </summary>
    public interface ILibraryLoaderImportLoad
    {
        /// <summary>
        /// The reference ID for tag information should be used during import. This will facilitate
        /// the read-only options in the library configuration. 
        /// </summary>
        int TagSmallId { get; set; }

        string SourceFullPath { get; set; }
        string DestinationFolder { get; set; }

        AudioEncoderInfo DestinationFormat { get; set; }
        bool ConvertAudioFormat { get; set; }

        public TrackCategory TrackCategory { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }

        public string MigrationSourceDirectory { get; set; }
        public bool MigrationDeleteSourceFiles { get; set; }
        public bool MigrationDeleteSourceFolders { get; set; }
        public bool MigrationOverwriteDestinationFiles { get; set; }

        /// <summary>
        /// This will protect any Library Directories or Application Directories in the configuration.
        /// </summary>
        public bool IsSourceDirectoryReadonly { get; set; }
    }
}

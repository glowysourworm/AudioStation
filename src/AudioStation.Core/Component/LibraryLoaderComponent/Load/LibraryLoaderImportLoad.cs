using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderImportLoad : ILibraryLoaderImportLoad
    {
        public int TagSmallId { get; set; }

        public string SourceFullPath { get; set; }
        public string DestinationFolder { get; set; }

        public TrackCategory TrackCategory { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }

        public string MigrationSourceDirectory { get; set; }
        public bool MigrationDeleteSourceFiles { get; set; }
        public bool MigrationDeleteSourceFolders { get; set; }
        public bool MigrationOverwriteDestinationFiles { get; set; }
        public bool IsSourceDirectoryReadonly { get; set; }

        public AudioEncoderInfo DestinationFormat { get; set; }
        public bool ConvertAudioFormat { get; set; }
    }
}

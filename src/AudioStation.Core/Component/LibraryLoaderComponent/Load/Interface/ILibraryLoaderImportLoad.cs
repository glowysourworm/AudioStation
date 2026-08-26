using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface
{
    public interface ILibraryLoaderImportLoad
    {
        string SourceFile { get; set; }
        string SourceFolder { get; set; }
        string DestinationFolder { get; set; }
        TrackGroupingType GroupingType { get; set; }
        TrackNamingType NamingType { get; set; }
        bool IncludeMusicBrainzDetail { get; set; }
        bool IdentifyUsingAcoustID { get; set; }
        bool ImportFileMigration { get; set; }
        bool MigrationDeleteSourceFiles { get; set; }
        bool MigrationDeleteSourceFolders { get; set; }
        bool MigrationOverwriteDestinationFiles { get; set; }
    }
}

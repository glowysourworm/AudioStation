using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad.Interface
{
    public interface ILibraryLoaderImportLoad
    {
        string SourceFile { get; set; }
        string SourceFolder { get; set; }
        string DestinationFolder { get; set; }
        LibraryEntryGroupingType GroupingType { get; set; }
        LibraryEntryNamingType NamingType { get; set; }
        bool IncludeMusicBrainzDetail { get; set; }
        bool IdentifyUsingAcoustID { get; set; }
        bool ImportFileMigration { get; set; }
        bool MigrationDeleteSourceFiles { get; set; }
        bool MigrationDeleteSourceFolders { get; set; }
        bool MigrationOverwriteDestinationFiles { get; set; }
    }
}

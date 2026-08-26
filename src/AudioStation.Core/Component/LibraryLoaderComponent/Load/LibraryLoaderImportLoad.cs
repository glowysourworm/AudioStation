using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderImportLoad : ILibraryLoaderImportLoad
    {
        public string SourceFile { get; set; }
        public string SourceFolder { get; set; }
        public string DestinationFolder { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }
        public bool IncludeMusicBrainzDetail { get; set; }
        public bool IdentifyUsingAcoustID { get; set; }
        public bool ImportFileMigration { get; set; }
        public bool MigrationDeleteSourceFiles { get; set; }
        public bool MigrationDeleteSourceFolders { get; set; }
        public bool MigrationOverwriteDestinationFiles { get; set; }

        public LibraryLoaderImportLoad(string sourceFolder,
                                           string destinationFolder,
                                           string sourceFile,
                                           TrackGroupingType groupingType,
                                           TrackNamingType namingType,
                                           bool includeMusicBrainzDetail,
                                           bool identifyUsingAcoustID,
                                           bool importFileMigration,
                                           bool migrationDeleteSourceFiles,
                                           bool migrationDeleteSourceFolders,
                                           bool migrationOverwriteDestinationFiles)
        {
            this.SourceFolder = sourceFolder;
            this.DestinationFolder = destinationFolder;
            this.GroupingType = groupingType;
            this.NamingType = namingType;
            this.IncludeMusicBrainzDetail = includeMusicBrainzDetail;
            this.IdentifyUsingAcoustID = identifyUsingAcoustID;
            this.ImportFileMigration = importFileMigration;
            this.MigrationDeleteSourceFolders = migrationDeleteSourceFolders;
            this.MigrationDeleteSourceFiles = migrationDeleteSourceFiles;
            this.MigrationOverwriteDestinationFiles = migrationOverwriteDestinationFiles;

            this.SourceFile = sourceFile;
        }
    }
}

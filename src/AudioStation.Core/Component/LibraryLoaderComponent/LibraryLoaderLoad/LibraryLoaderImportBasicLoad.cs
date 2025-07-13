using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderImportBasicLoad : LibraryLoaderLoadBase
    {
        public string SourceFile { get; set; }
        public string SourceFolder { get; set; }
        public string DestinationFolder { get; set; }
        public LibraryEntryGroupingType GroupingType { get; set; }
        public LibraryEntryNamingType NamingType { get; set; }
        public bool IncludeMusicBrainzDetail { get; set; }
        public bool IdentifyUsingAcoustID { get; set; }
        public bool ImportFileMigration { get; set; }
        public bool MigrationDeleteSourceFiles { get; set; }
        public bool MigrationDeleteSourceFolders { get; set; }
        public bool MigrationOverwriteDestinationFiles { get; set; }

        public LibraryLoaderImportBasicLoad(string sourceFolder,
                                            string destinationFolder,
                                            string sourceFile,
                                            LibraryEntryGroupingType groupingType,
                                            LibraryEntryNamingType namingType,
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

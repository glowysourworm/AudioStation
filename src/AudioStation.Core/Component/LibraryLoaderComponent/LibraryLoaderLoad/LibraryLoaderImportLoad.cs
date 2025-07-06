using AudioStation.Core.Model;

using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderImportLoad : LibraryLoaderLoadBase
    {
        protected SimpleDictionary<string, bool> SourceFiles { get; set; }
        protected SimpleDictionary<string, bool> CompletedFiles { get; set; }
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
        public bool TestOnly { get; set; }

        public LibraryLoaderImportLoad(string sourceFolder,
                                       string destinationFolder,
                                       IEnumerable<string> sourceFiles,
                                       LibraryEntryGroupingType groupingType,
                                       LibraryEntryNamingType namingType,
                                       bool includeMusicBrainzDetail,
                                       bool identifyUsingAcoustID,
                                       bool importFileMigration,
                                       bool migrationDeleteSourceFiles,
                                       bool migrationDeleteSourceFolders,
                                       bool migrationOverwriteDestinationFiles,
                                       bool testOnly)
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
            this.TestOnly = testOnly;

            this.SourceFiles = new SimpleDictionary<string, bool>();
            this.CompletedFiles = new SimpleDictionary<string, bool>();

            foreach (var file in sourceFiles)
            {
                this.SourceFiles.Add(file, false);
            }
        }

        public IEnumerable<string> GetSourceFiles()
        {
            return this.SourceFiles.Keys;
        }

        public void SetResult(string file, bool success)
        {
            if (!this.CompletedFiles.ContainsKey(file))
                this.CompletedFiles.Add(file, success);

            else
                throw new Exception("Set result for import file more than once");

            // Mark completed (also)
            this.SourceFiles[file] = true;
        }

        public override int GetFailureCount()
        {
            return this.CompletedFiles.Count(pair => !pair.Value);
        }

        public override double GetProgress()
        {
            if (this.SourceFiles.Count == 0)
                return 1;

            return this.CompletedFiles.Count / (double)this.SourceFiles.Count;
        }

        public override int GetSuccessCount()
        {
            return this.CompletedFiles.Count(pair => pair.Value);
        }

        public override bool HasErrors()
        {
            return GetFailureCount() > 0;
        }
    }
}

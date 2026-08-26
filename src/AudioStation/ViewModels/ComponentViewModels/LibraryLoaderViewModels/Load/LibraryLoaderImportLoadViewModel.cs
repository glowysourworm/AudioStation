using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderImportLoadViewModel : ViewModelBase, ILibraryLoaderImportLoad
    {
        string _sourceFile;
        string _sourceFolder;
        string _destinationFolder;
        TrackGroupingType _groupingType;
        TrackNamingType _namingType;
        bool _includeMusicBrainzDetail;
        bool _identifyUsingAcoustID;
        bool _importFileMigration;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        public string SourceFile
        {
            get { return _sourceFile; }
            set { RaiseAndSetIfChanged(ref _sourceFile, value); }
        }
        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { RaiseAndSetIfChanged(ref _sourceFolder, value); }
        }
        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { RaiseAndSetIfChanged(ref _destinationFolder, value); }
        }
        public TrackGroupingType GroupingType
        {
            get { return _groupingType; }
            set { RaiseAndSetIfChanged(ref _groupingType, value); }
        }
        public TrackNamingType NamingType
        {
            get { return _namingType; }
            set { RaiseAndSetIfChanged(ref _namingType, value); }
        }
        public bool IncludeMusicBrainzDetail
        {
            get { return _includeMusicBrainzDetail; }
            set { RaiseAndSetIfChanged(ref _includeMusicBrainzDetail, value); }
        }
        public bool IdentifyUsingAcoustID
        {
            get { return _identifyUsingAcoustID; }
            set { RaiseAndSetIfChanged(ref _identifyUsingAcoustID, value); }
        }
        public bool ImportFileMigration
        {
            get { return _importFileMigration; }
            set { RaiseAndSetIfChanged(ref _importFileMigration, value); }
        }
        public bool MigrationDeleteSourceFiles
        {
            get { return _migrationDeleteSourceFiles; }
            set { RaiseAndSetIfChanged(ref _migrationDeleteSourceFiles, value); }
        }
        public bool MigrationDeleteSourceFolders
        {
            get { return _migrationDeleteSourceFolders; }
            set { RaiseAndSetIfChanged(ref _migrationDeleteSourceFolders, value); }
        }
        public bool MigrationOverwriteDestinationFiles
        {
            get { return _migrationOverwriteDestinationFiles; }
            set { RaiseAndSetIfChanged(ref _migrationOverwriteDestinationFiles, value); }
        }

        public LibraryLoaderImportLoadViewModel()
        {
            this.SourceFile = string.Empty;
            this.SourceFolder = string.Empty;
            this.DestinationFolder = string.Empty;
        }

        public override string ToString()
        {
            return _sourceFile;
        }
    }
}

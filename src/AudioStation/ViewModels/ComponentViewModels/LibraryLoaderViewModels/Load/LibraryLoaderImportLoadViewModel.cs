using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderImportLoadViewModel : ViewModelBase, ILibraryLoaderImportLoad
    {
        string _sourceFullPath;
        string _destinationFolder;
        TrackCategory _trackCategory;
        TrackGroupingType _groupingType;
        TrackNamingType _namingType;
        string _migrationSourceDirectory;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;
        int _tagSmallId;

        bool _isSourceDirectoryReadonly;

        public string SourceFullPath
        {
            get { return _sourceFullPath; }
            set { this.RaiseAndSetIfChanged(ref _sourceFullPath, value); }
        }
        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolder, value); }
        }
        public TrackCategory TrackCategory
        {
            get { return _trackCategory; }
            set { this.RaiseAndSetIfChanged(ref _trackCategory, value); }
        }
        public TrackGroupingType GroupingType
        {
            get { return _groupingType; }
            set { this.RaiseAndSetIfChanged(ref _groupingType, value); }
        }
        public TrackNamingType NamingType
        {
            get { return _namingType; }
            set { this.RaiseAndSetIfChanged(ref _namingType, value); }
        }
        public string MigrationSourceDirectory
        {
            get { return _migrationSourceDirectory; }
            set { this.RaiseAndSetIfChanged(ref _migrationSourceDirectory, value); }
        }
        public bool MigrationDeleteSourceFiles
        {
            get { return _migrationDeleteSourceFiles; }
            set { this.RaiseAndSetIfChanged(ref _migrationDeleteSourceFiles, value); }
        }
        public bool MigrationDeleteSourceFolders
        {
            get { return _migrationDeleteSourceFolders; }
            set { this.RaiseAndSetIfChanged(ref _migrationDeleteSourceFolders, value); }
        }
        public bool MigrationOverwriteDestinationFiles
        {
            get { return _migrationOverwriteDestinationFiles; }
            set { this.RaiseAndSetIfChanged(ref _migrationOverwriteDestinationFiles, value); }
        }
        public int TagSmallId
        {
            get { return _tagSmallId; }
            set { this.RaiseAndSetIfChanged(ref _tagSmallId, value); }
        }
        public bool IsSourceDirectoryReadonly
        {
            get { return _isSourceDirectoryReadonly; }
            set { this.RaiseAndSetIfChanged(ref _isSourceDirectoryReadonly, value); }
        }


        public LibraryLoaderImportLoadViewModel()
        {
            this.SourceFullPath = string.Empty;
            this.DestinationFolder = string.Empty;
            this.MigrationSourceDirectory = string.Empty;
        }

        public override string ToString()
        {
            return this.SourceFullPath;
        }
    }
}

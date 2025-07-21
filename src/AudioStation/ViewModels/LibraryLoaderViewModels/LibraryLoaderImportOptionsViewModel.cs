using AudioStation.Controller.Interface;
using AudioStation.Core.Model;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportOptionsViewModel : ViewModelBase
    {
        private readonly IDialogController _dialogController;

        string _sourceFolderSearch;
        string _sourceFolder;
        string _destinationFolderSearch;
        string _destinationFolder;
        string _destinationMusicSubFolder;
        string _destinationAudioBooksSubFolder;

        LibraryEntryType _importAsType;
        LibraryEntryGroupingType _groupingType;
        LibraryEntryNamingType _namingType;

        NotifyingObservableCollection<LibraryLoaderImportFileViewModel> _sourceFiles;

        bool _includeMusicBrainzDetail;
        bool _identifyUsingAcoustID;

        bool _importFileMigration;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;

        public string SourceFolderSearch
        {
            get { return _sourceFolderSearch; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolderSearch, value); }
        }
        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolder, value); }
        }
        public string DestinationFolderSearch
        {
            get { return _destinationFolderSearch; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolderSearch, value); }
        }
        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolder, value); }
        }
        public string DestinationMusicSubFolder
        {
            get { return _destinationMusicSubFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationMusicSubFolder, value); }
        }
        public string DestinationAudioBooksSubFolder
        {
            get { return _destinationAudioBooksSubFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationAudioBooksSubFolder, value); }
        }
        public LibraryEntryType ImportAsType
        {
            get { return _importAsType; }
            set { this.RaiseAndSetIfChanged(ref _importAsType, value); }
        }
        public LibraryEntryGroupingType GroupingType
        {
            get { return _groupingType; }
            set { this.RaiseAndSetIfChanged(ref _groupingType, value); }
        }
        public LibraryEntryNamingType NamingType
        {
            get { return _namingType; }
            set { this.RaiseAndSetIfChanged(ref _namingType, value); }
        }
        public bool IncludeMusicBrainzDetail
        {
            get { return _includeMusicBrainzDetail; }
            set { this.RaiseAndSetIfChanged(ref _includeMusicBrainzDetail, value); }
        }
        public bool IdentifyUsingAcoustID
        {
            get { return _identifyUsingAcoustID; }
            set { this.RaiseAndSetIfChanged(ref _identifyUsingAcoustID, value); }
        }
        public bool ImportFileMigration
        {
            get { return _importFileMigration; }
            set { this.RaiseAndSetIfChanged(ref _importFileMigration, value); }
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
        public int SourceFileSelectedCount
        {
            get { return _sourceFiles.Count(x => x.IsSelected); }
            set { OnPropertyChanged("SourceFileSelectedCount"); }
        }
        public SimpleCommand SelectSourceFolderCommand
        {
            get { return _selectSourceFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectSourceFolderCommand, value); }
        }

        public LibraryLoaderImportOptionsViewModel(IDialogController dialogController)
        {
            _dialogController = dialogController;

            this.SelectSourceFolderCommand = new SimpleCommand(() =>
            {
                var originalFolder = this.SourceFolder;
                var folder = dialogController.ShowSelectFolder();

                if (!string.IsNullOrEmpty(folder))
                {
                    this.SourceFolder = folder;
                }
            });
        }
    }
}

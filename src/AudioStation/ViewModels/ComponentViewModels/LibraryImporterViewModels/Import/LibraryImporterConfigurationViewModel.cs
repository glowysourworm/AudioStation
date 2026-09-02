using AudioStation.ViewModels.MainViewModels;

using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    public class LibraryImporterConfigurationViewModel : ViewModelBase
    {
        LibraryDirectoryViewModel _sourceDirectory;
        LibraryDirectoryViewModel _destinationDirectory;

        bool _includeMusicBrainzDetail;
        bool _identifyUsingAcoustID;

        bool _importFileMigration;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;

        public LibraryDirectoryViewModel SourceDirectory
        {
            get { return _sourceDirectory; }
            set { this.RaiseAndSetIfChanged(ref _sourceDirectory, value); }
        }
        public LibraryDirectoryViewModel DestinationDirectory
        {
            get { return _destinationDirectory; }
            set { this.RaiseAndSetIfChanged(ref _destinationDirectory, value); }
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

        public SimpleCommand SelectSourceFolderCommand
        {
            get { return _selectSourceFolderCommand; }
            set { RaiseAndSetIfChanged(ref _selectSourceFolderCommand, value); }
        }

        public LibraryImporterConfigurationViewModel()
        {
            this.SourceDirectory = new LibraryDirectoryViewModel();
            this.DestinationDirectory = new LibraryDirectoryViewModel();

            this.SelectSourceFolderCommand = new SimpleCommand(() =>
            {
                //var originalFolder = this.SourceFolder;
                //var folder = dialogController.ShowSelectFolder();

                //if (!string.IsNullOrEmpty(folder))
                //{
                //    this.SourceFolder = folder;
                //}
            });
        }
    }
}

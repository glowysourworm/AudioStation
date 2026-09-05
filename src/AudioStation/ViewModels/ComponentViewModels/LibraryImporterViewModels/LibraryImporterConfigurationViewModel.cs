using AudioStation.Core.Model;
using AudioStation.ViewModels.MainViewModels;

using Microsoft.Win32;

using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public class LibraryImporterConfigurationViewModel : ViewModelBase
    {
        // Library Directorys (only)
        LibraryDirectoryViewModel _importDirectory;

        LibraryImportType _importType;

        // Service Options
        bool _identifyUsingAcoustID;
        bool _identifyUsingMusicBrainz;
        bool _includeMusicBrainzArtwork;

        // Migration
        string _migrationSourceDirectory;
        bool _migrationConvertAudioFiles;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;

        public LibraryDirectoryViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public LibraryImportType ImportType
        {
            get { return _importType; }
            set { this.RaiseAndSetIfChanged(ref _importType, value); }
        }
        public bool IdentifyUsingAcoustID
        {
            get { return _identifyUsingAcoustID; }
            set { this.RaiseAndSetIfChanged(ref _identifyUsingAcoustID, value); }
        }
        public bool IdentifyUsingMusicBrainz
        {
            get { return _identifyUsingMusicBrainz; }
            set { this.RaiseAndSetIfChanged(ref _identifyUsingMusicBrainz, value); }
        }
        public bool IncludeMusicBrainzArtwork
        {
            get { return _includeMusicBrainzArtwork; }
            set { this.RaiseAndSetIfChanged(ref _includeMusicBrainzArtwork, value); }
        }
        public string MigrationSourceDirectory
        {
            get { return _migrationSourceDirectory; }
            set { this.RaiseAndSetIfChanged(ref _migrationSourceDirectory, value); }
        }
        public bool MigrationConvertAudioFiles
        {
            get { return _migrationConvertAudioFiles; }
            set { this.RaiseAndSetIfChanged(ref _migrationConvertAudioFiles, value); }
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

        public SimpleCommand SelectSourceFolderCommand
        {
            get { return _selectSourceFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectSourceFolderCommand, value); }
        }

        public LibraryImporterConfigurationViewModel()
        {
            this.ImportDirectory = new LibraryDirectoryViewModel();
            this.ImportType = LibraryImportType.InPlaceDirectory;

            this.SelectSourceFolderCommand = new SimpleCommand(() =>
            {
                var dialog = new OpenFolderDialog();

                dialog.Multiselect = false;

                if (dialog.ShowDialog() == true)
                {
                    this.MigrationSourceDirectory = dialog.FolderName;
                }
            });
        }
    }
}

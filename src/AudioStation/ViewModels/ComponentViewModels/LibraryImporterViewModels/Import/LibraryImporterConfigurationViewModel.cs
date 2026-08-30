using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;

using SimpleWpf.Extensions.Command;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    public class LibraryImporterConfigurationViewModel : ViewModelBase
    {
        string _sourceFolder;
        string _destinationFolder;

        bool _includeMusicBrainzDetail;
        bool _identifyUsingAcoustID;

        bool _importFileMigration;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;

        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { RaiseAndSetIfChanged(ref _destinationFolder, value); }
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

        public LibraryImporterConfigurationViewModel(IAudioStationConfigurationManager configurationManager, IDialogController dialogController)
        {
            var configuration = configurationManager.GetConfiguration();

            //this.SourceFolder = configuration.StagingFolder.Dir%ectory;
            //this.DestinationFolder = configuration.DirectoryBase;
            //this.DestinationMusicSubFolder = configuration.MusicSubDirectory;
            //this.DestinationAudioBooksSubFolder = configuration.AudioBooksSubDirectory;

            //this.ImportAsType = TrackType.Music;
            //this.GroupingType = TrackGroupingType.ArtistAlbum;
            //this.NamingType = TrackNamingType.Standard;

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

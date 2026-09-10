using AudioStation.Core.Model;
using AudioStation.ViewModels.MainViewModels;

using Microsoft.Win32;

using Newtonsoft.Json;

using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public class LibraryImporterConfigurationViewModel : ViewModelBase
    {
        // Library Directorys (only)
        LibraryDirectoryViewModel _importDirectory;

        // Tag Source Options
        LibraryImportTagSource _tagSourcePreference;

        // Tag Service Options
        bool _serviceIncludeAcoustID;
        bool _serviceIncludeMusicBrainzBasic;
        bool _serviceIncludeMusicBrainzArtwork;
        bool _serviceOverwriteAcoustID;                 // AcoustIDLookupResult
        bool _serviceOverwriteMusicBrainzBasic;         // TagSmall
        bool _serviceOverwriteMusicBrainzArtwork;       // Artwork files in the cache directory

        // Convert Audio Format (to folder preference)
        bool _convertAudioFormat;

        // Library Record Conflict Options (some of these are "double-checks" with migration)
        bool _libraryOverwriteExistingFiles;
        bool _libraryOverwriteExistingTracks;
        bool _libraryOverwriteExistingAlbums;
        bool _libraryOverwriteExistingArtists;
        bool _libraryOverwriteExistingGenres;

        // Migration
        string _migrationSourceDirectory;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;

        public LibraryDirectoryViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public LibraryImportTagSource TagSourcePreference
        {
            get { return _tagSourcePreference; }
            set { this.RaiseAndSetIfChanged(ref _tagSourcePreference, value); }
        }
        public bool ServiceIncludeAcoustID
        {
            get { return _serviceIncludeAcoustID; }
            set { this.RaiseAndSetIfChanged(ref _serviceIncludeAcoustID, value); }
        }
        public bool ServiceIncludeMusicBrainzBasic
        {
            get { return _serviceIncludeMusicBrainzBasic; }
            set { this.RaiseAndSetIfChanged(ref _serviceIncludeMusicBrainzBasic, value); }
        }
        public bool ServiceIncludeMusicBrainzArtwork
        {
            get { return _serviceIncludeMusicBrainzArtwork; }
            set { this.RaiseAndSetIfChanged(ref _serviceIncludeMusicBrainzArtwork, value); }
        }
        public bool ServiceOverwriteAcoustID
        {
            get { return _serviceOverwriteAcoustID; }
            set { this.RaiseAndSetIfChanged(ref _serviceOverwriteAcoustID, value); }
        }
        public bool ServiceOverwriteMusicBrainzBasic
        {
            get { return _serviceOverwriteMusicBrainzBasic; }
            set { this.RaiseAndSetIfChanged(ref _serviceOverwriteMusicBrainzBasic, value); }
        }
        public bool ServiceOverwriteMusicBrainzArtwork
        {
            get { return _serviceOverwriteMusicBrainzArtwork; }
            set { this.RaiseAndSetIfChanged(ref _serviceOverwriteMusicBrainzArtwork, value); }
        }
        public bool ConvertAudioFormat
        {
            get { return _convertAudioFormat; }
            set { this.RaiseAndSetIfChanged(ref _convertAudioFormat, value); }
        }
        public bool LibraryOverwriteExistingFiles
        {
            get { return _libraryOverwriteExistingFiles; }
            set { this.RaiseAndSetIfChanged(ref _libraryOverwriteExistingFiles, value); }
        }
        public bool LibraryOverwriteExistingTracks
        {
            get { return _libraryOverwriteExistingTracks; }
            set { this.RaiseAndSetIfChanged(ref _libraryOverwriteExistingTracks, value); }
        }
        public bool LibraryOverwriteExistingAlbums
        {
            get { return _libraryOverwriteExistingAlbums; }
            set { this.RaiseAndSetIfChanged(ref _libraryOverwriteExistingAlbums, value); }
        }
        public bool LibraryOverwriteExistingArtists
        {
            get { return _libraryOverwriteExistingArtists; }
            set { this.RaiseAndSetIfChanged(ref _libraryOverwriteExistingArtists, value); }
        }
        public bool LibraryOverwriteExistingGenres
        {
            get { return _libraryOverwriteExistingGenres; }
            set { this.RaiseAndSetIfChanged(ref _libraryOverwriteExistingGenres, value); }
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

        [JsonIgnore]
        public SimpleCommand SelectSourceFolderCommand
        {
            get { return _selectSourceFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectSourceFolderCommand, value); }
        }



        public LibraryImporterConfigurationViewModel()
        {
            this.ImportDirectory = new LibraryDirectoryViewModel();

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

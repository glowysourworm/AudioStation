using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderImportLoadViewModel : ViewModelBase, ILibraryLoaderImportLoad
    {
        int _ownerId;
        int _tagSmallId;

        LibraryLoadType _loadType;
        LibraryImportType _importType;
        AudioEncoderInfo _importFormat;

        string _sourceFullPath;
        string _destinationFolder;

        TrackCategory _trackCategory;
        TrackGroupingType _groupingType;
        TrackNamingType _namingType;

        bool _isSourceDirectoryReadonly;
        bool _embedImportTagData;

        // Tag Source Options
        LibraryImportSource _tagSourcePreference;
        LibraryImportSource _acoustIDSourcePreference;
        LibraryImportSource _musicBrainzSourcePreference;

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

        public int OwnerId
        {
            get { return _ownerId; }
            set { this.RaiseAndSetIfChanged(ref _ownerId, value); }
        }
        public int TagSmallId
        {
            get { return _tagSmallId; }
            set { this.RaiseAndSetIfChanged(ref _tagSmallId, value); }
        }
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
        public bool IsSourceDirectoryReadonly
        {
            get { return _isSourceDirectoryReadonly; }
            set { this.RaiseAndSetIfChanged(ref _isSourceDirectoryReadonly, value); }
        }
        public bool EmbedImportTagData
        {
            get { return _embedImportTagData; }
            set { this.RaiseAndSetIfChanged(ref _embedImportTagData, value); }
        }
        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { this.RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public LibraryImportType ImportType
        {
            get { return _importType; }
            set { this.RaiseAndSetIfChanged(ref _importType, value); }
        }
        public AudioEncoderInfo ImportFormat
        {
            get { return _importFormat; }
            set { this.RaiseAndSetIfChanged(ref _importFormat, value); }
        }
        public LibraryImportSource TagSourcePreference
        {
            get { return _tagSourcePreference; }
            set { this.RaiseAndSetIfChanged(ref _tagSourcePreference, value); }
        }
        public LibraryImportSource AcoustIDSourcePreference
        {
            get { return _acoustIDSourcePreference; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDSourcePreference, value); }
        }
        public LibraryImportSource MusicBrainzSourcePreference
        {
            get { return _musicBrainzSourcePreference; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzSourcePreference, value); }
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

        public LibraryLoaderImportLoadViewModel()
        {
            this.LoadType = LibraryLoadType.Import;
            this.SourceFullPath = string.Empty;
            this.DestinationFolder = string.Empty;
            this.MigrationSourceDirectory = string.Empty;
            this.ImportFormat = new AudioEncoderInfo();
        }

        public override string ToString()
        {
            return this.SourceFullPath;
        }
    }
}

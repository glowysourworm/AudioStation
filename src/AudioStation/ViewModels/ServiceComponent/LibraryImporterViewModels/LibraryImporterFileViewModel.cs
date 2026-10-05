using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.DataComponent.MainViewModels;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Payload.Input;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Payload.Output;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    public class LibraryImporterFileViewModel : ViewModelBase
    {
        private readonly IAudioStationMapper _audioStationMapper;

        // This will be used to indicate errors from the service workflow
        bool _serviceError;

        // The error or conflict flags will indicate the state of the import. These must
        // be resolved with the options selected by the user for import. Minimum validation
        // of the tag (track) data comes from the ITagSmall interface; and the records for
        // the Genre, Artist, Album, Track, and File must be available for the import to
        // be complete.
        //
        bool _libraryConflict;      // Reported as a conflict in database records
        bool _fileConflict;         // This is a conflict in the actual file

        // ID3v2 [TXXX] User tag information typically set by MusicBrainz applications (e.g. Picard)
        //              that was found during import. These id's are used to load the "TagRecord" and
        //              to expedite the import process.
        //
        // Tag Source(s):
        //
        // 1) File (readonly) (embed tag is an option after import completes)
        // 2) Music Brainz Id ([TXXX] data left over from importing tag in another application)
        // 3) Music Brainz / AcoustID Result(s)
        // 
        // -> TagRecordDirty (final write location)
        //
        //    This will be where we edit our "tag" until the import is ready. Then, it
        //    will be used to create our database entities (Track, Album, Artist, Genre, ..)
        //
        TagSmallViewModel _tag;                      // File
        TagSmallViewModel _tagMusicBrainz;           // Music Brainz (from [TXXX] tag ID)
        TagSmallViewModel _tagRecordClean;           // Final Record (readonly)
        TagSmallEditViewModel _tagRecordDirty;       // Final Record (edit)

        LibraryLoaderImportInputViewModel _importLoad;
        LibraryLoaderImportOutputViewModel _importOutput;

        bool _musicBrainzReleaseTrackQuerySuccess;
        Guid? _musicBrainzReleaseTrackIDTag;

        public bool ServiceError
        {
            get { return _serviceError; }
            set { this.RaiseAndSetIfChanged(ref _serviceError, value); }
        }
        public bool LibraryConflict
        {
            get { return _libraryConflict; }
            set { this.RaiseAndSetIfChanged(ref _libraryConflict, value); }
        }
        public bool FileConflict
        {
            get { return _fileConflict; }
            set { this.RaiseAndSetIfChanged(ref _fileConflict, value); }
        }
        public TagSmallViewModel Tag
        {
            get { return _tag; }
            set { this.RaiseAndSetIfChanged(ref _tag, value); }
        }
        public TagSmallViewModel TagMusicBrainz
        {
            get { return _tagMusicBrainz; }
            set { this.RaiseAndSetIfChanged(ref _tagMusicBrainz, value); }
        }
        public TagSmallViewModel TagRecordClean
        {
            get { return _tagRecordClean; }
            set { this.RaiseAndSetIfChanged(ref _tagRecordClean, value); }
        }
        public TagSmallEditViewModel TagRecordDirty
        {
            get { return _tagRecordDirty; }
            set { this.RaiseAndSetIfChanged(ref _tagRecordDirty, value); }
        }
        public LibraryLoaderImportInputViewModel ImportLoad
        {
            get { return _importLoad; }
            set { this.RaiseAndSetIfChanged(ref _importLoad, value); }
        }
        public LibraryLoaderImportOutputViewModel ImportOutput
        {
            get { return _importOutput; }
            set { this.RaiseAndSetIfChanged(ref _importOutput, value); }
        }

        public bool MusicBrainzReleaseTrackQuerySuccess
        {
            get { return _musicBrainzReleaseTrackQuerySuccess; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzReleaseTrackQuerySuccess, value); }
        }
        public Guid? MusicBrainzReleaseTrackIDTag
        {
            get { return _musicBrainzReleaseTrackIDTag; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzReleaseTrackIDTag, value); }
        }

        public LibraryImporterFileViewModel(string fileFullPath,
                                            string fileBaseDirectory,
                                            LibraryImporterFileTreeNodeViewModel? parent,
                                            LibraryImporterConfigurationViewModel importerConfiguration)
        {
            var dialogController = IocContainer.Get<IDialogController>();
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();

            this.ImportLoad = new LibraryLoaderImportInputViewModel()
            {
                ConvertAudioFormat = importerConfiguration.ConvertAudioFormat,
                DestinationFolder = importerConfiguration.ImportDirectory.Directory,
                EmbedImportTagData = importerConfiguration.EmbedImportTagData,
                GroupingType = importerConfiguration.ImportDirectory.FolderFormatType,
                ImportFormat = importerConfiguration.ImportFormat != null ?
                                    _audioStationMapper.Map<AudioEncoderViewModel, AudioEncoderInfo>(importerConfiguration.ImportFormat) :
                                    new AudioEncoderInfo(),
                ImportType = importerConfiguration.ImportType,
                IsSourceDirectoryReadonly = importerConfiguration.ImportDirectory.IsReadOnly,
                LibraryOverwriteExistingAlbums = importerConfiguration.LibraryOverwriteExistingAlbums,
                LibraryOverwriteExistingArtists = importerConfiguration.LibraryOverwriteExistingArtists,
                LibraryOverwriteExistingFiles = importerConfiguration.LibraryOverwriteExistingFiles,
                LibraryOverwriteExistingGenres = importerConfiguration.LibraryOverwriteExistingGenres,
                LibraryOverwriteExistingTracks = importerConfiguration.LibraryOverwriteExistingTracks,
                MigrationDeleteSourceFiles = importerConfiguration.MigrationDeleteSourceFiles,
                MigrationDeleteSourceFolders = importerConfiguration.MigrationDeleteSourceFolders,
                MigrationOverwriteDestinationFiles = importerConfiguration.MigrationOverwriteDestinationFiles,
                MigrationSourceDirectory = importerConfiguration.MigrationSourceDirectory,
                NamingType = importerConfiguration.ImportDirectory.FileFormatType,
                ServiceIncludeAcoustID = importerConfiguration.ServiceIncludeAcoustID,
                ServiceIncludeMusicBrainzArtwork = importerConfiguration.ServiceIncludeMusicBrainzArtwork,
                ServiceIncludeMusicBrainzBasic = importerConfiguration.ServiceIncludeMusicBrainzBasic,
                ServiceOverwriteAcoustID = importerConfiguration.ServiceOverwriteAcoustID,
                ServiceOverwriteMusicBrainzArtwork = importerConfiguration.ServiceOverwriteMusicBrainzArtwork,
                ServiceOverwriteMusicBrainzBasic = importerConfiguration.ServiceOverwriteMusicBrainzBasic,
                SourceFullPath = fileFullPath,
                TagFinal = new TagSmall(),
                TagSourcePreference = importerConfiguration.TagSourcePreference
            };

            this.Tag = new TagSmallViewModel();
            this.TagMusicBrainz = new TagSmallViewModel();
            this.TagRecordClean = new TagSmallViewModel();
            this.TagRecordDirty = new TagSmallEditViewModel();
            this.ImportOutput = new LibraryLoaderImportOutputViewModel();

            this.ImportOutput.PropertyChanged += ImportOutput_PropertyChanged;
            this.TagRecordDirty.PropertyChanged += TagRecordDirty_PropertyChanged;
        }

        private void ImportOutput_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Bubble Up
            OnPropertyChanged("ImportOutput");

            //OnPropertyChanged("FinalImportDetail");
            //OnPropertyChanged("TagDetail");
        }

        private void TagRecordDirty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Bubble Up
            OnPropertyChanged("TagRecordDirty");

            // Map -> ImportLoad
            _audioStationMapper.MapOnto(this.TagRecordDirty, this.ImportLoad.TagFinal);
        }

        public override string ToString()
        {
            return this.ImportLoad.SourceFullPath;
        }
    }
}

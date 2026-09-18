using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;
using AudioStation.ViewModels.MainViewModels;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    /// <summary>
    /// This is a UI data class for the library importer. It will operate on the staged files for the 
    /// import function - setting the Load / Output properties during load.
    /// </summary>
    public class LibraryLoaderImportViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioStationMapper _audioStationMapper;

        private readonly KeyedObservableCollection<string, LibraryImporterFileViewModel> _stagedFiles;
        private readonly LibraryImporterConfigurationViewModel _libraryImporterConfiguration;

        public LibraryLoaderImportViewModel(
            LibraryImporterConfigurationViewModel libraryImporterConfiguration,
            KeyedObservableCollection<string, LibraryImporterFileViewModel> stagedFiles)
            : base("Library Import Worker", "Library import worker task is for importing library records during an import workflow")
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();

            _stagedFiles = stagedFiles;
            _libraryImporterConfiguration = libraryImporterConfiguration;
        }

        protected override IEnumerable<LibraryLoaderLoad> CreateWorkLoads(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var result = new List<LibraryLoaderLoad>();
            var counter = 0;

            // Load / Output:  These are part of the workflow process. All of the import data is setup here
            //                 so that the view binding can happen without a big mess in the code. Also, the
            //                 back and forth with the backend for imports is kept clean by using these objects.
            //
            foreach (LibraryImporterFileViewModel stagedFile in _stagedFiles)
            {
                progressHandler(1, 1, _stagedFiles.Count(), counter++, "Staging: " + stagedFile.FullPath);

                result.Add(new LibraryLoaderLoad(LibraryLoadType.Import, new LibraryLoaderImportLoad(this.Id, LibraryLoadType.Import)
                {
                    TagSmallId = stagedFile.MusicBrainzTag.Id,

                    SourceFullPath = stagedFile.FullPath,
                    DestinationFolder = _libraryImporterConfiguration.ImportDirectory.Directory,

                    ConvertAudioFormat = _libraryImporterConfiguration.ConvertAudioFormat,
                    ImportFormat = _audioStationMapper.Map<AudioEncoderViewModel, AudioEncoderInfo>(_libraryImporterConfiguration.ImportFormat),

                    AcoustIDSourcePreference = _libraryImporterConfiguration.AcoustIDSourcePreference,
                    MusicBrainzSourcePreference = _libraryImporterConfiguration.MusicBrainzSourcePreference,
                    TagSourcePreference = _libraryImporterConfiguration.TagSourcePreference,

                    ServiceIncludeAcoustID = _libraryImporterConfiguration.ServiceIncludeAcoustID,
                    ServiceIncludeMusicBrainzBasic = _libraryImporterConfiguration.ServiceIncludeMusicBrainzBasic,
                    ServiceIncludeMusicBrainzArtwork = _libraryImporterConfiguration.ServiceIncludeMusicBrainzArtwork,

                    ServiceOverwriteAcoustID = _libraryImporterConfiguration.ServiceOverwriteAcoustID,
                    ServiceOverwriteMusicBrainzBasic = _libraryImporterConfiguration.ServiceOverwriteMusicBrainzBasic,
                    ServiceOverwriteMusicBrainzArtwork = _libraryImporterConfiguration.ServiceOverwriteMusicBrainzArtwork,

                    TrackCategory = _libraryImporterConfiguration.ImportDirectory.TrackCategory,
                    GroupingType = _libraryImporterConfiguration.ImportDirectory.GroupingType,
                    NamingType = _libraryImporterConfiguration.ImportDirectory.NamingType,

                    IsSourceDirectoryReadonly = _libraryImporterConfiguration.ImportDirectory.IsReadOnly,

                    LibraryOverwriteExistingAlbums = _libraryImporterConfiguration.LibraryOverwriteExistingAlbums,
                    LibraryOverwriteExistingArtists = _libraryImporterConfiguration.LibraryOverwriteExistingArtists,
                    LibraryOverwriteExistingFiles = _libraryImporterConfiguration.LibraryOverwriteExistingFiles,
                    LibraryOverwriteExistingGenres = _libraryImporterConfiguration.LibraryOverwriteExistingGenres,
                    LibraryOverwriteExistingTracks = _libraryImporterConfiguration.LibraryOverwriteExistingTracks,

                    MigrationDeleteSourceFiles = _libraryImporterConfiguration.MigrationDeleteSourceFiles,
                    MigrationDeleteSourceFolders = _libraryImporterConfiguration.MigrationDeleteSourceFolders,
                    MigrationOverwriteDestinationFiles = _libraryImporterConfiguration.MigrationOverwriteDestinationFiles,
                    MigrationSourceDirectory = _libraryImporterConfiguration.MigrationSourceDirectory,
                }));

                // PERFORMANCE ISSUE:  The tag data must be read; and minimal during file reading. The objects
                //                     involved must be small. So, we're going to try making "TagSmall" objects
                //                     in the cache to help out. And, we need to be sure that the tag library 
                //                     is optimized. (IdSharp seems to be fairly good)
                //

                // STAGED FILES:  The Load / Output view models are used for this file object. So, they
                //                will be set along with the results
            }

            return result;
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(LibraryLoaderLoad workLoad)
        {
            var importLoad = workLoad.Get<LibraryLoaderImportLoad>();

            return new LibraryLoaderLoadViewModel()
            {
                Data = _audioStationMapper.Map<LibraryLoaderImportLoad, LibraryLoaderImportLoadViewModel>(importLoad),
                DisplayText = importLoad.SourceFullPath
            };
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(LibraryLoaderOutput workOutput)
        {
            var importOutput = workOutput.Get<LibraryLoaderImportOutput>();

            return new LibraryLoaderOutputViewModel()
            {
                Output = _audioStationMapper.Map<LibraryLoaderImportOutput, LibraryLoaderImportOutputViewModel>(importOutput)
            };
        }

        protected override LibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            return new LibraryLoaderLoad(workItem.LoadType,
                   new LibraryLoaderFileLoad(this.Id, workItem.LoadType, (workItem.Load.Data as LibraryLoaderFileLoadViewModel).FullPath));
        }
    }
}

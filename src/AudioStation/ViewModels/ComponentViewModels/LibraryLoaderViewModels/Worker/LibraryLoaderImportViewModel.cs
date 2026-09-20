using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Payload.Input;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Payload.Output;
using AudioStation.ViewModels.MainViewModels;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    /// <summary>
    /// This is a UI data class for the library importer. It will operate on the staged files for the 
    /// import function - setting the Load / Output properties during load.
    /// </summary>
    public class LibraryLoaderImportViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileViewModel>
    {
        private readonly IAudioStationMapper _audioStationMapper;

        private readonly LibraryImporterConfigurationViewModel _libraryImporterConfiguration;

        public LibraryLoaderImportViewModel(LibraryImporterConfigurationViewModel libraryImporterConfiguration)
            : base("Library Import Worker", "Library import worker task is for importing library records during an import workflow")
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();

            _libraryImporterConfiguration = libraryImporterConfiguration;
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            return new LibraryLoaderLoad<LibraryLoaderImportPayload>(this.Id, LibraryLoadType.Import, loadItem.FullPath, new LibraryLoaderImportPayload()
            {
                TagSmallId = loadItem.TagRecord.Id,

                SourceFullPath = loadItem.FullPath,
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
            });
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var result = new List<ILibraryLoaderLoad>();
            var counter = 0;

            // Load / Output:  These are part of the workflow process. All of the import data is setup here
            //                 so that the view binding can happen without a big mess in the code. Also, the
            //                 back and forth with the backend for imports is kept clean by using these objects.
            //
            foreach (LibraryImporterFileViewModel stagedFile in loadItems)
            {
                progressHandler(1, 1, loadItems.Count(), counter++, "Staging: " + stagedFile.FullPath);

                result.Add(CreateWorkLoad(stagedFile, configuration, audioStationController, progressHandler));

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

        protected override LibraryLoaderLoadViewModel MapWorkLoad(ILibraryLoaderLoad workLoad)
        {
            var importLoad = workLoad.Payload as LibraryLoaderImportPayload;

            return new LibraryLoaderLoadViewModel()
            {
                Payload = _audioStationMapper.Map<LibraryLoaderImportPayload, LibraryLoaderImportInputViewModel>(importLoad),
            };
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(ILibraryLoaderOutput workOutput)
        {
            var importOutput = workOutput.Payload as LibraryLoaderImportOutputPayload;

            return new LibraryLoaderOutputViewModel()
            {
                Payload = _audioStationMapper.Map<LibraryLoaderImportOutputPayload, LibraryLoaderImportOutputViewModel>(importOutput)
            };
        }

        protected override ILibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            var inputPayloadViewModel = workItem.Load.Payload as LibraryLoaderImportInputViewModel;

            // These share a common interface
            var inputPayload = _audioStationMapper.Map<LibraryLoaderImportInputViewModel, LibraryLoaderImportPayload>(inputPayloadViewModel);

            return new LibraryLoaderLoad<LibraryLoaderImportPayload>(this.Id, workItem.LoadType, workItem.Load.DisplayName, inputPayload);
        }
    }
}

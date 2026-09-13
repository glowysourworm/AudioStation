using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;
using AudioStation.ViewModels.MainViewModels;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    /// <summary>
    /// This is a UI data class for the library importer. It will operate on the staged files for the 
    /// import function - setting the Load / Output properties during load.
    /// </summary>
    public class LibraryLoaderImportViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioStationMapper _audioStationMapper;

        private readonly IEnumerable<LibraryImporterFileViewModel> _stagedFiles;
        private readonly LibraryImporterConfigurationViewModel _libraryImporterConfiguration;

        public LibraryLoaderImportViewModel(
            LibraryImporterConfigurationViewModel libraryImporterConfiguration,
            IEnumerable<LibraryImporterFileViewModel> stagedFiles)
            : base("Library Import Worker", "Library import worker task is for importing library records during an import workflow", -1, false)
        {
            _stagedFiles = stagedFiles;
            _libraryImporterConfiguration = libraryImporterConfiguration;
        }

        public LibraryLoaderImportViewModel(int workflowId,
            LibraryImporterConfigurationViewModel libraryImporterConfiguration,
            IEnumerable<LibraryImporterFileViewModel> stagedFiles)
            : base("Library Import Worker", "Library import worker task is for importing library records during an import workflow", workflowId, true)
        {
            _stagedFiles = stagedFiles;
            _libraryImporterConfiguration = libraryImporterConfiguration;
        }

        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            base.Load(configuration, audioStationController, progressHandler);

            var counter = 0;

            // Load / Output:  These are part of the workflow process. All of the import data is setup here
            //                 so that the view binding can happen without a big mess in the code. Also, the
            //                 back and forth with the backend for imports is kept clean by using these objects.
            //
            foreach (var stagedFile in _stagedFiles)
            {
                progressHandler(_stagedFiles.Count(), counter++, 0, "Staging: " + stagedFile.FullPath);

                var importLoad = new LibraryLoaderImportLoadViewModel()
                {
                    ConvertAudioFormat = _libraryImporterConfiguration.ConvertAudioFormat,
                    DestinationFolder = _libraryImporterConfiguration.ImportDirectory.Directory,
                    DestinationFormat = _audioStationMapper.Map<AudioEncoderViewModel, AudioEncoderInfo>(_libraryImporterConfiguration.ImportDirectory.FormatPreference),
                    GroupingType = _libraryImporterConfiguration.ImportDirectory.GroupingType,
                    IsSourceDirectoryReadonly = _libraryImporterConfiguration.ImportDirectory.IsReadOnly,
                    MigrationDeleteSourceFiles = _libraryImporterConfiguration.MigrationDeleteSourceFiles,
                    MigrationDeleteSourceFolders = _libraryImporterConfiguration.MigrationDeleteSourceFolders,
                    MigrationOverwriteDestinationFiles = _libraryImporterConfiguration.MigrationOverwriteDestinationFiles,
                    MigrationSourceDirectory = _libraryImporterConfiguration.MigrationSourceDirectory,
                    NamingType = _libraryImporterConfiguration.ImportDirectory.NamingType,
                    SourceFullPath = stagedFile.FullPath,
                    TagSmallId = stagedFile.MusicBrainzTag.Id,
                    TrackCategory = _libraryImporterConfiguration.ImportDirectory.TrackCategory
                };

                // This will be set by the data services involved with the backend during import
                var importOutput = new LibraryLoaderImportOutputViewModel();

                this.WorkItems.Add(new LibraryWorkItemViewModel()
                {
                    Load = new LibraryLoaderLoadViewModel()
                    {
                        DisplayText = stagedFile.DisplayName,
                        Data = importLoad
                    },
                    LoadType = LibraryLoadType.Import,
                    Output = new LibraryLoaderOutputViewModel()
                    {
                        Output = importOutput
                    }
                });

                stagedFile.ImportLoad = importLoad;
                stagedFile.ImportOutput = importOutput;

                // PERFORMANCE ISSUE:  The tag data must be read; and minimal during file reading. The objects
                //                     involved must be small. So, we're going to try making "TagSmall" objects
                //                     in the cache to help out. And, we need to be sure that the tag library 
                //                     is optimized. So, CSCore may be a replacement for ATL.
                //

            }

            this.Loaded = true;
        }
    }
}

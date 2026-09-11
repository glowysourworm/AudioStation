using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.MainViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderImportViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioStationMapper _audioStationMapper;

        private readonly IEnumerable<LibraryImporterFileViewModel> _stagedFiles;
        private readonly LibraryImporterConfigurationViewModel _libraryImporterConfiguration;

        public LibraryLoaderImportViewModel(
            IIocEventAggregator eventAggregator,
            IAudioStationMapper audioStationMapper,
            ILibraryLoaderWorkerService libraryLoaderService,
            LibraryImporterConfigurationViewModel libraryImporterConfiguration,
            IEnumerable<LibraryImporterFileViewModel> stagedFiles)
            : base("Library Import Worker", "Library import worker task is for importing library records during an import workflow", eventAggregator, libraryLoaderService)
        {
            _audioStationMapper = audioStationMapper;

            _stagedFiles = stagedFiles;
            _libraryImporterConfiguration = libraryImporterConfiguration;
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.WorkItems.Clear();

            foreach (var stagedFile in _stagedFiles)
            {
                this.WorkItems.Add(new LibraryWorkItemViewModel()
                {
                    Load = new LibraryLoaderLoadViewModel()
                    {
                        DisplayText = stagedFile.DisplayName,
                        Data = new LibraryLoaderImportLoadViewModel()
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
                        }
                    },
                    LoadType = LibraryLoadType.Import
                });
            }
        }
    }
}

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.TagViewModels;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzBasicViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileViewModel>
    {
        private readonly IAudioStationMapper _audioStationMapper;

        Dictionary<string, LibraryImporterFileViewModel> _loadItemDict;

        public LibraryLoaderMusicBrainzBasicViewModel()
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID")
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _loadItemDict = new Dictionary<string, LibraryImporterFileViewModel>();
        }

        public LibraryLoaderMusicBrainzBasicViewModel(LibraryImporterConfigurationViewModel configuration)
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID")
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _loadItemDict = new Dictionary<string, LibraryImporterFileViewModel>();
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

            return new LibraryLoaderLoad<IEnumerable<IAcoustIDLookupResult>>(this.Id, LibraryLoadType.MusicBrainzBasic, loadItem.DisplayName, new IAcoustIDLookupResult[] { loadItem.SelectedAcoustIDResult });
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var audioConverter = IocContainer.Get<IAudioConverter>();
            var tagCache = audioStationController.ServiceController.GetCache<ITagCache>();

            try
            {
                // Check against existing AcoustID Results
                var existingResults = audioStationController.ServiceController
                                                            .GetDataService<IAudioStationDbClient>()
                                                            .GetEntities<AcoustIDLookupResult>()
                                                            .GroupBy(x => x.FileName)
                                                            .ToDictionary(x => x.Key, x => x.ToList());

                var result = new List<ILibraryLoaderLoad>();
                var counter = 0;

                foreach (var stagedFile in loadItems)
                {
                    counter++;

                    // Look for AcoustID Results by file name
                    if (!existingResults.ContainsKey(stagedFile.FullPath))
                        continue;

                    // Track items by file name
                    _loadItemDict.Add(stagedFile.FullPath, stagedFile);

                    // Report Progress
                    progressHandler(1, 1, loadItems.Count(), counter, "Loading: " + stagedFile.FullPath);

                    // Create Load
                    result.Add(new LibraryLoaderLoad<IEnumerable<IAcoustIDLookupResult>>(this.Id,
                                    LibraryLoadType.MusicBrainzBasic, stagedFile.DisplayName, existingResults[stagedFile.FullPath]));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(ILibraryLoaderLoad workLoad)
        {
            return new LibraryLoaderLoadViewModel()
            {
                Payload = workLoad.Payload,
                LoadType = workLoad.LoadType,
                OwnerId = workLoad.OwnerId,
                DisplayName = workLoad.DisplayName
            };
        }
        protected override LibraryLoaderOutputViewModel MapWorkOutput(ILibraryLoaderOutput workOutput)
        {
            return new LibraryLoaderOutputViewModel()
            {
                Payload = workOutput.Payload
            };
        }
        protected override ILibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            return new LibraryLoaderLoad<IEnumerable<IAcoustIDLookupResult>>(
                workItem.Load.OwnerId,
                workItem.Load.LoadType,
                workItem.Load.DisplayName,
                workItem.Load.Payload as IEnumerable<IAcoustIDLookupResult>);
        }
        protected override void CompleteWorkItem(LibraryWorkItemViewModel workItem)
        {
            var tagResults = workItem.Output.Payload as IEnumerable<TagSmall>;
            var acoustIDResults = workItem.Load.Payload as IEnumerable<IAcoustIDLookupResult>;

            if (tagResults == null)
                throw new Exception("Corrupt work item output");

            if (acoustIDResults == null || !acoustIDResults.Any())
                throw new Exception("Corrupt AcoustID load");

            if (!_loadItemDict.ContainsKey(acoustIDResults.First().FileName))
                throw new Exception("Missing work item payload");

            var loadItem = _loadItemDict[acoustIDResults.First().FileName];
            var tagViewModels = tagResults.Select(tag => _audioStationMapper.Map<TagSmall, TagSmallViewModel>(tag)).Actualize();

            // Pass results to the Import Output
            foreach (var result in tagViewModels)
            {
                if (loadItem.ImportOutput.MusicBrainzRecordingMatches.Any(x => x.Id == result.Id))
                    loadItem.ImportOutput.MusicBrainzRecordingMatches.Add(result);
            }

        }
    }
}

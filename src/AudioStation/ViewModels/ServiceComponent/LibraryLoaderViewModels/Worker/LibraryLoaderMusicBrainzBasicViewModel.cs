using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility.RecursiveComparer;
using AudioStation.Event;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application;

namespace AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzBasicViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileTreeNodeViewModel>
    {
        private readonly IAudioStationMapper _audioStationMapper;
        private IAudioStationDbClient _audioStationDbClient;

        Dictionary<string, LibraryImporterFileTreeNodeViewModel> _loadItemDict;

        private readonly bool _serviceMusicBrainzBasicIncludeTagLookup;

        public LibraryLoaderMusicBrainzBasicViewModel(bool executeAsBulk)
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID", executeAsBulk)
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _loadItemDict = new Dictionary<string, LibraryImporterFileTreeNodeViewModel>();
            _serviceMusicBrainzBasicIncludeTagLookup = false;
        }

        public LibraryLoaderMusicBrainzBasicViewModel(LibraryImporterConfigurationViewModel configuration, bool executeAsBulk)
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID", executeAsBulk)
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _loadItemDict = new Dictionary<string, LibraryImporterFileTreeNodeViewModel>();
            _serviceMusicBrainzBasicIncludeTagLookup = configuration.ServiceMusicBrainzBasicIncludeTagLookup;
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileTreeNodeViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

            return CreateWorkLoads(new LibraryImporterFileTreeNodeViewModel[] { loadItem }, configuration, audioStationController, progressHandler).FirstOrDefault();

            //return new LibraryLoaderLoad<IEnumerable<IAcoustIDLookupResult>>(this.Id, LibraryLoadType.MusicBrainzBasic, loadItem.DisplayName, new IAcoustIDLookupResult[] { loadItem.SelectedAcoustIDResult });
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileTreeNodeViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var audioConverter = IocContainer.Get<IAudioConverter>();
            var tagCache = audioStationController.ServiceController.GetCache<ITagCache>();

            _audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

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
                    result.Add(new LibraryLoaderLoad<LibraryLoaderMusicBrainzBasicPayload>(
                        this.Id, LibraryLoadType.MusicBrainzBasic, stagedFile.ImportFile.ImportLoad.SourceFullPath,
                        new LibraryLoaderMusicBrainzBasicPayload()
                        {
                            AcoustIDResults = existingResults[stagedFile.FullPath],
                            FileName = stagedFile.FullPath,
                            MusicBrainzReleaseTrackIDTag = stagedFile.ImportFile.MusicBrainzReleaseTrackIDTag,
                            PerformExtraTagLookup = _serviceMusicBrainzBasicIncludeTagLookup
                        }));
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
            return new LibraryLoaderLoad<LibraryLoaderMusicBrainzBasicPayload>(
                workItem.Load.OwnerId,
                workItem.Load.LoadType,
                workItem.Load.DisplayName,
                workItem.Load.Payload as LibraryLoaderMusicBrainzBasicPayload);
        }
        protected override void CompleteWorkItem(LibraryWorkItemViewModel workItem)
        {
            var output = workItem.Output.Payload as LibraryLoaderMusicBrainzBasicOutputPayload;
            var input = workItem.Load.Payload as LibraryLoaderMusicBrainzBasicPayload;

            if (output == null)
                throw new Exception("Corrupt work item output");

            if (input == null)
                throw new Exception("Corrupt AcoustID load");

            if (!_loadItemDict.ContainsKey(input.FileName))
                throw new Exception("Missing work item payload");

            var loadItem = _loadItemDict[input.FileName];
            var acoustIDResults = output.AcoustIDResults.Select(tag => _audioStationMapper.Map<TagSmall, TagSmallViewModel>(tag)).Actualize();
            var comparer = new SimpleRecursiveComparer();

            // Music Brainz (special tag result)
            if (output.MusicBrainzResult != null)
            {
                _audioStationMapper.MapOnto(output.MusicBrainzResult, loadItem.ImportFile.TagMusicBrainz);

                // Music Brainz (special tag result) Success!
                loadItem.ImportFile.MusicBrainzReleaseTrackQuerySuccess = true;
            }


            // Pass results to the Import Output
            foreach (var result in acoustIDResults)
            {
                // Compare (by value)
                if (!loadItem.ImportFile.ImportOutput.MusicBrainzRecordingMatches.Any(x => comparer.Compare(x, result)))
                    loadItem.ImportFile.ImportOutput.MusicBrainzRecordingMatches.Add(result);
            }

            // TODO: Re-situate workflow code
            var combinedResults = _audioStationDbClient.GetViewEntities<MusicBrainzAcoustIDResult>().ToList();

            loadItem.ImportFile.ImportOutput.MusicBrainzAcoustIDResults.Clear();
            loadItem.ImportFile.ImportOutput.MusicBrainzAcoustIDResults.AddRange(combinedResults.Where(x => x.FileName == loadItem.FullPath));

        }
    }
}

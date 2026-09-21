using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Payload.Input;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderAcoustIDViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileViewModel>
    {
        private readonly IAudioStationMapper _audioStationMapper;

        private readonly LibraryImporterConfigurationViewModel? _workflowConfiguration;

        // Keep track of files that have been added (directory iteration was missing some)
        Dictionary<string, ILibraryLoaderLoad> _workLoadDict;
        Dictionary<string, LibraryImporterFileViewModel> _loadItemDict;

        public LibraryLoaderAcoustIDViewModel()
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service")
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _workLoadDict = new Dictionary<string, ILibraryLoaderLoad>();
            _loadItemDict = new Dictionary<string, LibraryImporterFileViewModel>();
            _workflowConfiguration = null;
        }
        public LibraryLoaderAcoustIDViewModel(LibraryImporterConfigurationViewModel configuration)
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service")
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _workflowConfiguration = configuration;
            _workLoadDict = new Dictionary<string, ILibraryLoaderLoad>();
            _loadItemDict = new Dictionary<string, LibraryImporterFileViewModel>();
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var result = CreateWorkLoads(new LibraryImporterFileViewModel[] { loadItem }, configuration, audioStationController, progressHandler);

            return result.FirstOrDefault();
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var audioConverter = IocContainer.Get<IAudioConverter>();
            var tagCache = audioStationController.ServiceController.GetCache<ITagCache>();

            try
            {
                // Procedure
                //
                // 1) Loop through all library directories (+ staging and download)
                // 2) Build directory tree (using DirectoryTreeLoader)
                // 3) Build work items

                progressHandler(1, 1, 0, 0, "Loading AcoustID Data...");

                // Check against existing AcoustID Results
                var existingResults = audioStationController.ServiceController
                                                            .GetDataService<IAudioStationDbClient>()
                                                            .GetEntities<AcoustIDLookupResult>()
                                                            .GroupBy(x => x.FileName)
                                                            .ToDictionary(x => x.Key, x => x.ToList());

                var result = new List<ILibraryLoaderLoad>();
                var totalCount = loadItems.Count();
                var counter = 0;

                foreach (var stagedFile in loadItems)
                {
                    progressHandler(1, 1, totalCount, counter++, "Loading: " + stagedFile.DisplayName);

                    // Keep track of load items for post processing
                    _loadItemDict.Add(stagedFile.FullPath, stagedFile);

                    //// Already Added
                    //if (_workLoadDict.ContainsKey(stagedFile.FullPath) ||
                    //     existingResults.ContainsKey(stagedFile.FullPath))
                    //    continue;

                    //// Use Existing AcoustID
                    //else if (_workflowConfiguration?.AcoustIDSourcePreference == LibraryImportSource.File)
                    //{
                    //    if (stagedFile.AcoustIDTag != null)
                    //        continue;
                    //}



                    result.Add(new LibraryLoaderLoad<LibraryLoaderFilePayload>(this.Id, LibraryLoadType.AcoustID, stagedFile.FullPath,
                               new LibraryLoaderFilePayload(stagedFile.FullPath)));
                }

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(ILibraryLoaderLoad workLoad)
        {
            var fileLoad = workLoad.Payload as LibraryLoaderFilePayload;

            return new LibraryLoaderLoadViewModel()
            {
                Payload = new LibraryLoaderFileLoadViewModel(fileLoad.File, fileLoad.File),
                DisplayName = workLoad.DisplayName
            };
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(ILibraryLoaderOutput workOutput)
        {
            var entityOutput = workOutput.Payload as List<AcoustIDLookupResult>;

            var payload = new ObservableCollection<AcoustIDLookupResultViewModel>();

            foreach (var entity in entityOutput)
            {
                payload.Add(_audioStationMapper.Map<AcoustIDLookupResult, AcoustIDLookupResultViewModel>(entity));
            }

            var output = new LibraryLoaderOutputViewModel()
            {
                Payload = payload
            };

            return output;
        }

        protected override ILibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            return new LibraryLoaderLoad<LibraryLoaderFilePayload>(this.Id, workItem.LoadType, workItem.Load.DisplayName,
                   new LibraryLoaderFilePayload((workItem.Load.Payload as LibraryLoaderFileLoadViewModel).FullPath));
        }
        protected override void CompleteWorkItem(LibraryWorkItemViewModel workItem)
        {
            var filePayload = workItem.Load.Payload as LibraryLoaderFileLoadViewModel;
            var acoustIdResults = workItem.Output.Payload as IEnumerable<AcoustIDLookupResultViewModel>;

            if (filePayload == null)
                throw new Exception("Corrupt work item payload");

            if (acoustIdResults == null)
                throw new Exception("Corrupt work item output");

            if (!_loadItemDict.ContainsKey(filePayload.FullPath))
                throw new Exception("Missing work item payload");

            var loadItem = _loadItemDict[filePayload.FullPath];

            // Pass results to the Import Output
            foreach (var result in acoustIdResults)
            {
                if (!loadItem.ImportOutput.AcoustIDResults.Any(x => x.LookupId == result.LookupId))
                    loadItem.ImportOutput.AcoustIDResults.Add(result);
            }

        }

        //private IEnumerable<LibraryLoaderLoad> LoadDirectory(string directory,
        //                                                     string filter,
        //                                                     ITagCache tagCache,
        //                                                     Dictionary<string, List<AcoustIDLookupResult>> existingResults,
        //                                                     DialogEventHandlers.DialogProgressHandler progressHandler)
        //{
        //    var result = new List<LibraryLoaderLoad>();

        //    progressHandler(1, 1, 0, 0, "Loading Directory: " + directory);

        //    // Load Directory
        //    var directoryTree = DirectoryTreeLoader.Load(directory, -1, progressHandler, filter);

        //    // File Count
        //    var totalCount = directoryTree.RecursiveCount(x => !x.CanHaveChildren);
        //    var counter = 0;

        //    directoryTree.RecurseForEach(entry =>
        //    {
        //        var tree = entry as FileTreeViewModel;

        //        // Already Added
        //        if (_workLoadDict.ContainsKey(tree.GetNodeValue().FullPath))
        //            return;

        //        if (!tree.GetNodeValue().IsDirectory)
        //        {
        //            // Report Progress
        //            progressHandler(1, 1, totalCount, counter++, "Loading: " + entry.NodeValue.DisplayName);

        //            var alreadyRun = existingResults.ContainsKey(tree.GetNodeValue().FullPath);

        //            // Existing Results (Output)
        //            if (alreadyRun)
        //                return;

        //            // Use Existing AcoustID
        //            else if (_workflowConfiguration?.AcoustIDSourcePreference == LibraryImportSource.File)
        //            {
        //                var tagData = tagCache.Get(tree.GetNodeValue().FullPath);

        //                // AcoustID Stored in Tag
        //                var acoustID = tagData.GetAcoustIDIdentifier();
        //                var musicBrainzTrackId = tagData.GetMusicBrainzTrackId();
        //                var musicBrainzReleaseTrackId = tagData.GetMusicBrainzReleaseTrackId();

        //                if (acoustID != null ||
        //                    musicBrainzReleaseTrackId != null ||
        //                    musicBrainzTrackId != null)
        //                    return;
        //            }

        //            var workLoad = new LibraryLoaderFileLoad(this.Id, LibraryLoadType.AcoustID, tree.GetNodeValue().FullPath);

        //            // Add (by file full path)
        //            _workLoadDict.Add(tree.GetNodeValue().FullPath, workLoad);

        //            result.Add(new LibraryLoaderLoad(LibraryLoadType.AcoustID, workLoad));
        //        }
        //    });

        //    return result;
        //}
    }
}

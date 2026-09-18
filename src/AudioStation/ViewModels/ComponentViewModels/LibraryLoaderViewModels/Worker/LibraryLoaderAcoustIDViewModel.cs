using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.ViewModel.FileTreeView;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderAcoustIDViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly LibraryImporterConfigurationViewModel? _workflowConfiguration;
        private readonly KeyedObservableCollection<string, LibraryImporterFileViewModel>? _stagedFiles;

        // Keep track of files that have been added (directory iteration was missing some)
        Dictionary<string, ILibraryLoaderLoad> _workLoadDict;

        public LibraryLoaderAcoustIDViewModel()
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service")
        {
            _workLoadDict = new Dictionary<string, ILibraryLoaderLoad>();
            _workflowConfiguration = null;
            _stagedFiles = null;
        }
        public LibraryLoaderAcoustIDViewModel(LibraryImporterConfigurationViewModel configuration,
                                              KeyedObservableCollection<string, LibraryImporterFileViewModel>? stagedFiles = null)
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service")
        {
            _workflowConfiguration = configuration;
            _workLoadDict = new Dictionary<string, ILibraryLoaderLoad>();
            _stagedFiles = stagedFiles;
        }

        protected override IEnumerable<LibraryLoaderLoad> CreateWorkLoads(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
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

                progressHandler(0, 0, 0, 0, "Loading AcoustID Data...");

                // Check against existing AcoustID Results
                var existingResults = audioStationController.ServiceController
                                                            .GetDataService<IAudioStationDbClient>()
                                                            .GetEntities<AcoustIDLookupResult>()
                                                            .GroupBy(x => x.FileName)
                                                            .ToDictionary(x => x.Key, x => x.ToList());

                var result = new List<LibraryLoaderLoad>();

                foreach (var extension in audioConverter.GetSupportedFormatExtensions())
                {
                    var format = audioConverter.GetDefaultFormat(extension);

                    // Staging (default)
                    if (_workflowConfiguration == null)
                    {
                        result.AddRange(LoadDirectory(configuration.StagingFolder.Directory, format.Filter, tagCache, existingResults, progressHandler));
                    }
                    else
                    {
                        // Staged Files (this worker is being told what to do)
                        if (_stagedFiles != null)
                        {
                            result.AddRange(LoadFromStaged(format.Filter, tagCache, existingResults, progressHandler));
                        }

                        // Non-Staged Files (this worker will figure out what to do)
                        else
                        {
                            // Migration
                            if (_workflowConfiguration.ImportType == LibraryImportType.Migration)
                            {
                                result.AddRange(LoadDirectory(_workflowConfiguration.MigrationSourceDirectory, format.Filter, tagCache, existingResults, progressHandler));
                            }

                            // In Place
                            else
                            {
                                result.AddRange(LoadDirectory(_workflowConfiguration.ImportDirectory.Directory, format.Filter, tagCache, existingResults, progressHandler));
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(LibraryLoaderLoad workLoad)
        {
            var fileLoad = workLoad.Get<LibraryLoaderFileLoad>();

            return new LibraryLoaderLoadViewModel()
            {
                Data = new LibraryLoaderFileLoadViewModel(fileLoad.File, fileLoad.File),
                DisplayText = fileLoad.File
            };
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(LibraryLoaderOutput workOutput)
        {
            var entityOutput = workOutput.Get<LibraryLoaderEntitySetOutput<AcoustIDLookupResult>>();

            return new LibraryLoaderOutputViewModel()
            {
                Output = new LibraryLoaderEntitySetOutputViewModel<AcoustIDLookupResult>()
            };
        }

        protected override LibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            return new LibraryLoaderLoad(workItem.LoadType,
                   new LibraryLoaderFileLoad(this.Id, workItem.LoadType, (workItem.Load.Data as LibraryLoaderFileLoadViewModel).FullPath));
        }

        private IEnumerable<LibraryLoaderLoad> LoadFromStaged(string filter,
                                                             ITagCache tagCache,
                                                             Dictionary<string, List<AcoustIDLookupResult>> existingResults,
                                                             DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (_stagedFiles == null)
                throw new ArgumentException("Must assign staged files for this loading option (please see constructor)");

            var result = new List<LibraryLoaderLoad>();

            // File Count
            var counter = 0;

            foreach (LibraryImporterFileViewModel stagedFile in _stagedFiles)
            {
                // Report Progress
                progressHandler(1, 1, _stagedFiles.Count, counter++, "Loading: " + stagedFile.DisplayName);

                // Already Added
                if (_workLoadDict.ContainsKey(stagedFile.FullPath) ||
                     existingResults.ContainsKey(stagedFile.FullPath))
                    continue;

                // Use Existing AcoustID
                else if (_workflowConfiguration?.AcoustIDSourcePreference == LibraryImportSource.File)
                {
                    var tagData = tagCache.Get(stagedFile.FullPath);

                    // AcoustID Stored in Tag
                    var acoustID = tagData.GetAcoustIDIdentifier();
                    var musicBrainzTrackId = tagData.GetMusicBrainzTrackId();
                    var musicBrainzReleaseTrackId = tagData.GetMusicBrainzReleaseTrackId();

                    if (acoustID != null ||
                        musicBrainzReleaseTrackId != null ||
                        musicBrainzTrackId != null)
                        continue;
                }

                var workLoad = new LibraryLoaderFileLoad(this.Id, LibraryLoadType.AcoustID, stagedFile.FullPath);

                // Add (by file full path)
                _workLoadDict.Add(stagedFile.FullPath, workLoad);

                result.Add(new LibraryLoaderLoad(LibraryLoadType.AcoustID, workLoad));
            }

            return result;
        }


        private IEnumerable<LibraryLoaderLoad> LoadDirectory(string directory,
                                                             string filter,
                                                             ITagCache tagCache,
                                                             Dictionary<string, List<AcoustIDLookupResult>> existingResults,
                                                             DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var result = new List<LibraryLoaderLoad>();

            progressHandler(1, 1, 0, 0, "Loading Directory: " + directory);

            // Load Directory
            var directoryTree = DirectoryTreeLoader.Load(directory, -1, progressHandler, filter);

            // File Count
            var totalCount = directoryTree.RecursiveCount(x => !x.CanHaveChildren);
            var counter = 0;

            directoryTree.RecurseForEach(entry =>
            {
                var tree = entry as FileTreeViewModel;

                // Already Added
                if (_workLoadDict.ContainsKey(tree.GetNodeValue().FullPath))
                    return;

                if (!tree.GetNodeValue().IsDirectory)
                {
                    // Report Progress
                    progressHandler(1, 1, totalCount, counter++, "Loading: " + entry.NodeValue.DisplayName);

                    var alreadyRun = existingResults.ContainsKey(tree.GetNodeValue().FullPath);

                    // Existing Results (Output)
                    if (alreadyRun)
                        return;

                    // Use Existing AcoustID
                    else if (_workflowConfiguration?.AcoustIDSourcePreference == LibraryImportSource.File)
                    {
                        var tagData = tagCache.Get(tree.GetNodeValue().FullPath);

                        // AcoustID Stored in Tag
                        var acoustID = tagData.GetAcoustIDIdentifier();
                        var musicBrainzTrackId = tagData.GetMusicBrainzTrackId();
                        var musicBrainzReleaseTrackId = tagData.GetMusicBrainzReleaseTrackId();

                        if (acoustID != null ||
                            musicBrainzReleaseTrackId != null ||
                            musicBrainzTrackId != null)
                            return;
                    }

                    var workLoad = new LibraryLoaderFileLoad(this.Id, LibraryLoadType.AcoustID, tree.GetNodeValue().FullPath);

                    // Add (by file full path)
                    _workLoadDict.Add(tree.GetNodeValue().FullPath, workLoad);

                    result.Add(new LibraryLoaderLoad(LibraryLoadType.AcoustID, workLoad));
                }
            });

            return result;
        }
    }
}

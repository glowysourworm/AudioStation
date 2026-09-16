using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
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

        // Keep track of files that have been added (directory iteration was missing some)
        Dictionary<string, LibraryWorkItemViewModel> _workItemDict;

        public LibraryLoaderAcoustIDViewModel()
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service")
        {
            _workItemDict = new Dictionary<string, LibraryWorkItemViewModel>();
            _workflowConfiguration = null;
        }
        public LibraryLoaderAcoustIDViewModel(LibraryImporterConfigurationViewModel configuration)
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service")
        {
            _workflowConfiguration = configuration;
            _workItemDict = new Dictionary<string, LibraryWorkItemViewModel>();
        }

        protected override void LoadWorkItems(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
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

                foreach (var extension in audioConverter.GetSupportedFormatExtensions())
                {
                    var format = audioConverter.GetDefaultFormat(extension);

                    // Staging (default)
                    if (_workflowConfiguration == null)
                    {
                        LoadDirectory(configuration.StagingFolder.Directory, format.Filter, tagCache, existingResults, progressHandler);
                    }
                    else
                    {
                        // Migration
                        if (_workflowConfiguration.ImportType == LibraryImportType.Migration)
                        {
                            LoadDirectory(_workflowConfiguration.MigrationSourceDirectory, format.Filter, tagCache, existingResults, progressHandler);
                        }

                        // In Place
                        else
                        {
                            LoadDirectory(_workflowConfiguration.ImportDirectory.Directory, format.Filter, tagCache, existingResults, progressHandler);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private void LoadDirectory(string directory, string filter,
                                   ITagCache tagCache,
                                   Dictionary<string, List<AcoustIDLookupResult>> existingResults,
                                   DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            progressHandler(1, 1, 0, 0, "Loading Directory: " + directory);

            // Load Directory
            var directoryTree = DirectoryTreeLoader.Load(directory, filter, -1);

            // File Count
            var totalCount = directoryTree.RecursiveCount(x => !x.CanHaveChildren);
            var counter = 0;

            directoryTree.RecurseForEach(entry =>
            {
                var tree = entry as FileTreeViewModel;

                // Already Added
                if (_workItemDict.ContainsKey(tree.GetNodeValue().FullPath))
                    return;

                if (!tree.GetNodeValue().IsDirectory)
                {
                    // Report Progress
                    progressHandler(1, 1, totalCount, counter++, "Loading: " + entry.NodeValue.DisplayName);

                    var alreadyRun = existingResults.ContainsKey(tree.GetNodeValue().FullPath);
                    var output = new LibraryLoaderEntitySetOutputViewModel<AcoustIDLookupResult>();
                    var tagData = tagCache.Get(tree.GetNodeValue().FullPath);

                    // AcoustID Stored in Tag
                    var acoustID = tagData.GetAcoustIDIdentifier();
                    var musicBrainzTrackId = tagData.GetMusicBrainzTrackId();
                    var musicBrainzReleaseTrackId = tagData.GetMusicBrainzReleaseTrackId();

                    // Existing Results (Output)
                    if (alreadyRun)
                        output.ResultSet.AddRange(existingResults[tree.GetNodeValue().FullPath]);

                    // Use Existing AcoustID
                    else if (_workflowConfiguration?.AcoustIDSourcePreference == LibraryImportSource.File)
                    {
                        if (acoustID != null ||
                            musicBrainzReleaseTrackId != null ||
                            musicBrainzTrackId != null)
                            return;
                    }

                    var workItem = new LibraryWorkItemViewModel()
                    {
                        // This will indicate the failed result
                        HasErrors = alreadyRun && output.ResultSet.Count == 0,
                        IsCompleted = alreadyRun,
                        LoadType = LibraryLoadType.AcoustID,
                        Load = new LibraryLoaderLoadViewModel()
                        {
                            DisplayText = tree.GetNodeValue().FullPath,
                            Data = new LibraryLoaderFileLoadViewModel(tree.GetNodeValue().FullPath, tree.GetNodeValue().ShortPath)
                        },
                        Output = new LibraryLoaderOutputViewModel()
                        {
                            Output = output
                        },
                        InProgress = false,
                        Progress = alreadyRun ? 1 : 0
                    };

                    // Add
                    this.WorkItems.Add(workItem);

                    // Add (by file full path)
                    _workItemDict.Add(tree.GetNodeValue().FullPath, workItem);
                }
            });
        }
    }
}

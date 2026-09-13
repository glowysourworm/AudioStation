using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Utility;
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
        public LibraryLoaderAcoustIDViewModel()
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service", -1, false)
        {
        }
        public LibraryLoaderAcoustIDViewModel(int workflowId)
            : base("AcoustID", "Identifies recordings using AcoustID acoustic fingerprint service", workflowId, true)
        {
        }

        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            base.Load(configuration, audioStationController, progressHandler);

            var audioConverter = IocContainer.Get<IAudioConverter>();

            try
            {
                // Procedure
                //
                // 1) Loop through all library directories (+ staging and download)
                // 2) Build directory tree (using DirectoryTreeLoader)
                // 3) Build work items

                // Check against existing AcoustID Results
                var existingResults = audioStationController.ServiceController
                                                            .GetDataService<IAudioStationDbClient>()
                                                            .GetEntities<AcoustIDLookupResult>()
                                                            .GroupBy(x => x.FileName)
                                                            .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var format in audioConverter.GetSupportedFormats())
                {
                    foreach (var libraryDirectory in configuration.LibraryDirectories.Union(new LibraryDirectory[]
                    {
                        configuration.StagingFolder,
                        configuration.DownloadFolder
                    }))
                    {
                        // Load Directory
                        var directoryTree = DirectoryTreeLoader.Load(libraryDirectory.Directory, format.Filter, -1);

                        // File Count
                        var totalCount = directoryTree.RecursiveCount(x => true);
                        var counter = 0;

                        directoryTree.RecurseForEach(entry =>
                        {
                            var tree = entry as FileTreeViewModel;
                            var alreadyRun = existingResults.ContainsKey(tree.GetNodeValue().FullPath);
                            var output = new LibraryLoaderEntitySetOutputViewModel<AcoustIDLookupResult>();

                            // Existing Results (Output)
                            if (alreadyRun)
                                output.ResultSet.AddRange(existingResults[tree.GetNodeValue().FullPath]);

                            // Report Progress
                            progressHandler(totalCount, counter++, 0, "Loading: " + entry.NodeValue.DisplayName);

                            if (!tree.GetNodeValue().IsDirectory)
                            {
                                this.WorkItems.Add(new LibraryWorkItemViewModel()
                                {
                                    HasErrors = false,
                                    IsCompleted = alreadyRun,
                                    IsWorkflowItem = this.IsWorkflowTask,
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
                                    Progress = alreadyRun ? 1 : 0,
                                    WorkflowId = this.WorkflowId
                                });
                            }
                        });
                    }
                }

                this.Loaded = true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}

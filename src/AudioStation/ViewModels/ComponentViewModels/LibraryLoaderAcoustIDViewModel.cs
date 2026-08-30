using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryLoaderAcoustIDViewModel : LibraryLoaderComponentViewModelBase
    {
        [IocImportingConstructor]
        public LibraryLoaderAcoustIDViewModel(IIocEventAggregator eventAggregator, ILibraryLoaderService libraryLoaderService)
            : base(eventAggregator, libraryLoaderService)
        {
        }


        protected override void InitializeComponent(IAudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                // Procedure
                //
                // 1) Loop through all library directories (+ staging and download)
                // 2) Build directory tree (using DirectoryTreeLoader)
                // 3) Build work items

                // Initialize Work Items
                this.WorkItems.Clear();

                foreach (var libraryDirectory in configuration.LibraryDirectories.Union(new LibraryDirectory[]
                {
                    configuration.StagingFolder,
                    configuration.DownloadFolder
                }))
                {
                    // Load Directory
                    var directoryTree = DirectoryTreeLoader.Load(libraryDirectory.Directory, "*.mp3");

                    directoryTree.RecurseForEach(entry =>
                    {
                        if (!entry.NodeValue.IsDirectory)
                            this.WorkItems.Add(new LibraryWorkItemViewModel()
                            {
                                HasErrors = false,
                                IsCompleted = false,
                                LoadType = LibraryLoadType.AcoustID,
                                Load = new LibraryLoaderLoadViewModel()
                                {
                                    DisplayText = entry.NodeValue.FullPath,
                                    Data = new LibraryLoaderFileLoadViewModel(entry.NodeValue.FullPath, entry.NodeValue.ShortPath)
                                },
                                Output = new LibraryLoaderOutputViewModel()
                                {
                                    Output = new LibraryLoaderEntitySetOutputViewModel<AcoustIDLookupResult>()
                                },
                                InProgress = false,
                                Progress = 0
                            });
                    });
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}

using System.IO;

using AudioStation.Core;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Utility;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryLoaderAcoustIDViewModel : LibraryLoaderComponentViewModelBase
    {
        string _musicFolder;
        string _downloadFolder;

        public string MusicFolder
        {
            get { return _musicFolder; }
            set { this.RaiseAndSetIfChanged(ref _musicFolder, value); }
        }
        public string DownloadFolder
        {
            get { return _downloadFolder; }
            set { this.RaiseAndSetIfChanged(ref _downloadFolder, value); }
        }

        [IocImportingConstructor]
        public LibraryLoaderAcoustIDViewModel(IIocEventAggregator eventAggregator, ILibraryLoaderService libraryLoaderService)
            : base(eventAggregator, libraryLoaderService)
        {
        }


        protected override void InitializeComponent(Configuration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.MusicFolder = Path.Combine(configuration.DirectoryBase, configuration.MusicSubDirectory);
            this.DownloadFolder = configuration.DownloadFolder;

            try
            {
                // Load Directory
                var directoryTree = DirectoryTreeLoader.Load(this.MusicFolder, "*.mp3");

                // Initialize Work Items
                this.WorkItems.Clear();

                directoryTree.RecurseForEach(entry =>
                {
                    if (!entry.NodeValue.IsDirectory)
                        this.WorkItems.Add(new LibraryWorkItemViewModel()
                        {
                            HasErrors = false,
                            IsCompleted = false,
                            LoadType = LibraryLoadType.AcoustID,
                            Load = new LibraryLoaderFileLoadViewModel(entry.NodeValue.FullPath, entry.NodeValue.ShortPath),
                            Output = new LibraryLoaderEntitySetOutputViewModel<AcoustIDLookupResult>(),
                            InProgress = false,
                            Progress = 0
                        });
                });
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}

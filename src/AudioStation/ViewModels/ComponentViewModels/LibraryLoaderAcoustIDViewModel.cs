using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Core;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Utility;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryLoaderAcoustIDViewModel : ComponentViewModelBase<NoViewModel>
    {
        private readonly ILibraryLoaderService _libraryLoaderService;

        string _musicFolder;
        string _downloadFolder;
        ObservableCollection<LibraryWorkItemViewModel> _workItems;

        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;

        SimpleCommand _executeCommand;

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
        public ObservableCollection<LibraryWorkItemViewModel> WorkItems
        {
            get { return _workItems; }
            set { this.RaiseAndSetIfChanged(ref _workItems, value); }
        }
        public int WorkItemsInProgress
        {
            get { return _workItemsInProgress; }
            set { this.RaiseAndSetIfChanged(ref _workItemsInProgress, value); }
        }
        public int WorkItemsWaiting
        {
            get { return _workItemsWaiting; }
            set { this.RaiseAndSetIfChanged(ref _workItemsWaiting, value); }
        }
        public int WorkItemsSuccessful
        {
            get { return _workItemsSuccessful; }
            set { this.RaiseAndSetIfChanged(ref _workItemsSuccessful, value); }
        }
        public int WorkItemsError
        {
            get { return _workItemsError; }
            set { this.RaiseAndSetIfChanged(ref _workItemsError, value); }
        }

        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryLoaderAcoustIDViewModel(IIocEventAggregator eventAggregator, ILibraryLoaderService libraryLoaderService)
        {
            _libraryLoaderService = libraryLoaderService;

            this.MusicFolder = string.Empty;
            this.DownloadFolder = string.Empty;
            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();

            this.ExecuteCommand = new SimpleCommand(() =>
            {
                Execute();

            }, CanExecute);

            eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(OnWorkItemComplete);
            eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(OnWorkItemUpdate);
        }

        private void Execute()
        {
            foreach (var workItem in this.WorkItems)
            {
                switch (workItem.LoadType)
                {
                    case LibraryLoadType.AcoustID:
                        workItem.InProgress = true;
                        workItem.Progress = 0;

                        _libraryLoaderService.RunLoaderTaskAsync(workItem.Load as LibraryLoaderFileLoadViewModel);
                        break;
                    case LibraryLoadType.Import:
                    case LibraryLoadType.ImportRadio:
                    case LibraryLoadType.MusicBrainz:
                    default:
                        throw new Exception("Unhandled work item load type");
                }
            }
        }

        private bool CanExecute()
        {
            return !this.Loading &&
                   this.WorkItems.Any() &&
                   this.WorkItems.All(x => x.IsCompleted);
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            OnUpdate();
        }

        public override NoViewModel? Load { get; }

        public override void Dispose()
        {

        }

        private void OnWorkItemUpdate(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                ApplicationHelpers.MapOnto(model, workItem);
            }

            OnUpdate();
        }

        private void OnWorkItemComplete(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                ApplicationHelpers.MapOnto(model, workItem);
            }

            OnUpdate();
        }

        private void OnUpdate()
        {
            this.ExecuteCommand.RaiseCanExecuteChanged();

            this.Loading = this.WorkItems.Any(x => !x.IsCompleted);
        }

        public override void Initialize(Configuration configuration, NoViewModel load, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.MusicFolder = Path.Combine(configuration.DirectoryBase, configuration.MusicSubDirectory);
            this.DownloadFolder = configuration.DownloadFolder;

            try
            {
                // Load Directory
                var directoryTree = DirectoryTreeLoader.Load(this.MusicFolder, "*.mp3");

                // Initialize Work Items
                this.WorkItems.Clear();

                var counter = 0;

                directoryTree.RecurseForEach(entry =>
                {
                    if (!entry.NodeValue.IsDirectory)
                        this.WorkItems.Add(new LibraryWorkItemViewModel()
                        {
                            Id = counter++,
                            HasErrors = false,
                            IsCompleted = false,
                            LoadType = LibraryLoadType.AcoustID,
                            Load = new LibraryLoaderFileLoadViewModel(entry.NodeValue.FullPath),
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

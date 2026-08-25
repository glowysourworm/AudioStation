using System.Collections.ObjectModel;
using System.Windows.Threading;

using AudioStation.Core;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryLoaderMusicBrainzViewModel : ComponentViewModelBase<NoViewModel>
    {
        private readonly ILibraryLoaderService _libraryLoaderService;
        private readonly IAudioStationDbClient _audioStationDbClient;

        ObservableCollection<LibraryWorkItemViewModel> _workItems;
        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;

        SimpleCommand _executeCommand;

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
        public ObservableCollection<LibraryWorkItemViewModel> WorkItems
        {
            get { return _workItems; }
            set { this.RaiseAndSetIfChanged(ref _workItems, value); }
        }
        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }

        public override NoViewModel? Load { get; }

        [IocImportingConstructor]
        public LibraryLoaderMusicBrainzViewModel(
                IIocEventAggregator eventAggregator,
                ILibraryLoaderService libraryLoaderService,
                IAudioStationDbClient audioStationDbClient)
        {
            _libraryLoaderService = libraryLoaderService;
            _audioStationDbClient = audioStationDbClient;

            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();

            this.ExecuteCommand = new SimpleCommand(Execute, CanExecute);

            eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(OnWorkItemComplete);
            eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(OnWorkItemUpdate);
        }

        private bool CanExecute()
        {
            return !this.Loading;
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            OnUpdate();
        }

        private void Execute()
        {
            foreach (var workItem in this.WorkItems)
            {
                switch (workItem.LoadType)
                {
                    case LibraryLoadType.MusicBrainz:
                        workItem.Progress = 0;

                        // WORK ITEM:  Id is set from the backend!
                        workItem.Id = _libraryLoaderService.RunLoaderTaskAsync(workItem);
                        break;

                    case LibraryLoadType.AcoustID:
                    case LibraryLoadType.Import:
                    case LibraryLoadType.ImportRadio:
                    default:
                        throw new Exception("Unhandled work item load type");
                }
            }

            OnUpdate();
        }

        private void OnUpdate()
        {
            if (this.ExecuteCommand != null)
            {
                this.ExecuteCommand.RaiseCanExecuteChanged();

                this.Loading = this.WorkItems.Any(x => x.InProgress);
                this.WorkItemsWaiting = this.WorkItems.Count(x => !x.InProgress && !x.IsCompleted);
                this.WorkItemsInProgress = this.WorkItems.Count(x => x.InProgress);
                this.WorkItemsSuccessful = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && !x.HasErrors);
                this.WorkItemsError = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && x.HasErrors);
            }
        }

        private void OnWorkItemUpdate(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                Map(model, workItem);
            }

            OnUpdate();
        }

        private void OnWorkItemComplete(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                Map(model, workItem);
            }

            OnUpdate();
        }

        private void Map(LibraryWorkItemViewModel source, LibraryWorkItemViewModel dest)
        {
            if (source.Id != dest.Id)
                throw new ArgumentException("Trying to map mis-matching work items");

            dest.HasErrors = source.HasErrors;
            dest.InProgress = source.InProgress;
            dest.IsCompleted = source.IsCompleted;
            dest.LogMessages = source.LogMessages;
            dest.Output = source.Output;
            dest.Progress = source.Progress;
            dest.WorkSteps = source.WorkSteps;
        }

        public override void Initialize(Configuration configuration, NoViewModel load, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(Initialize, DispatcherPriority.Background, configuration, load, progressHandler);

            else
            {
                try
                {
                    var results = _audioStationDbClient.GetEntities<AcoustIDLookupResult>();

                    foreach (var result in results.GroupBy(x => x.MusicBrainzRecordingId))
                    {
                        this.WorkItems.Add(new LibraryWorkItemViewModel()
                        {
                            HasErrors = false,
                            InProgress = false,
                            IsCompleted = false,
                            Load = new LibraryLoaderEntitySetLoadViewModel<AcoustIDLookupResult>()
                            {
                                DisplayName = result.First().FileName,
                                EntitySet = new ObservableCollection<AcoustIDLookupResult>(result)
                            },
                            LoadType = LibraryLoadType.MusicBrainz,
                            Output = new LibraryLoaderEntitySetOutputViewModel<VendorTagSmall>(),
                            Progress = 0
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error initializing Library Loader component:  " + ex.Message);
                }

                OnUpdate();
            }
        }

        public override void Dispose()
        {

        }
    }
}

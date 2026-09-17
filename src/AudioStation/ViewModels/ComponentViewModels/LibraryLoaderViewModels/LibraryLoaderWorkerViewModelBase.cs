using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public abstract class LibraryLoaderWorkerViewModelBase : ViewModelBase
    {
        /// <summary>
        /// This is a simple / fast way to provide a unique identifier for each instance of the
        /// class. It is needed for handling callbacks from the ILibraryLoader.
        /// </summary>
        private static int WORKER_COUNTER = 0;

        private ILibraryLoader _libraryLoader;
        private List<LibraryLoaderLoad> _workLoads;

        int _id;
        string _name;
        string _description;
        bool _complete;
        bool _loaded;
        bool _working;

        KeyedObservableCollection<int, LibraryWorkItemViewModel> _workItems;

        int _workPendingCount;
        int _workInProgressCount;
        int _workSuccessCount;
        int _workCanceledCount;
        int _workErrorCount;
        double _totalProgress;

        // ILibraryLoader (current state)
        PlayStopPause _libraryLoaderState;

        // Blocker for preventing events during loading
        bool _updating;


        /// <summary>
        /// Executes when the library loader worker has changed status
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase> StatusChangeEvent;

        /// <summary>
        /// Executes when work item is updated
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, LibraryWorkItemViewModel> WorkItemChangedEvent;

        /// <summary>
        /// Event that fires when any of the UI properties of the work item are changed (e.g. IsSelected)
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, LibraryWorkItemViewModel> WorkItemUIChangedEvent;

        public int Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public bool Loaded
        {
            get { return _loaded; }
            set { this.RaiseAndSetIfChanged(ref _loaded, value); }
        }
        public bool Working
        {
            get { return _working; }
            set { this.RaiseAndSetIfChanged(ref _working, value); }
        }
        public bool Complete
        {
            get { return _complete; }
            set { this.RaiseAndSetIfChanged(ref _complete, value); }
        }
        public IEnumerable<LibraryWorkItemViewModel> WorkItems
        {
            get { return _workItems; }
        }
        public int WorkPendingCount
        {
            get { return _workPendingCount; }
            set { this.RaiseAndSetIfChanged(ref _workPendingCount, value); }
        }
        public int WorkInProgressCount
        {
            get { return _workInProgressCount; }
            set { this.RaiseAndSetIfChanged(ref _workInProgressCount, value); }
        }
        public int WorkSuccessCount
        {
            get { return _workSuccessCount; }
            set { this.RaiseAndSetIfChanged(ref _workSuccessCount, value); }
        }
        public int WorkCanceledCount
        {
            get { return _workCanceledCount; }
            set { this.RaiseAndSetIfChanged(ref _workCanceledCount, value); }
        }
        public int WorkErrorCount
        {
            get { return _workErrorCount; }
            set { this.RaiseAndSetIfChanged(ref _workErrorCount, value); }
        }
        public double TotalProgress
        {
            get { return _totalProgress; }
            set { this.RaiseAndSetIfChanged(ref _totalProgress, value); }
        }
        public PlayStopPause LibraryLoaderState
        {
            get { return _libraryLoaderState; }
            set { this.RaiseAndSetIfChanged(ref _libraryLoaderState, value); }
        }
        public string Status
        {
            get
            {
                if (!this.Loaded)
                    return "Not Loaded";

                else if (this.Complete)
                    return "Complete";

                else
                {
                    switch (this.LibraryLoaderState)
                    {
                        case PlayStopPause.Play:
                            return "Running";
                        case PlayStopPause.Pause:
                            return "Paused";
                        case PlayStopPause.Stop:
                            return "Stopped";
                        default:
                            throw new Exception("Unhandled library loader state");
                    }
                }
            }
        }

        public LibraryLoaderWorkerViewModelBase(string name, string description)
        {
            // UNIQUE IDENTIFIER!
            //
            this.Id = WORKER_COUNTER++;

            this.Name = name;
            this.Description = description;
            this.Working = false;

            _workItems = new KeyedObservableCollection<int, LibraryWorkItemViewModel>();
            _workLoads = new List<LibraryLoaderLoad>();

            _workItems.ItemPropertyChanged += OnWorkItemUIPropertyChanged;
        }

        protected abstract IEnumerable<LibraryLoaderLoad> CreateWorkLoads(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler);
        protected abstract LibraryLoaderLoadViewModel MapWorkLoad(LibraryLoaderLoad workLoad);
        protected abstract LibraryLoaderOutputViewModel MapWorkOutput(LibraryLoaderOutput workOutput);
        protected abstract LibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem);

        public void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.Loaded)
                throw new Exception("Library loader worker is already loaded");

            _libraryLoader = audioStationController.LibraryLoader;

            // ILibraryLoader Events!
            //
            // NOTE*** These are shared events! Each instance must verify that they are holding
            //         the work item that belongs to them; and that the ID is verified!
            //
            _libraryLoader.WorkItemComplete += OnWorkItemComplete;
            _libraryLoader.WorkItemUpdate += OnWorkItemUpdate;
            _libraryLoader.WorkItemQueued += OnWorkItemQueued;
            _libraryLoader.WorkItemCanceled += OnWorkItemCanceled;
            _libraryLoader.StateChangeEvent += OnStateChangeEvent;

            // BeginUpdate()
            _updating = true;

            // -> Inherited Class
            var libraryLoads = CreateWorkLoads(configuration, audioStationController, progressHandler);

            // EndUpdate()
            _updating = false;

            // Work loads are dispatched on Execute()
            foreach (var workLoad in libraryLoads)
                _workLoads.Add(workLoad);

            this.Loaded = true;

            OnUpdate();
        }

        protected void BeginUpdate()
        {
            _updating = true;
        }
        protected void EndUpdate()
        {
            _updating = false;
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new Exception("Loader task currently running. Please call 'CanExecute' first to verify it is finished.");

            // -> Play (execute)
            for (int index = _workLoads.Count - 1; index >= 0; index--)
            {
                var workLoad = _workLoads[index];

                // -> Updates via events
                _libraryLoader.QueueLoaderTask(workLoad);

                _workLoads.RemoveAt(index);
            }
        }
        public void RerunSelected()
        {
            if (!CanRerunSelected())
                throw new Exception("Loader task currently running. Please call 'CanRerunSelected' first to verify it is finished.");

            // Selected: These must be queried before modifying the queue due to events firing and updating the
            //           view - which is bound to IsSelected
            //
            var rerunItems = this.WorkItems.Where(x => x.IsSelected && (x.State == LibraryWorkItemState.Canceled ||
                                                                        x.State == LibraryWorkItemState.Error))
                                 .Actualize();


            foreach (var workItem in rerunItems)
            {
                if (_libraryLoader.IsTaskQueued(workItem.Id) ||
                    _libraryLoader.IsTaskRunning(workItem.Id))
                    continue;

                // Create next work load (only)
                var workLoad = ResetWorkLoad(workItem);

                // Reset Id (old)
                _workItems.Remove(workItem.Id);

                // Reset Work Item(s) (these data get set by backend updates)
                workItem.Id = _libraryLoader.QueueLoaderTask(workLoad);

                // -> (EVENT) Reset Id (new) (finished)
            }

            OnUpdate();
        }
        public void SkipSelected()
        {
            if (!CanSkipSelected())
                throw new Exception("Loader task currently running. Please call 'CanSkipSelected' first to verify it is finished.");

            // Selected: The events change the UI state. So, these have to be queried before they
            //           are modified.
            //
            var skippedItems = this.WorkItems
                                   .Where(x => x.IsSelected && x.State == LibraryWorkItemState.Pending)
                                   .Actualize();

            foreach (var workItem in skippedItems)
            {
                if (_libraryLoader.IsTaskQueued(workItem.Id))
                {
                    // Reset Work Item(s) (these data get set by backend updates)
                    _libraryLoader.DequeueTask(workItem.Id);
                }

                else if (_libraryLoader.IsTaskRunning(workItem.Id))
                {
                    _libraryLoader.CancelTask(workItem.Id);
                }
            }

            OnUpdate();
        }
        public void Reset()
        {
            _workItems.Clear();

            OnUpdate();
        }

        /// <summary>
        /// Attempts a state change of the loader service. This will not affect the front end workflow items except
        /// for whatever occurs in the proceeding state.
        /// </summary>
        public void ChangeState(PlayStopPause loaderState)
        {
            _libraryLoader.ChangeState(loaderState);
        }

        public bool CanExecute()
        {
            return this.Loaded &&
                  !this.Working &&
                   _workLoads.Any();
        }
        public bool CanRerunSelected()
        {
            return this.Loaded &&
                   this.LibraryLoaderState != PlayStopPause.Play &&
                   _workItems.Count > 0 &&
                   _workItems.Any(x => x.IsSelected && (x.State == LibraryWorkItemState.Canceled ||
                                                        x.State == LibraryWorkItemState.Error));
        }
        public bool CanSkipSelected()
        {
            return this.Loaded &&
                   this.LibraryLoaderState != PlayStopPause.Play &&
                   _workItems.Count > 0 &&
                   _workItems.Any(x => x.IsSelected && x.State == LibraryWorkItemState.Pending);
        }

        /// <summary>
        /// Updates work item counter properties
        /// </summary>
        protected void OnUpdate()
        {
            if (this.Loaded)
            {
                this.WorkPendingCount = _workItems.Count(x => x.State == LibraryWorkItemState.Pending);
                this.WorkInProgressCount = _workItems.Count(x => x.State == LibraryWorkItemState.Processing);
                this.WorkSuccessCount = _workItems.Count(x => x.State == LibraryWorkItemState.Successful);
                this.WorkErrorCount = _workItems.Count(x => x.State == LibraryWorkItemState.Error);
                this.WorkCanceledCount = _workItems.Count(x => x.State == LibraryWorkItemState.Canceled);
                this.TotalProgress = (this.WorkSuccessCount + this.WorkErrorCount + this.WorkCanceledCount) / (double)_workItems.Count;
                this.Working = this.WorkPendingCount > 0 || this.WorkInProgressCount > 0;
                this.Complete = this.WorkPendingCount == 0 && this.WorkInProgressCount == 0;

                OnPropertyChanged("Status");
            }
        }
        protected override void OnPropertyChanged(string name)
        {
            // Updating:  Prevent event raising during updates
            if (_updating)
                return;

            if (name != "Status")
                base.OnPropertyChanged(name);

            else
            {
                base.OnPropertyChanged(name);

                if (this.StatusChangeEvent != null)
                    this.StatusChangeEvent(this);
            }
        }

        private void OnStateChangeEvent(PlayStopPause state)
        {
            this.LibraryLoaderState = state;
        }
        private void OnWorkItemUpdate(LibraryLoaderWorkItemUpdate update)
        {
            if (update.OwnerId != this.Id)
                return;

            LibraryWorkItemViewModel workItem;

            // Update
            if (_workItems.ContainsKey(update.Id))
            {
                workItem = _workItems[update.Id];
            }

            // Add
            else
            {
                // NOTE*** No Load/Output! (this may be ok)(we mostly just visualize the work status)
                workItem = new LibraryWorkItemViewModel();

                workItem.Id = update.Id;
                workItem.LoadType = update.Type;

                _workItems.Add(workItem.Id, workItem);
            }

            LibraryLoaderHelpers.ApplyLibraryLoaderWorkItem(update, ref workItem);

            if (this.WorkItemChangedEvent != null)
                this.WorkItemChangedEvent(this, workItem);

            OnUpdate();
        }
        private void OnWorkItemQueued(LibraryLoaderWorkItem sender)
        {
            if (sender.GetOwnerId() == this.Id)
            {
                // This is essentially the same code (update/add)
                OnWorkItemComplete(sender);
            }
        }
        private void OnWorkItemCanceled(LibraryLoaderWorkItem sender)
        {
            if (sender.GetOwnerId() == this.Id)
            {
                // This is essentially the same code (update/add)
                OnWorkItemComplete(sender);
            }
        }
        private void OnWorkItemComplete(LibraryLoaderWorkItem complete)
        {
            if (complete.GetOwnerId() != this.Id)
                return;

            LibraryWorkItemViewModel workItem;

            // Update
            if (_workItems.ContainsKey(complete.GetId()))
            {
                workItem = _workItems[complete.GetId()];
            }

            // Add
            else
            {
                workItem = new LibraryWorkItemViewModel();

                workItem.Id = complete.GetId();
                workItem.LoadType = complete.GetLoadType();
                workItem.Load = MapWorkLoad(complete.GetWorkItem());
                workItem.Output = MapWorkOutput(complete.GetOutputItem());

                _workItems.Add(workItem.Id, workItem);
            }

            LibraryLoaderHelpers.ApplyLibraryLoaderWorkItem(complete, ref workItem);

            if (this.WorkItemChangedEvent != null)
                this.WorkItemChangedEvent(this, workItem);

            OnUpdate();
        }
        private void OnWorkItemUIPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Updating:  Prevent event raising during updates
            if (_updating)
                return;

            if (e.PropertyName != "IsSelected")
                return;

            if (this.WorkItemUIChangedEvent != null)
                this.WorkItemUIChangedEvent(this, sender as LibraryWorkItemViewModel);
        }
    }
}

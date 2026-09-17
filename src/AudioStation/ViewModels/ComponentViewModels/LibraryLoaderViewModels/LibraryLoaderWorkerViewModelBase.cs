using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service;

using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public abstract class LibraryLoaderWorkerViewModelBase : ViewModelBase
    {
        private ILibraryLoader _libraryLoader;
        private List<LibraryLoaderLoad> _workLoads;

        string _name;
        string _description;
        bool _isAllWorkComplete;
        bool _loaded;
        bool _working;

        KeyedObservableCollection<int, LibraryWorkItemViewModel> _workItems;

        int _queuedCount;
        int _inProgressCount;
        int _successCount;
        int _errorCount;
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
        public bool IsAllWorkComplete
        {
            get { return _isAllWorkComplete; }
            set { this.RaiseAndSetIfChanged(ref _isAllWorkComplete, value); }
        }
        public IEnumerable<LibraryWorkItemViewModel> WorkItems
        {
            get { return _workItems; }
        }
        public int QueuedCount
        {
            get { return _queuedCount; }
            set { this.RaiseAndSetIfChanged(ref _queuedCount, value); }
        }
        public int InProgressCount
        {
            get { return _inProgressCount; }
            set { this.RaiseAndSetIfChanged(ref _inProgressCount, value); }
        }
        public int SuccessCount
        {
            get { return _successCount; }
            set { this.RaiseAndSetIfChanged(ref _successCount, value); }
        }
        public int ErrorCount
        {
            get { return _errorCount; }
            set { this.RaiseAndSetIfChanged(ref _errorCount, value); }
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

                else if (this.Working)
                    return "Running";

                else if (this.TotalProgress >= 1)
                    return "Completed";

                else
                    return "Stopped";
            }
        }

        public LibraryLoaderWorkerViewModelBase(string name, string description)
        {
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

            // ILibraryLoader Events
            _libraryLoader.WorkItemComplete += OnWorkItemComplete;
            _libraryLoader.WorkItemUpdate += OnWorkItemUpdate;
            _libraryLoader.WorkItemQueued += OnWorkItemQueued;
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
            foreach (var workLoad in _workLoads)
                _libraryLoader.QueueLoaderTask(workLoad);
        }
        public void RerunSelected()
        {
            if (!CanExecute())
                throw new Exception("Loader task currently running. Please call 'CanExecute' first to verify it is finished.");

            // Selected
            foreach (var workItem in this.WorkItems.Where(x => x.IsSelected))
            {
                if (_libraryLoader.IsTaskQueued(workItem.Id) ||
                    _libraryLoader.IsTaskRunning(workItem.Id))
                    continue;

                // Create next work load (only)
                var workLoad = ResetWorkLoad(workItem);

                // Reset Work Item(s) (these data get set by backend updates)
                workItem.Id = _libraryLoader.QueueLoaderTask(workLoad);
            }

            OnUpdate();
        }
        public void SkipSelected()
        {
            // Selected
            foreach (var workItem in this.WorkItems.Where(x => x.IsSelected))
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
            // Reset must be done from loader component
            return this.Loaded &&
                  !this.Working &&
                   _workItems.Count > 0 &&
                   _workItems.Any(x => !x.IsCompleted && !x.InProgress);
        }

        /// <summary>
        /// Updates work item counter properties
        /// </summary>
        protected void OnUpdate()
        {
            if (this.Loaded)
            {
                this.QueuedCount = _workItems.Count(x => !x.InProgress && !x.IsCompleted);
                this.InProgressCount = _workItems.Count(x => x.InProgress);
                this.SuccessCount = _workItems.Count(x => !x.InProgress && x.IsCompleted && !x.HasErrors);
                this.ErrorCount = _workItems.Count(x => !x.InProgress && x.IsCompleted && x.HasErrors);
                this.TotalProgress = (this.SuccessCount + this.ErrorCount) / (double)_workItems.Count;
                this.Working = _workItems.Any(x => !x.IsCompleted && x.InProgress);
                this.IsAllWorkComplete = _workItems.All(x => x.IsCompleted);

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
            // This is essentially the same code
            OnWorkItemComplete(sender);
        }
        private void OnWorkItemComplete(LibraryLoaderWorkItem complete)
        {
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

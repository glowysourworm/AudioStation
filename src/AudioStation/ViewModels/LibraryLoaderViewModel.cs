using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryLoaderViewModel : ViewModelBase, IDisposable
    {
        private readonly IOutputController _outputController;
        private readonly ILibraryLoader _libraryLoader;

        #region Backing Fields (private)
        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItemsPending;
        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItemsProcessing;
        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItemsSuccess;
        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItemsError;

        ObservableCollection<LibraryWorkItemViewModel> _libraryWorkItemsSelected;

        LibraryWorkItemState _selectedLibraryWorkItemState;

        SimpleCommand _loadLibraryCommand;
        #endregion

        #region Properties (public)
        public PlayStopPause LibraryLoaderState
        {
            // These forward / receive state requests to/from the library loader
            get { return _libraryLoader.GetState(); }
            set { OnLibraryLoaderStateRequest(value); }
        }
        public LibraryWorkItemState SelectedLibraryWorkItemState
        {
            get { return _selectedLibraryWorkItemState; }
            set { this.RaiseAndSetIfChanged(ref _selectedLibraryWorkItemState, value); SetSelectedWorkItemCollection(); }
        }
        public KeyedObservableCollection<int, LibraryWorkItemViewModel> LibraryWorkItemsPending
        {
            get { return _libraryWorkItemsPending; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsPending, value); }
        }
        public KeyedObservableCollection<int, LibraryWorkItemViewModel> LibraryWorkItemsProcessing
        {
            get { return _libraryWorkItemsProcessing; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsProcessing, value); }
        }
        public KeyedObservableCollection<int, LibraryWorkItemViewModel> LibraryWorkItemsSuccess
        {
            get { return _libraryWorkItemsSuccess; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsSuccess, value); }
        }
        public KeyedObservableCollection<int, LibraryWorkItemViewModel> LibraryWorkItemsError
        {
            get { return _libraryWorkItemsError; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsError, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> LibraryWorkItemsSelected
        {
            get { return _libraryWorkItemsSelected; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsSelected, value); }
        }
        public SimpleCommand LoadLibraryCommand
        {
            get { return _loadLibraryCommand; }
            set { this.RaiseAndSetIfChanged(ref _loadLibraryCommand, value); }
        }
        #endregion

        [IocImportingConstructor]
        public LibraryLoaderViewModel(IOutputController outputController,
                                      ILibraryLoader libraryLoader,
                                      IConfigurationManager configurationManager)
        {
            _outputController = outputController;
            _libraryLoader = libraryLoader;

            // Filtering of the library loader work items
            _selectedLibraryWorkItemState = LibraryWorkItemState.Pending;

            this.LibraryWorkItemsPending = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);
            this.LibraryWorkItemsProcessing = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);
            this.LibraryWorkItemsSuccess = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);
            this.LibraryWorkItemsError = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);

            this.LibraryWorkItemsSelected = this.LibraryWorkItemsPending;

            this.LibraryLoaderState = libraryLoader.GetState();

            libraryLoader.WorkItemUpdate += OnWorkItemUpdate;
            libraryLoader.WorkItemCompleted += OnWorkItemCompleted;
            libraryLoader.WorkItemsAdded += OnWorkItemsAdded;
            libraryLoader.WorkItemsRemoved += OnWorkItemsRemoved;
            libraryLoader.ProcessingComplete += OnLibraryProcessingComplete;
            libraryLoader.ProcessingChanged += OnLibraryProcessingChanged;

            this.LoadLibraryCommand = new SimpleCommand(() =>
            {
                libraryLoader.LoadLibraryAsync(configurationManager.GetConfiguration().DirectoryBase);
                libraryLoader.Start();
            });
        }

        #region ILibraryLoader Events (these are all on the Dispatcher)
        private void OnWorkItemsRemoved(LibraryLoaderWorkItem[] workItems)
        {
            foreach (var workItem in workItems)
            {
                if (this.LibraryWorkItemsPending.ContainsKey(workItem.Id))
                    this.LibraryWorkItemsPending.RemoveByKey(workItem.Id);

                if (this.LibraryWorkItemsProcessing.ContainsKey(workItem.Id))
                    this.LibraryWorkItemsProcessing.RemoveByKey(workItem.Id);

                if (this.LibraryWorkItemsSuccess.ContainsKey(workItem.Id))
                    this.LibraryWorkItemsSuccess.RemoveByKey(workItem.Id);

                if (this.LibraryWorkItemsError.ContainsKey(workItem.Id))
                    this.LibraryWorkItemsError.RemoveByKey(workItem.Id);
            }
        }
        private void OnWorkItemsAdded(LibraryLoaderWorkItem[] workItems)
        {
            foreach (var workItem in workItems)
            {
                var item = new LibraryWorkItemViewModel()
                {
                    Id = workItem.Id,
                    FileName = workItem.FileName,
                    LoadType = workItem.LoadType,
                    ErrorMessage = workItem.ErrorMessage,
                    LoadState = workItem.LoadState
                };

                if (workItem.LoadState == LibraryWorkItemState.Pending)
                    this.LibraryWorkItemsPending.Add(item);

                if (workItem.LoadState == LibraryWorkItemState.Processing)
                    this.LibraryWorkItemsProcessing.Add(item);

                if (workItem.LoadState == LibraryWorkItemState.CompleteSuccessful)
                    this.LibraryWorkItemsSuccess.Add(item);

                if (workItem.LoadState == LibraryWorkItemState.CompleteError)
                    this.LibraryWorkItemsError.Add(item);
            }
        }
        private void OnWorkItemUpdate(LibraryLoaderWorkItem item)
        {
            // TRANSFER TO PROPER COLLECTION

            // Pending
            if (this.LibraryWorkItemsPending.ContainsKey(item.Id))
            {
                var workItem = this.LibraryWorkItemsPending[item.Id];

                // Remove
                if (workItem.LoadState != item.LoadState)
                    this.LibraryWorkItemsPending.Remove(workItem);

                workItem.LoadState = item.LoadState;
                workItem.ErrorMessage = item.ErrorMessage;

                // -> Processing
                if (workItem.LoadState == LibraryWorkItemState.Processing)
                    this.LibraryWorkItemsProcessing.Add(workItem);

                // -> Success
                else if (workItem.LoadState == LibraryWorkItemState.CompleteSuccessful)
                    this.LibraryWorkItemsSuccess.Add(workItem);

                // -> Error
                else if (workItem.LoadState == LibraryWorkItemState.CompleteError)
                    this.LibraryWorkItemsError.Add(workItem);
            }

            // Processing
            else if (this.LibraryWorkItemsProcessing.ContainsKey(item.Id))
            {
                var workItem = this.LibraryWorkItemsProcessing[item.Id];

                // Remove
                if (workItem.LoadState != item.LoadState)
                    this.LibraryWorkItemsProcessing.Remove(workItem);

                workItem.LoadState = item.LoadState;
                workItem.ErrorMessage = item.ErrorMessage;

                // -> Pending
                if (workItem.LoadState == LibraryWorkItemState.Pending)
                    this.LibraryWorkItemsPending.Add(workItem);

                // -> Success
                else if (workItem.LoadState == LibraryWorkItemState.CompleteSuccessful)
                    this.LibraryWorkItemsSuccess.Add(workItem);

                // -> Error
                else if (workItem.LoadState == LibraryWorkItemState.CompleteError)
                    this.LibraryWorkItemsError.Add(workItem);
            }

            // Success
            else if (this.LibraryWorkItemsSuccess.ContainsKey(item.Id))
            {
                var workItem = this.LibraryWorkItemsSuccess[item.Id];

                // Remove
                if (workItem.LoadState != item.LoadState)
                    this.LibraryWorkItemsSuccess.Remove(workItem);

                workItem.LoadState = item.LoadState;
                workItem.ErrorMessage = item.ErrorMessage;

                // -> Pending
                if (workItem.LoadState == LibraryWorkItemState.Pending)
                    this.LibraryWorkItemsPending.Add(workItem);

                // -> Processing
                else if (workItem.LoadState == LibraryWorkItemState.Processing)
                    this.LibraryWorkItemsProcessing.Add(workItem);

                // -> Error
                else if (workItem.LoadState == LibraryWorkItemState.CompleteError)
                    this.LibraryWorkItemsError.Add(workItem);
            }

            // Error
            else if (this.LibraryWorkItemsError.ContainsKey(item.Id))
            {
                var workItem = this.LibraryWorkItemsError[item.Id];

                // Remove
                if (workItem.LoadState != item.LoadState)
                    this.LibraryWorkItemsError.Remove(workItem);

                workItem.LoadState = item.LoadState;
                workItem.ErrorMessage = item.ErrorMessage;

                // -> Pending
                if (workItem.LoadState == LibraryWorkItemState.Pending)
                    this.LibraryWorkItemsPending.Add(workItem);

                // -> Processing
                else if (workItem.LoadState == LibraryWorkItemState.Processing)
                    this.LibraryWorkItemsProcessing.Add(workItem);

                // -> Success
                else if (workItem.LoadState == LibraryWorkItemState.CompleteSuccessful)
                    this.LibraryWorkItemsSuccess.Add(workItem);
            }
            else
                throw new Exception("Work item not contained in proper collection:  LibraryLoaderViewModel.cs");
        }
        private void OnWorkItemCompleted(LibraryLoaderWorkItem workItem)
        {
            // Handles status changes
            OnWorkItemUpdate(workItem);
        }
        private void OnLibraryProcessingChanged()
        {
            // Notify listeners. The getter draws from the library loader
            OnPropertyChanged("LibraryLoaderState");
        }
        private void OnLibraryProcessingComplete()
        {
            _outputController.AddLog("Library Loader processing complete", LogMessageType.General);
        }
        #endregion

        private void SetSelectedWorkItemCollection()
        {
            switch (this.SelectedLibraryWorkItemState)
            {
                case LibraryWorkItemState.Pending:
                    this.LibraryWorkItemsSelected = this.LibraryWorkItemsPending;
                    break;
                case LibraryWorkItemState.Processing:
                    this.LibraryWorkItemsSelected = this.LibraryWorkItemsProcessing;
                    break;
                case LibraryWorkItemState.CompleteSuccessful:
                    this.LibraryWorkItemsSelected = this.LibraryWorkItemsSuccess;
                    break;
                case LibraryWorkItemState.CompleteError:
                    this.LibraryWorkItemsSelected = this.LibraryWorkItemsError;
                    break;
                default:
                    throw new Exception("Unhandled LibraryWorkItemState:  LibraryLoaderViewModel.cs");
            }
        }

        private void OnLibraryLoaderStateRequest(PlayStopPause state)
        {
            switch (state)
            {
                case PlayStopPause.Play:
                    _libraryLoader.Start();
                    break;
                case PlayStopPause.Pause:
                case PlayStopPause.Stop:
                    _libraryLoader.Stop();
                    break;
                default:
                    throw new Exception("Unhandled play / stop / pause state:  MainViewModel.cs");
            }
        }

        public void Dispose()
        {
            if (_libraryLoader != null)
            {
                _libraryLoader.Dispose();
            }
        }
    }
}

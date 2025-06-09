using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
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
        NotifyingObservableCollection<LibraryWorkItemViewModel> _libraryCoreWorkItems;
        List<LibraryWorkItemViewModel> _libraryCoreWorkItemsUnfiltered;

        LibraryWorkItemState _selectedLibraryWorkItemState;

        int _libraryWorkItemCountPending;
        int _libraryWorkItemCountProcessing;
        int _libraryWorkItemCountSuccess;
        int _libraryWorkItemCountError;

        SimpleCommand _loadLibraryCommand;
        #endregion

        #region Properties (public)
        public int LibraryWorkItemCountPending
        {
            get { return _libraryWorkItemCountPending; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemCountPending, value); }
        }
        public int LibraryWorkItemCountProcessing
        {
            get { return _libraryWorkItemCountProcessing; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemCountProcessing, value); }
        }
        public int LibraryWorkItemCountSuccess
        {
            get { return _libraryWorkItemCountSuccess; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemCountSuccess, value); }
        }
        public int LibraryWorkItemCountError
        {
            get { return _libraryWorkItemCountError; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemCountError, value); }
        }
        public PlayStopPause LibraryLoaderState
        {
            // These forward / receive state requests to/from the library loader
            get { return _libraryLoader.GetState(); }
            set { OnLibraryLoaderStateRequest(value); }
        }
        public LibraryWorkItemState SelectedLibraryWorkItemState
        {
            get { return _selectedLibraryWorkItemState; }
            set { this.RaiseAndSetIfChanged(ref _selectedLibraryWorkItemState, value); SetLibraryCoreWorkItemsFilter(); }
        }
        public NotifyingObservableCollection<LibraryWorkItemViewModel> LibraryCoreWorkItems
        {
            get { return _libraryCoreWorkItems; }
            set { this.RaiseAndSetIfChanged(ref _libraryCoreWorkItems, value); }
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
            _libraryCoreWorkItemsUnfiltered = new List<LibraryWorkItemViewModel>();

            this.LibraryCoreWorkItems = new NotifyingObservableCollection<LibraryWorkItemViewModel>();
            this.LibraryLoaderState = libraryLoader.GetState();

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

        #region ILibraryLoader Events (some of these events are from the worker thread)
        private void OnLibraryLoaderWorkItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // These items our our view model items
            var item = sender as LibraryWorkItemViewModel;

            if (item != null && e.PropertyName == "LoadState")
            {
                var contains = this.LibraryCoreWorkItems.Contains(item);

                if (item.LoadState != this.SelectedLibraryWorkItemState && contains)
                    this.LibraryCoreWorkItems.Remove(item);

                else if (item.LoadState == this.SelectedLibraryWorkItemState && !contains)
                    this.LibraryCoreWorkItems.Add(item);

                UpdateLibraryWorkItemCounts();
            }
        }
        private void OnWorkItemsRemoved(LibraryLoaderWorkItem[] workItems)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnWorkItemsRemoved, DispatcherPriority.ApplicationIdle, workItems);
            else
            {
                foreach (var workItem in workItems)
                {
                    var removedItems = _libraryCoreWorkItemsUnfiltered.Remove(item => item.FileName == workItem.FileName);

                    foreach (var item in removedItems)
                    {
                        item.PropertyChanged -= OnLibraryLoaderWorkItemPropertyChanged;

                        if (this.LibraryCoreWorkItems.Contains(item))
                            this.LibraryCoreWorkItems.Remove(item);
                    }
                }

                UpdateLibraryWorkItemCounts();
            }
        }
        private void OnWorkItemsAdded(LibraryLoaderWorkItem[] workItems)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnWorkItemsAdded, DispatcherPriority.ApplicationIdle, workItems);
            else
            {
                foreach (var workItem in workItems)
                {
                    var item = new LibraryWorkItemViewModel()
                    {
                        FileName = workItem.FileName,
                        LoadType = workItem.LoadType,
                        ErrorMessage = workItem.ErrorMessage,
                        LoadState = workItem.LoadState
                    };

                    item.PropertyChanged += OnLibraryLoaderWorkItemPropertyChanged;

                    _libraryCoreWorkItemsUnfiltered.Add(item);

                    if (item.LoadState == this.SelectedLibraryWorkItemState)
                        this.LibraryCoreWorkItems.Add(item);
                }

                UpdateLibraryWorkItemCounts();
            }
        }
        private void OnWorkItemCompleted(LibraryLoaderWorkItem workItem)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnWorkItemCompleted, DispatcherPriority.ApplicationIdle, workItem);
            else
            {
                var item = _libraryCoreWorkItemsUnfiltered.FirstOrDefault(x => x.FileName == workItem.FileName);

                // -> Property Changed (sets the filtering)
                if (item != null)
                    item.LoadState = workItem.LoadState;

                else
                    _outputController.AddLog("Cannot find work item in local cache:  {0}", LogMessageType.General, LogLevel.Error, workItem.FileName);
            }
        }
        private void OnLibraryProcessingChanged(PlayStopPause oldState, PlayStopPause newState)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnLibraryProcessingChanged, DispatcherPriority.ApplicationIdle, oldState, newState);

            // Notify listeners. The getter draws from the library loader
            else
                OnPropertyChanged("LibraryLoaderState");
        }
        private void OnLibraryProcessingComplete()
        {
            _outputController.AddLog("Library Loader processing complete", LogMessageType.General);
        }
        #endregion

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
        private void SetLibraryCoreWorkItemsFilter()
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(SetLibraryCoreWorkItemsFilter, DispatcherPriority.ApplicationIdle);
            else
            {
                this.LibraryCoreWorkItems.Clear();

                foreach (var item in _libraryCoreWorkItemsUnfiltered)
                {
                    if (item.LoadState == this.SelectedLibraryWorkItemState)
                        this.LibraryCoreWorkItems.Add(item);
                }
            }
        }
        private void UpdateLibraryWorkItemCounts()
        {
            this.LibraryWorkItemCountError = _libraryCoreWorkItemsUnfiltered.Count(x => x.LoadState == LibraryWorkItemState.CompleteError);
            this.LibraryWorkItemCountSuccess = _libraryCoreWorkItemsUnfiltered.Count(x => x.LoadState == LibraryWorkItemState.CompleteSuccessful);
            this.LibraryWorkItemCountPending = _libraryCoreWorkItemsUnfiltered.Count(x => x.LoadState == LibraryWorkItemState.Pending);
            this.LibraryWorkItemCountProcessing = _libraryCoreWorkItemsUnfiltered.Count(x => x.LoadState == LibraryWorkItemState.Processing);
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

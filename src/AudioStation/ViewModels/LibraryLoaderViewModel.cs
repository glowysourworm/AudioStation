using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
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
        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItems;

        ObservableCollection<LibraryWorkItemViewModel> _libraryWorkItemsSelected;

        LibraryLoadType _selectedLibraryNewWorkItemType;

        SimpleCommand _runWorkItemCommand;
        #endregion

        #region Properties (public)
        public PlayStopPause LibraryLoaderState
        {
            // These forward / receive state requests to/from the library loader
            get { return _libraryLoader.GetState(); }
            set { OnLibraryLoaderStateRequest(value); }
        }
        public LibraryLoadType SelectedLibraryNewWorkItemType
        {
            get { return _selectedLibraryNewWorkItemType; }
            set { this.RaiseAndSetIfChanged(ref _selectedLibraryNewWorkItemType, value); }
        }
        public KeyedObservableCollection<int, LibraryWorkItemViewModel> LibraryWorkItems
        {
            get { return _libraryWorkItems; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItems, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> LibraryWorkItemsSelected
        {
            get { return _libraryWorkItemsSelected; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsSelected, value); }
        }
        public SimpleCommand RunWorkItemCommand
        {
            get { return _runWorkItemCommand; }
            set { this.RaiseAndSetIfChanged(ref _runWorkItemCommand, value); }
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
            this.LibraryWorkItems = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);

            this.LibraryWorkItemsSelected = this.LibraryWorkItems;

            this.LibraryLoaderState = libraryLoader.GetState();

            libraryLoader.WorkItemUpdate += OnWorkItemUpdate;
            libraryLoader.ProcessingUpdate += OnLibraryProcessingChanged;

            this.RunWorkItemCommand = new SimpleCommand(() =>
            {
                switch (this.SelectedLibraryNewWorkItemType)
                {
                    case LibraryLoadType.Mp3FileAddUpdate:
                        libraryLoader.LoadLibraryAsync(configurationManager.GetConfiguration().DirectoryBase);
                        break;
                    case LibraryLoadType.M3UFileAddUpdate:
                        libraryLoader.LoadRadioAsync(configurationManager.GetConfiguration().DirectoryBase);
                        break;
                    case LibraryLoadType.FillMusicBrainzIds:
                        libraryLoader.LoadMusicBrainzRecordsAsync();
                        break;
                    default:
                        throw new Exception("Unhandled work item type:  LibraryLoaderViewModel.cs");
                }

                libraryLoader.Start();
            });
        }

        #region ILibraryLoader Events (these are all on the Dispatcher)
        private void RefreshWorkItems(LibraryWorkItem newItem)
        {
            foreach (var workItem in _libraryLoader.GetIdleWorkItems().Union(new LibraryWorkItem[] {newItem}))
            {
                // May not be a new item (we're consolidating code)
                if (workItem == null)
                    continue;

                if (!this.LibraryWorkItems.ContainsKey(workItem.Id))
                {
                    this.LibraryWorkItems.Add(new LibraryWorkItemViewModel()
                    {
                        Id = workItem.Id,
                        HasErrors = workItem.HasErrors,
                        Progress = workItem.PercentComplete,
                        EstimatedCompletionTime = workItem.EstimatedCompletionTime,
                        FailureCount = workItem.FailureCount,
                        SuccessCount = workItem.SuccessCount,
                        LoadType = workItem.LoadType,
                        LoadState = workItem.LoadState,
                        LastMessage = workItem.LastMessage,
                    });
                }
                else
                {
                    var viewModel = this.LibraryWorkItems[workItem.Id];

                    viewModel.Progress = workItem.PercentComplete;
                    viewModel.HasErrors = workItem.HasErrors;
                    viewModel.FailureCount = workItem.FailureCount;
                    viewModel.SuccessCount = workItem.SuccessCount;
                    viewModel.EstimatedCompletionTime = workItem.EstimatedCompletionTime;
                    viewModel.LoadState = workItem.LoadState;
                    viewModel.LoadType = workItem.LoadType;
                    viewModel.LastMessage = workItem.LastMessage;
                }
            }
        }
        private void OnWorkItemUpdate(LibraryWorkItem workItem)
        {
            RefreshWorkItems(workItem);
        }

        private void OnLibraryProcessingChanged()
        {
            RefreshWorkItems(null);

            // Notify listeners. The getter draws from the library loader
            OnPropertyChanged("LibraryLoaderState");
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

        public void Dispose()
        {
            if (_libraryLoader != null)
            {
                _libraryLoader.Dispose();
            }
        }
    }
}
